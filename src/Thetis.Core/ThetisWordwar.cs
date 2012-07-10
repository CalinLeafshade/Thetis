using System;
using Thetis.Plugin;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Core
{
	
	public class WordWar
	{
		public DateTime Start;
		public TimeSpan Length;
		public String Starter;
		public bool Started;
		public String Channel;
		public bool Finished;
		
		public WordWar(String starter, int length, int timeToStart, String channel)
		{
			Channel = channel;
			Starter = starter;
			Length = TimeSpan.FromMinutes(length);
			Start = DateTime.Now + TimeSpan.FromMinutes(timeToStart);
			Started = false;
		}
		
	}
	
	public class ThetisWordwar : IThetisPlugin
	{
		
		public List<WordWar> Wars= new List<WordWar>();
		IThetisPluginHost host;
		
		public ThetisWordwar ()
		{
		}

		#region IThetisPlugin implementation
		
		public int Priority 
		{
			get { return 0;}
		}
		
		public PluginResponse ChannelMessageReceived (MessageData data)
		{

            PluginResponse toReturn = new PluginResponse();

			if (data.Direct && data.LowerCaseMessage.StartsWith("wordwar")){
                toReturn.Claimed = true;
                if (!data.Channel.Contains("belperscribbles")) {
                    host.SendToChannel(MessageType.Message, data.Channel, "No, not for you people");
                }

				String[] split = data.Message.Split(' ');

				if (split.Length < 3) {
                    host.SendToChannel(MessageType.Message, data.Channel, "Not enough parameters for a word war");
                    return toReturn;
                }

				int length = 0, timeToStart = 0;
				if(!Int32.TryParse(split[1], out length)) 
                {
                    host.SendToChannel(MessageType.Message, data.Channel, "First param not a number");
                    return toReturn;
                }
				if(!Int32.TryParse(split[2], out timeToStart))
                {
                    host.SendToChannel(MessageType.Message, data.Channel, "Second param not a number");
                    return toReturn;
                }
				if (length < 1 || length > 60) 
                {
                    host.SendToChannel(MessageType.Message, data.Channel, "Word war length is outside acceptable bounds (1 - 60)");
                    return toReturn;
                }
				if (timeToStart < 1 || timeToStart > 60) 
                {
                    host.SendToChannel(MessageType.Message, data.Channel, "Word war start time is outside acceptable bounds (1 - 60)");
                    return toReturn;
                }
                    
				Wars.Add(new WordWar(data.SentFrom.Nick, length, timeToStart, data.Channel));
				host.SendToChannel(MessageType.Message, data.Channel, String.Format("Ok {0}, your word war has been set!", data.SentFrom.Nick));
					         
			}
			return toReturn;
		}

		public void Closing ()
		{
			
		}

        String nickList(String channel)
        {
            List<String> nicks = host.GetChannelUsers(channel);
            StringBuilder sb = new StringBuilder();
            foreach (String s in nicks)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(s);
            }
            return sb.ToString();
        }

		public void Tick ()
		{

			foreach(WordWar w in Wars){

				TimeSpan timeTil = w.Start - DateTime.Now;
                if (timeTil.TotalSeconds < 5 && timeTil.TotalSeconds > -1 && !w.Started)
				{
                    host.SendToChannel(MessageType.Notice, w.Channel, String.Format("{0} your wordwar starts in {1} seconds!", w.Starter, (int)Math.Ceiling(timeTil.TotalSeconds)));
				}
				if (DateTime.Now > w.Start && !w.Started) {
                    //host.SendNotice(String.Format("Heather, zackkaufen, Hayley Your word war has started!", w.Starter), w.Channel);
                    host.SendToChannel(MessageType.Notice, w.Channel, "Heather, zackkaufen, Hayley Your word war has started!");				
					w.Started = true;	
				}
				if (w.Started && DateTime.Now > w.Start + w.Length) 
				{
					w.Finished = true;
                    host.SendToChannel(MessageType.Notice, w.Channel, "Heather, zackkaufen, Hayley Your word war has finished!");
				}
			}
			for (int i = Wars.Count - 1; i >= 0; i--) {
				if (Wars[i].Finished) Wars.RemoveAt(i);
			}
		
			
		}

		public void Init ()
		{
			host.RegisterCommand(this,"Wordwar");
		}

		public bool ForceSave ()
		{
			return true;
		}

		public string GetHelp (string command)
		{
			return "Usage: wordwar (length in minutes) (time till start in minutes)";
		}

		public IThetisPluginHost Host {
			set {
				host = value;
			}
		}

		public string Name {
			get {
				return "Word War";
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

