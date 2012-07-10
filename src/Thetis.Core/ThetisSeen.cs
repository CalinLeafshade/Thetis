using System;
using System.Collections.Generic;
using System.Text;
using Thetis.Plugin;

namespace Thetis
{


    public class ThetisSeen : IThetisPlugin
    {
        Dictionary<String, DateTime> lastSeen = new Dictionary<string, DateTime>();
        Dictionary<String, String> lastSaid = new Dictionary<string, String>();

        IThetisPluginHost host;
		
		public int Priority 
		{
			get { return 0;}
		}
		
        public string Name
        {
            get { return "Seen"; }
        }
		
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "abbri") return "Gets when someone last spoke. Usage: seen <nickname>";
			return null;
		}
		
        public bool IgnoreClaimed
        {
            get { return false; }
        }

        String formatTime(TimeSpan ts) // TODO maybe make this a utility function of the host to keep it consistent
        {
            StringBuilder sb = new StringBuilder();
            if (ts.Days > 0) sb.Append(String.Format("{0} days", ts.Days));
            if (ts.Hours > 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(String.Format("{0} hours", ts.Hours));

            }
            if (ts.Minutes > 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(String.Format("{0} minutes", ts.Minutes));

            }
            if (ts.Seconds > 0)
            {
                if (sb.Length > 0) sb.Append(" and ");
                sb.Append(String.Format("{0} seconds", ts.Seconds));

            }
            sb.Append(" ago.");
            return sb.ToString();
        }

        public PluginResponse ChannelMessageReceived(MessageData data)
        {
            PluginResponse toReturn = new PluginResponse();

            if (data.Direct && data.LowerCaseMessage.StartsWith("seen "))
            {
                toReturn.Claimed = true;
                string[] split = data.LowerCaseMessage.Split(' ');
                if (split.Length < 2)
                {
                    host.SendToChannel(MessageType.Message, data.LowerCaseMessage, "You need to include a nick like this: Thetis seen Matt");
                    return toReturn;
                }

                String name = split[1];
                if (!lastSeen.ContainsKey(name.ToLower()))
                {
                    host.SendToChannel(MessageType.Message,data.Channel,String.Format("{0}, I havent seen {1}", data.SentFrom.Nick, name));
                }
                else
                {
                    TimeSpan ts = DateTime.Now.Subtract(lastSeen[name.ToLower()]);
                    String response = String.Format("{0}, I last saw {1} {2} saying {3}", data.SentFrom.Nick, name, formatTime(ts), lastSaid[name.ToLower()]);
                    host.SendToChannel(MessageType.Message, data.Channel, response);
                }
            }
            else
            {
                lastSeen[data.SentFrom.Nick.ToLower()] = DateTime.Now;
                lastSaid[data.SentFrom.Nick.ToLower()] = data.RawMessage;
            }
            return toReturn;
        }

        public void Closing()
        {
            //TODO save
        }


        public void Tick()
        {
            
        }


        public IThetisPluginHost Host
        {
            set { host = value; }
        }


        public void Init()
        {
            host.RegisterCommand(this, "seen");
        }
		
		public bool ForceSave()
		{
				return true;
		}
		
    }
}
