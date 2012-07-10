using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Plugin
{
	/// <summary>
	/// An Interface for plugins to the Thetis bot.
	/// </summary>
    public interface IThetisPlugin
    {
		/// <summary>
		/// Provides a reference on init to the <see cref="Thetis.Plugin.IThetisPluginHost"/> which initialised the plugin.
		/// </summary>
		/// <value>
		/// The IThetisPluginHost.
		/// </value>
        IThetisPluginHost Host { set; }
		
		/// <summary>
		/// Returns the plugin name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        String Name { get; }
		
		/// <summary>
		/// Gets the priority of the plugin.
		/// </summary>
		/// <value>
		/// The priority. Higher values mean a lower priority.
		/// </value>
		int Priority { get; }
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Thetis.Plugin.IThetisPlugin"/> ignores messages which have been claimed by other plugins.
		/// </summary>
		/// <value>
		/// <c>true</c> if the plugin should ignore claimed messages; otherwise, <c>false</c>.
		/// </value>
        bool IgnoreClaimed { get; }
		
		/// <summary>
		/// Called by the <see cref="Thetis.Plugin.IThetisPluginHost"/> when a message is pushed to the bot.
		/// </summary>
		/// <returns>
		/// A response value from the plugin.
		/// </returns>
		/// <param name='message'>
		/// The message data sent to the bot.
		/// </param>
        PluginResponse ChannelMessageReceived(MessageData message);
		
		/// <summary>
		/// Called when the bot is shutting down.
		/// </summary>
        void Closing();
		
		/// <summary>
		/// A one second interval tick from the bot.
		/// </summary>
        void Tick();
		
		/// <summary>
		/// Initialise this instance.
		/// </summary>
        void Init();
		
		/// <summary>
		/// Forces the plugin to save data.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the plugin successfully saved otherwise, <c>false</c>.
		/// </returns>
		bool ForceSave();
		
		/// <summary>
		/// Gets a help string for a particular command.
		/// </summary>
		/// <returns>
		/// The help string.
		/// </returns>
		/// <param name='command'>
		/// The command name as registered with the bot.
		/// </param>
		String GetHelp(String command);
		
    }
}
