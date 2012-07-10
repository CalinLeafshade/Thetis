using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Plugin
{
	/// <summary>
	/// The type of message tobe sent to a channel.
	/// </summary>
    public enum MessageType { Message, Action, Notice };
	
	/// <summary>
	/// An interface for the plugin host for a bot.
	/// </summary>
    public interface IThetisPluginHost
    {
		/// <summary>
		/// Gets the name of the server associated with this bot.
		/// </summary>
		/// <value>
		/// The name of the server.
		/// </value>
        String ServerName { get; }
		
		/// <summary>
		/// Gets the root plugin path for plugin data.
		/// </summary>
		/// <returns>
		/// The path.
		/// </returns>
        String GetPluginPath();
		
		/// <summary>
		/// Sends a message to a channel.
		/// </summary>
		/// <param name='type'>
		/// The type of message.
		/// </param>
		/// <param name='channelName'>
		/// The channel to send the message to.
		/// </param>
		/// <param name='message'>
		/// The message.
		/// </param>
        void SendToChannel(MessageType type, String channelName, String message);
		
		/// <summary>
		/// Writes to the console.
		/// </summary>
		/// <param name='from'>
		/// The plugin sending the message.
		/// </param>
		/// <param name='message'>
		/// The message.
		/// </param>
        void WriteToConsole(IThetisPlugin from, String message);
		
		/// <summary>
		/// Registers a command with the bot.
		/// </summary>
		/// <param name='from'>
		/// The plugin which implementes the command.
		/// </param>
		/// <param name='command'>
		/// The command name.
		/// </param>
		void RegisterCommand(IThetisPlugin from, String command);
		
		/// <summary>
		/// Gets the channel users. (Not working)
		/// </summary>
		/// <returns>
		/// The channel users.
		/// </returns>
		/// <param name='channel'>
		/// The channel name.
		/// </param>
        List<String> GetChannelUsers(String channel); // FIXME
		
		void ReloadPlugin(IThetisPlugin plugin);
    }
}
