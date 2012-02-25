using System;
using System.Collections.Generic;
using System.Text;
using Thetis.Plugin;

namespace Thetis
{
    public class ThetisTime : IThetisPlugin
    {

        IThetisPluginHost host;

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


        public PluginResponse ChannelMessageReceived(MessageData data)
        {
            PluginResponse toReturn = new PluginResponse();

            if (data.Direct && data.LowerCaseMessage == "time") { // TODO Add different time zones
                host.SendToChannel(MessageType.Message, data.Channel, DateTime.Now.ToShortTimeString());
                toReturn.Claimed = true;
            }
            return toReturn;
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
