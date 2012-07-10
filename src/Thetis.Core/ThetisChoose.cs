using System;
using Thetis.Plugin;

namespace Thetis.Core
{
	/// <summary>
	/// Chooses values from an ' or ' delimitered list.
	/// </summary>
	public class ThetisChoose : IThetisPlugin
	{
		public ThetisChoose ()
		{
		}

        Random random = new Random();

		#region IThetisPlugin implementation
		
		public int Priority 
		{
			get { return 0;}
		}
		
		public PluginResponse ChannelMessageReceived (MessageData data)
		{
            PluginResponse toReturn = new PluginResponse();

			if (data.Direct) 
			{
				if (data.LowerCaseMessage.StartsWith("choose"))
				{
                    toReturn.Claimed = true;
					if (data.LowerCaseMessage.Length > 6)
					{
						String[] opts = data.Message.Substring(7).Split(new String[] {" or "},StringSplitOptions.RemoveEmptyEntries );
						if (opts.Length > 0) 
						{
							host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0}, I choose {1}", data.SentFrom.Nick, opts[random.Next(opts.Length)]));
						}
					}
					else host.SendToChannel(MessageType.Message, data.Channel, "Choose what?");
				}
				
			}
            return toReturn;
		}

		public void Closing ()
		{
			
		}

		public void Tick ()
		{
			
		}

		public void Init ()
		{
            host.RegisterCommand(this, "choose");
		}

		public bool ForceSave ()
		{
			return true;
		}

		public string GetHelp (string command)
		{
			if (command.Trim().ToLower() == "choose") return "Choose returns a random value from an ' or ' delimitered list";
			return null;
		}
		
		IThetisPluginHost host;
		
		public IThetisPluginHost Host {
			set 
            {
				host = value;
			}
		}

		public string Name {
			get 
            {
				return "Choose";
			}
		}

		public bool IgnoreClaimed {
			get 
            {
				return true;
			}
		}
		#endregion
	}
}

