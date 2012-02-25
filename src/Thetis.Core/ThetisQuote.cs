using System;
using Thetis.Plugin;
using System.Net;
using System.Text;
using System.IO;
namespace Thetis.Core
{
	public class ThetisQuote : IThetisPlugin
	{
		
		IThetisPluginHost host;
		HttpWebRequest requester;
		
		public ThetisQuote ()
		{
			
		}
	
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "quote") return "Gets a quote from the Sanctuary qdb. Usage: quote <opt. search string>";
			return null;
		}
		
		
		bool addQuote(string who, string quote)
		{
				return false;
		}
		
		
		String getQuote(String searchString) // TODO make a webrequest interface bot-wide
		{
			
			
			StringBuilder sb  = new StringBuilder();

			byte[]        buf = new byte[8192];
	
			String url = "http://irc.thethoughtradar.com/qdb/thetis.php";
			if (searchString != null && searchString != "") {
				searchString = searchString.Replace(" ", "%20");	
				url += "?s=" + searchString;
			}
			
			HttpWebRequest  request  = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream resStream = response.GetResponseStream();
	
			string tempString = null;
			int count = 0;
	
			do
			{
				count = resStream.Read(buf, 0, buf.Length);
				if (count != 0)
				{
					// translate from bytes to ASCII text
					tempString = Encoding.ASCII.GetString(buf, 0, count);
	
					sb.Append(tempString);
				}
			}
			while (count > 0); 
	
			String toReturn = sb.ToString(); // TODO maybe make a HTML response class too to filter this out.
			toReturn = toReturn.Replace("&lt;", "<");
			toReturn = toReturn.Replace("&gt;", ">");
			toReturn = toReturn.Replace("\\'", "'");
			toReturn = toReturn.Replace("\\\"", "\"");
			//toReturn = toReturn.Replace("\\n", " \\\\ ");
			return toReturn;
			
		}
		
		#region IThetisPlugin implementation
		public PluginResponse ChannelMessageReceived (MessageData data)
		{
            PluginResponse toReturn = new PluginResponse();

			if (data.Direct)
			{
				if (data.LowerCaseMessage.StartsWith("quote"))
				{
                    toReturn.Claimed = true;
					String searchString = "";
					if (data.LowerCaseMessage.Length > 6) { //more than just quote + 1 space
						searchString = data.Message.Substring(6).Trim();	
					}
					String s = getQuote(searchString);
                    if (s.Trim() == "")
                    {
                        host.SendToChannel(MessageType.Message, data.Channel, "No quotes found");
                    }
                    else
                    {
                        String[] lines = s.Split(new String[] { "\\n" }, StringSplitOptions.None);
                        foreach (String line in lines)
                        {
                            host.SendToChannel(MessageType.Message, data.Channel, line);
                        }
                        host.SendToChannel(MessageType.Message, data.Channel, "End Quote");
                    }
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
			host.RegisterCommand(this, "quote");
		}

		public bool ForceSave ()
		{
			return true;
		}

		public IThetisPluginHost Host {
			set 
			{
				host = value;
			}
		}

		public string Name {
			get 
			{
				return "Quotes";
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

