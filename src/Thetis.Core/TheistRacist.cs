using System;
using Thetis.Plugin;
using System.Collections.Generic;
using System.IO;

namespace Thetis.Core
{
	public class TheistRacist : IThetisPlugin
	{
		public TheistRacist ()
		{
		}

		List<string> epithets = new List<string>();
		
		bool addEpithet(string epithet)
		{
			epithet = epithet.Trim().ToLower();
			if (epithets.Contains(epithet)) return false;
			else 
			{
				epithets.Add(epithet);
				save();
			}
			return true;
		}
		
		#region IThetisPlugin implementation
		public PluginResponse ChannelMessageReceived (MessageData data)
		{
            PluginResponse toReturn = new PluginResponse();

			if (data.Direct && data.LowerCaseMessage.StartsWith("epithet")) 
			{
				string[] split = data.LowerCaseMessage.Split(' ');
                if (split.Length == 1) host.SendToChannel(MessageType.Message, data.Channel, "You didnt supply an epithet.");
                else
                {
                    //throw new Exception("Test Exception");
                    //host.SendToChannel(MessageType.Message, data.Channel, "No, Drew fucks shit up. This is why you can't have nice things."); // TODO add admin support
                }
				/*
				else if (addEpithet(split[1]))
				{
					return "Ok I've added that you fucking racist.";
				}
				else return "That is already in the db.";
				*/
			}
			foreach(String s in epithets){
				if (data.LowerCaseMessage.Contains(s)) 
				{
					host.SendToChannel(MessageType.Message, data.Channel, String.Format("Shocking amounts of racism there from {0}", data.SentFrom.Nick));	
				}
			}
            return toReturn;
		}

		public void Closing ()
		{
			save();
		}

		public void Tick ()
		{
			
		}

		public void Init ()
		{
			String path = host.GetPluginPath() + "/epithets/";
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			if (File.Exists(path + "epithets.txt"))
			{
				StreamReader sr = new StreamReader(path + "epithets.txt");
				String line;
				while ((line = sr.ReadLine()) != null)
				{
					epithets.Add(line.Trim().ToLower());	
				}
				sr.Close();
			}
			host.RegisterCommand(this,"epithet");
		}
		
		bool save()
		{
			StreamWriter sw = null;
			bool success = true;
			try 
			{
				String path = host.GetPluginPath() + "/epithets/";
				if (!Directory.Exists(path)) Directory.CreateDirectory(path);
				sw = new StreamWriter(path + "epithets.txt",false);
				foreach(String s in epithets)
				{
					sw.WriteLine(s);	
				}
			}
			catch 
			{
				success = false;
			}
			finally
			{
				if (sw != null) sw.Close();
			}
			return success;
		}

		public bool ForceSave ()
		{
			return save ();
		}

		public string GetHelp (string command)
		{
			if (command == "epithet") return "Adds a new epithet to the db. Usage epithet <epithet>";
			return null;
				
		}
		
		IThetisPluginHost host;
		
		public IThetisPluginHost Host {
			set {
				host = value;
			}
		}

		public string Name {
			get {
				return "Racist";
			}
		}

		public bool IgnoreClaimed {
			get {
				return false;
			}
		}
		#endregion
	}
}

