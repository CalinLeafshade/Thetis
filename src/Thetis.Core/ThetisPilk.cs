using System;
using Thetis.Plugin;
using System.IO;
using System.Collections.Generic;

namespace Thetis.Core
{
	public class ThetisPilk :IThetisPlugin 
	{
		
		Random rand = new Random();
		
		public ThetisPilk ()
		{
		}
		
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "pilk") return "Gets a random Karl Pilkington quote. Usage: pilk";
			return null;
		}
		
		List<String> quotes = new List<String>();
		
		IThetisPluginHost host;
		
		#region IThetisPlugin implementation
		
		public int Priority 
		{
			get { return 0;}
		}
		
		public PluginResponse ChannelMessageReceived (MessageData data)
		{
            PluginResponse toReturn = new PluginResponse();

            if (data.LowerCaseMessage == "pilk")
            {
                host.SendToChannel(MessageType.Message, data.Channel, quotes[rand.Next(quotes.Count - 1)]);
                toReturn.Claimed = true;
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
			string path = host.GetPluginPath() + "/pilk/pilk.dat";
			TextReader tr = new StreamReader(path);
			String line;
			while((line = tr.ReadLine()) != null)
			{
				quotes.Add(line);
			}
			host.WriteToConsole(this, String.Format("Loaded {0} quotes", quotes.Count));
			host.RegisterCommand(this, "pilk");
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
				return "Pilk";
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

