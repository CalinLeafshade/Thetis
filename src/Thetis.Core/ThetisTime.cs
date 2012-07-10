using System;
using System.Collections.Generic;
using System.Text;
using Thetis.Plugin;

namespace Thetis.Core
{
	
	public static class DateTimeUtils
	{
		
		static Dictionary<string,int> months;
		
		static DateTimeUtils()
		{
			months = new Dictionary<string, int>();
			months["january"] = 1;
			months["february"] = 2;
			months["march"] = 3;
			months["april"] = 4;
			months["may"] = 5;
			months["june"] = 6;
			months["july"] = 7;
			months["august"] = 8;
			months["september"] = 9;
			months["october"] = 10;
			months["november"] = 11;
			months["december"] = 12;
			
			months["jan"] = 1;
			months["feb"] = 2;
			months["mar"] = 3;
			months["apr"] = 4;
			months["may"] = 5;
			months["jun"] = 6;
			months["jul"] = 7;
			months["aug"] = 8;
			months["sept"] = 9;
			months["oct"] = 10;
			months["nov"] = 11;
			months["dec"] = 12;
		}
		
		static int yearFromString(string s)
		{
			int toReturn;
			if (s.Length > 2)
			{
				if (Int32.TryParse(s, out toReturn))
				{
					return toReturn;	
				}
			}
			return -1;
		}
		
		static int dayFromString(string s)
		{
			if (s.Length > 4) return -1;
			s = s.ToLower();
			
			if (s.EndsWith("st") || s.EndsWith("nd") || s.EndsWith("rd") || s.EndsWith("th")) 
			{
				s = s.Substring(0,s.Length - 2);
			}
			if (s.Length > 2) return -1;
			int toReturn;
			if (Int32.TryParse(s, out toReturn))
			{
				return toReturn;
			}
			else return -1;
		}
		
		static int monthFromString(string s)
		{
			s = s.ToLower();
			if (months.ContainsKey(s)) return months[s];
			else return -1;
		}
		
		public static DateTime DTFromString(string s)
		{
			DateTime toReturn;
			if (DateTime.TryParse(s, out toReturn)) // easy parse
			{
				return toReturn;	
			}
			else // hard parse
			{
				int d,m,y;
				d = m = y = -1;
				string[] split = s.Split(' ');
				foreach(string str in split)
				{
					if (d < 0) d = dayFromString(str);
					if (m < 0) m = monthFromString(str);
					if (y < 0) y = yearFromString(str);
				}
				if (d < 0) d = 1;
				if (m < 0) m = 1;
				if (y < 0) y = DateTime.Now.Year;
				toReturn = new DateTime(y,m,d);
				return toReturn;
			}
			return DateTime.MinValue;
		}

		
	}
	
	public class CalenderEntry
	{
		public String Name {get; set;}
		public String Channel {get; set;}
		public DateTime Time {get; set;}
		public bool Recurring {get; set;}
		public bool Notify {get; set;}
		
		public CalenderEntry() { }
		
		public CalenderEntry(string name, string channel, DateTime time, bool rec, bool not)
		{
			Name = name;
			Channel = channel;
			Time = time;
			Recurring = rec;
			Notify = not;
		}
		
		public static CalenderEntry FromString(String s, String channel)
		{
			CalenderEntry ce = new CalenderEntry();
			ce.Channel = channel;
			int needle = s.IndexOf(" as ");
			string dtstr = s.Substring(needle + " as ".Length);
			if (dtstr.IndexOf('[') > -1)
			{
				dtstr = dtstr.Substring(0, 	dtstr.IndexOf('['));
			}
			DateTime dt = DateTimeUtils.DTFromString(dtstr);
			ce.Name = s.Substring(0, needle);
			if (dt == DateTime.MinValue)
			{
				throw new Exception("Date Time Parse failed");	
			}
			ce.Time = dt;
			
			if (s.ToLower().Contains("[recurring]")) ce.Recurring = true;
			if (s.ToLower().Contains("[notify]")) ce.Notify = true;
			
			return ce;
			
			
		}
		
		public override string ToString ()
		{
			String n = Name;
			if (n.Length > 20) n = n.Substring(0,20);
			n = n.PadRight(20);
			string rec = Recurring ? "    Recurring" : "Not Recurring";
			string not = Notify ? "      Notify" : "Don't Notify";
			return String.Format("{0} - {1} - {2} - {3}", n, Time.ToShortDateString().PadRight(10), rec, not);
		}
	}
	
    public class ThetisTime : IThetisPlugin
    {

        IThetisPluginHost host;
		
		Dictionary<string,CalenderEntry> calender = new Dictionary<string, CalenderEntry>();

        public string Name
        {
            get { return "Time"; }
        }
		
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "time") return "Gets the time. Usage: time";
			return null;
		}
		
        public string GetResponse()
        {
            throw new NotImplementedException();
        }
		
		public void echoDT (DateTime dt, MessageData data)
		{
			if (dt == DateTime.MinValue) host.SendToChannel(MessageType.Message, data.Channel, "lol failed to parse");
			else if (dt < DateTime.Now)
			{
				TimeSpan ts = DateTime.Now - dt;
				String mess = String.Format("That passed {0} ago.", Utilities.FormatTimespan(ts));
				host.SendToChannel(MessageType.Message, data.Channel, mess);
			}
			else 
			{
				TimeSpan ts = dt - DateTime.Now;
				String mess = String.Format("You'll have to wait {0}", Utilities.FormatTimespan(ts));
				host.SendToChannel(MessageType.Message, data.Channel, mess);
				
			}
		}

        public PluginResponse ChannelMessageReceived(MessageData data)
        {
            PluginResponse toReturn = new PluginResponse();

            if (data.Direct) {
				if (data.LowerCaseMessage == "time") //TODO: add timezones
				{
                	host.SendToChannel(MessageType.Message, data.Channel, DateTime.Now.ToShortTimeString());
                	toReturn.Claimed = true;
				}
				else if (data.LowerCaseMessage.StartsWith("parsetime"))
		        {
					toReturn.Claimed = true;
					if (data.LowerCaseMessage.Length > 7)
					{
						DateTime dt = DateTimeUtils.DTFromString(data.LowerCaseMessage.Substring(7));
						if (dt == DateTime.MinValue) host.SendToChannel(MessageType.Message, data.Channel, "lol failed to parse");
						else host.SendToChannel(MessageType.Message, data.Channel, dt.ToLongDateString());
						
						
					}
				}
				else if (data.LowerCaseMessage.StartsWith("timetil"))
		        {
					toReturn.Claimed = true;
					if (data.LowerCaseMessage.Length > 7)
					{
						string[] split = data.LowerCaseMessage.Split(' ');
						if (calender.ContainsKey(split[1]))
						{
							echoDT(calender[split[1]].Time, data);
						}
						else 
						{
							DateTime dt = DateTimeUtils.DTFromString(data.LowerCaseMessage.Substring(7));
							echoDT(dt, data);
						}
						
						
					}
				}
				else if (data.LowerCaseMessage.StartsWith("setevent "))
				{
					toReturn.Claimed = true;
						
					if (!data.LowerCaseMessage.Contains(" as "))
					{
						host.SendToChannel(MessageType.Message, data.Channel, "Malformed message: Did not conatins 'as'");	
					}
					else
					{
						CalenderEntry ce = CalenderEntry.FromString(data.Message, data.Channel);
						int needle = data.Message.IndexOf(" as ");
						DateTime dt = DateTimeUtils.DTFromString(data.Message.Substring(needle + 4));
						string eventName = data.Message.Substring("setevent ".Length, needle - "setevent ".Length);
						if (dt != DateTime.MinValue)
						{
							calender[eventName] = new CalenderEntry(eventName, data.Channel, dt, false, false);
							host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0} set as {1}", eventName, dt.ToShortDateString()));
						}
					}
				}
				else if (data.LowerCaseMessage.StartsWith("listevents"))
				{
					toReturn.Claimed = true;
					if (calender.Count == 0) 
					{
						host.SendToChannel(MessageType.Message, data.Channel, "No entries in the calender");	
					}
					else 
					{
						foreach(KeyValuePair<string, CalenderEntry> ce in calender)
						{
							if ((ce.Value.Time > DateTime.Now || ce.Value.Recurring) && ce.Value.Channel == data.Channel)
							{
								host.SendToChannel(MessageType.Message, data.Channel, ce.Value.ToString());	
							}
						}
					}
					
				}
				        
					
					
            }
			
            return toReturn;
        }
		
		public int Priority 
		{
			get { return 0;}
		}

        public bool IgnoreClaimed
        {
            get { return true; }
        }


        public void Closing()
        {
            
        }


        public IThetisPluginHost Host
        {
            set { host = value; }
        }

        public void Tick()
        {
            
        }


        public void Init()
        {
            host.RegisterCommand(this, "time");
			
			
			
			
        }
    
		public bool ForceSave ()
		{
			return true;
		}
		
	}
}
