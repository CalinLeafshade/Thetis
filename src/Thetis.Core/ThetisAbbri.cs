using System;
using Thetis.Plugin;
using System.Text;
using System.Net;
using System.IO;

namespace Thetis.Core
{
	public class ThetisAbbri : IThetisPlugin
	{
		public ThetisAbbri ()
		{
		}
		
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "abbri") return "Gets an abbriviation from abbriviations.com. Usage: abbri <abbriviation>";
			return null;
		}
		
		IThetisPluginHost host;
		
		String downloadString(string url) // TODO Should possibly factor this into the core bot since its a common task
		{
			
			StringBuilder sb = new StringBuilder();
			
			byte[] buf = new byte[8192];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
	
			return sb.ToString();	
		}
		
		#region IThetisPlugin implementation
		public PluginResponse ChannelMessageReceived (MessageData data)
		{

            PluginResponse toReturn = new PluginResponse();

			String start = "<td class=dsc align=left>";
			String end = "</td>";
			if (data.Direct) 
			{
				if (data.LowerCaseMessage.StartsWith("abbri") && data.LowerCaseMessage.Length > 5)
                {
                    toReturn.Claimed = true;
                    String query = data.Message.Substring(6);
					int i = query.IndexOf(' ');
					if (i > -1) 
					{
						query = query.Substring(0,i);	
					}
					String search = downloadString(String.Format("http://www.abbreviations.com/{0}", query));
					
					int startNeedle = search.IndexOf(start);
                    if (startNeedle == -1)
                    {
                        host.SendToChannel(MessageType.Message, data.Channel, "Error searching");
                        return toReturn;
                    }
					int endNeedle = search.IndexOf(end, startNeedle + start.Length);
					String desc = search.Substring(startNeedle + start.Length, endNeedle - (startNeedle + start.Length));
					host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0} is {1}", query, desc));
					
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
			host.RegisterCommand(this, "abbri");
		}

		public bool ForceSave ()
		{
			return true;
		}

		public IThetisPluginHost Host {
			set {
				host = value;
			}
		}

		public string Name {
			get 
            {
				return "Abbri";
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

