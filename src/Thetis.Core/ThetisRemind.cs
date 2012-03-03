using System;
using Thetis.Plugin;
using System.Text;
using System.Collections.Generic;

namespace Thetis.Core
{
	
	public class Reminder
	{
		public String From {get; set;}
		public String To {get; set;}
		public DateTime DateSet {get; set;}
		public TimeSpan Duration {get; set;}
		public String Text {get; set;}
		public String Channel {get; set;}
		
		public Reminder(String whoFrom, String whoFor, DateTime whenSet, TimeSpan duration, String text, String channel)
		{
			From = whoFrom;
			To = whoFor;
			DateSet = whenSet;
			Duration = duration;
			Text = text;
			Channel = channel;
		}
		
	}
	
	public class ThetisRemind : IThetisPlugin
	{
		
		IThetisPluginHost host;
		public List<Reminder> reminders = new List<Reminder>();
		
		public ThetisRemind ()
		{
		}

		#region IThetisPlugin implementation
		public PluginResponse ChannelMessageReceived (MessageData message)
		{
			PluginResponse toReturn = new PluginResponse();
			
			if (message.Direct && message.Message.StartsWith("remind"))
			{
				toReturn.Claimed = true;
				String[] split = message.Message.Split(' ');
				if (split.Length < 6) 
				{
					host.SendToChannel(MessageType.Message, message.Channel,"Not enough arguments");
					return toReturn;
				}
				
				String whoFor = split[1];
				if (split[1].ToLower() == "me") whoFor = message.SentFrom.Nick;
				
				String whoFrom = message.SentFrom.Nick;
				if (whoFrom == whoFor) whoFrom = "you";
				
				double number = 0;
				if (!Double.TryParse(split[3],out number))
				{
					host.SendToChannel(MessageType.Message, message.Channel,"Number format invalid");	
					return toReturn;
				}
				
				TimeSpan time;
				switch (split[4].ToLower())
				{
					case "seconds":
					case "second":
						time = TimeSpan.FromSeconds(number);
						break;
					case "minutes":
					case "minute":
						time = TimeSpan.FromMinutes(number);
						break;
					case "hours":
					case "hour":
						time = TimeSpan.FromHours(number);
						break;
					case "days":
					case "day":
						time = TimeSpan.FromDays(number);
						break;
					case "century":
					case "millenium":
					case "eon":
					case "centuries":
					case "millenia":
					case "eons":
						host.SendToChannel(MessageType.Message, message.Channel, "That's optimistic.");
						return toReturn;
						
					default:
						host.SendToChannel(MessageType.Message, message.Channel, String.Format("Sorry I don't understand {0}", split[4]));
						return toReturn;
					
				}
				
				StringBuilder text = new StringBuilder();
				for (int i = 5; i < split.Length; i++) 
				{
					text.Append(split[i]);	
					text.Append(" ");
				}
				
				
				Reminder r = new Reminder(whoFrom, whoFor, DateTime.Now, time, text.ToString(), message.Channel);
				reminders.Add(r);
				if (whoFrom == "you") whoFor = "you";
				host.SendToChannel(MessageType.Message, message.Channel, String.Format("Ok {0}, I'll tell {1} that in {2}.", message.SentFrom.Nick, whoFor, Utilities.FormatTimespan(time)));
				
			}
			
			return toReturn;
		}

		public void Closing ()
		{
			
		}

		public void Tick ()
		{
			for (int i = reminders.Count - 1; i >= 0; i--) 
			{
				if (reminders[i].DateSet + reminders[i].Duration < DateTime.Now)
				{
					host.SendToChannel(MessageType.Message, 
					                   reminders[i].Channel, 
					                   String.Format("{0}, {1} asked me to remind you {2} ago, {3}", reminders[i].To, reminders[i].From, Utilities.FormatTimespan(reminders[i].Duration), reminders[i].Text));
					reminders.RemoveAt(i);
				}
			}
		}

		public void Init ()
		{
			//TODO Load reminds
		}

		public bool ForceSave ()
		{
			//TODO force saves
			return true;
		}

		public string GetHelp (string command)
		{
			return "Reminds a user of something after a period of time, example: remind <nick>/me in 5 minutes BLARGH.";
		}

		public IThetisPluginHost Host {
			set {
				host = value;
			}
		}

		public string Name {
			get {
				return "Remind";
			}
		}

		public bool IgnoreClaimed {
			get {
				return true;
			}
		}
		#endregion
	}
}

