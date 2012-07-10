using System;
using Thetis.Plugin;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;
namespace Thetis.Google
{
	public class ThetisGoogleSearch: IThetisPlugin
	{
		
		IThetisPluginHost host;
		
		public ThetisGoogleSearch ()
		{
		}
	
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "google") return "Gets the first result from google. Usage: google <search string>";
			return null;
		}

		String downloadString(string url)
		{
			
			StringBuilder sb = new StringBuilder();
			
			byte[] buf = new byte[8192];
			
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
	
			return sb.ToString();	
		}
		
		#region IThetisPlugin implementation
		
		public int Priority 
		{
			get { return 0;}
		}
		
		public PluginResponse ChannelMessageReceived (MessageData data)
		{
            PluginResponse toReturn = new PluginResponse();

			if (data.Direct && data.LowerCaseMessage.StartsWith("google"))
				{
					toReturn.Claimed = true;
					String searchString = "";
					if (data.Message.Length > 7) //more than just google + 1 space
                    { 
						searchString = data.Message.Substring(7).Trim();	
					}
					else 
                    {
                        host.SendToChannel(MessageType.Message, data.Channel, "Google for what?");
                        return toReturn;
                    }
						
					String urlTemplate = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=large&safe=active&q={0}&start=0";
 
        			String page = downloadString(string.Format(urlTemplate, searchString));
					
					JObject o = (JObject)JsonConvert.DeserializeObject(page); // TODO make this all better because it currently sucks
					JObject arr = (JObject)o["responseData"];
					JArray res = (JArray)arr["results"];
					JObject theResult = (JObject)res.First;
					String url = (String)theResult["unescapedUrl"];
					String title = (String)theResult["titleNoFormatting"];
					
					host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0} - {1}", url, title));
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
			host.RegisterCommand(this, "google");
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
			get {
				return "Google Search";
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

