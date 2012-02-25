using System;
using Thetis.Plugin;
using System.Net;


namespace Thetis.Core
{
	public class ThetisSad : IThetisPlugin
	{
		public ThetisSad ()
		{
		}
		
		
		
		#region IThetisPlugin implementation
		public PluginResponse ChannelMessageReceived (MessageData data)
		{
            PluginResponse toReturn = new PluginResponse();

			if (data.Direct && data.LowerCaseMessage.StartsWith("sad "))
				{

                    toReturn.Claimed = true;

                    int firstSpace = data.Message.IndexOf(' ');
                    
                    if (firstSpace < 0)
                    {
                        host.SendToChannel(MessageType.Message, data.Channel, "Who's sad?");
                        return toReturn;
                    }

                    String nick = data.Message.Substring(firstSpace);

                    String sadString = new WebClient().DownloadString("http://nickclegglookingsad.tumblr.com/random");
					String startSearch = "<div class=\"copy\"><p>";
					String endSearch = "</p></div>";

					int start = sadString.IndexOf(startSearch);
                    if (start == -1)
                    {
                        host.SendToChannel(MessageType.Message, data.Channel, "Sorry Calin! Error!");
                        return toReturn;
                    }

					start += startSearch.Length;
					int end = sadString.IndexOf(endSearch, start);
					if (end == -1)
                    {
                        host.SendToChannel(MessageType.Message, data.Channel, "Sorry Calin! Error!");
                        return toReturn;
                    }

					string message = sadString.Substring(start, end - start);
                    message = message.Replace("Nick Clegg", nick);
                    message = message.Replace("Nick", nick);
                    message = message.Replace("&#8217;", "'");
                    message = message.Replace("</p>", "");
                    message = message.Replace("<em>", "");
                    message = message.Replace("</em>", "");

                    host.SendToChannel(MessageType.Message, data.Channel, message);
					
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
            host.RegisterCommand(this, "sad");
		}

		public bool ForceSave ()
		{
			return true;
		}

		public string GetHelp (string command)
		{
			return "Tells you why someone is sad. Usage: Thetis sad <nick>";
		}
		
		IThetisPluginHost host;
		
		public IThetisPluginHost Host {
			set {
				host = value;
			}
		}

		public string Name {
			get {
				return "Sad";
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

