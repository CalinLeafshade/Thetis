using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Plugin
{
    public interface IThetisPlugin
    {
        IThetisPluginHost Host { set; }
        String Name { get; }
		
        bool IgnoreClaimed { get; }
        PluginResponse ChannelMessageReceived(MessageData message);
        void Closing();
        void Tick();
        void Init();
		bool ForceSave();
		String GetHelp(String command);
		
    }
}
