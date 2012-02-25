using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Plugin
{

    public enum MessageType { Message, Action, Notice };

    public interface IThetisPluginHost
    {
        String ServerName { get; }
        String GetPluginPath();
        void SendToChannel(MessageType type, String channelName, String message);
        void WriteToConsole(IThetisPlugin from, String message);
		void RegisterCommand(IThetisPlugin from, String command);
        List<String> GetChannelUsers(String channel);
    }
}
