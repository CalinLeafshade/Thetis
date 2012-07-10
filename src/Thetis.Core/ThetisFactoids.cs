using System;
using System.Collections.Generic;
using System.Text;
using Thetis.Plugin;
using System.IO;
using System.Text.RegularExpressions;

namespace Thetis.Core
{
    public class ThetisFactoids : IThetisPlugin
    {

        //TODO Completely rewrite this class, it's a fucking mess.

        Dictionary<String, String> facts = new Dictionary<string, string>();
        DateTime lastSaved;
        IThetisPluginHost host;
		
		public string GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "forget") return "Forgets a factoid. Usage: forget <fact name>";
			return null;
			
		}
		
		public int Priority 
		{
			get 
			{
				return Int32.MaxValue;
			}
		}
		
        public string Name
        {
            get { return "Factoids";  }
        }

        public bool IgnoreClaimed
        {
            get { return true; }
        }


        public IThetisPluginHost Host
        {
            set { host = value; }
        }

		private String formatFact(String name, String fact, string sender)
		{
			
			String toReturn = "";
			if (!fact.Trim().ToLower().StartsWith("<reply>"))
			{
				toReturn += name + " is ";	
			}
			
			toReturn += fact;
			
			toReturn = toReturn.Replace("<reply>", "");
			toReturn = toReturn.Replace("!who", sender);
			toReturn = toReturn.Replace("$who", sender);
			
			return toReturn.Trim();
		}
		
        private void Save()
        {
            string path = host.GetPluginPath();
            path += "factoids/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += "factoids.dat";
           
            StreamWriter fs = new StreamWriter(path);
            foreach (KeyValuePair<String, String> kvp in facts)
            {
                fs.WriteLine("{0} is {1}", kvp.Key, kvp.Value);
            }
            fs.Close();
            host.WriteToConsole(this, "Saved.");
            lastSaved = DateTime.Now;
        }
		
		String hasFact(string message)
		{

			foreach (KeyValuePair<String, String> kvp in facts)
			{
				String regex = "";
				String[] split = kvp.Key.ToLower().Split(' ');
				foreach (string s in split)
				{
					if (regex.Length > 0) regex += "\\s";
					if (s.StartsWith("$") && s.Length > 1)  regex += "[\\w]+";
					else regex += Regex.Escape(s);
				}
					
                MatchCollection mc = Regex.Matches(message.ToLower(), regex);
                if (mc.Count > 0 && mc[0].Value == message.ToLower()) {
					string[] nameTokens = message.Split(' ');
					string fact = kvp.Value;
					for (int i = 0; i < split.Length; i++) 
					{
						if (split[i].StartsWith("$")) 
						{
							fact = fact.Replace(split[i], nameTokens[i]);	
						}
					}
                    if (fact.ToLower().StartsWith("<reply>")) fact = fact.Substring(7);
                    else if (fact.ToLower().StartsWith("<action>")) 
                    {
                        
                    }
                    else
                    {
                        fact = message + " is " + fact;
                    }
					return fact;
				}
			}
			return null;
		}
		
        private void LoadFacts()
        {
            string path = host.GetPluginPath();
            path += "factoids/factoids.dat";
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                String s;
                while ((s = sr.ReadLine()) != null)
                {
                    addFact(s);
                }
            }
            lastSaved = DateTime.Now;
        }

        string addFact(String message)
        {
            int start = message.IndexOf(" is ");
			String name = message.Substring(0, start);
                   
			String fact = message.Substring(start + 4);

            bool ok = false;
			String[] nameTokens = name.Split(' ');
			foreach(string s in nameTokens)
			{
				if (s.StartsWith("$") && s.Length > 1) 
				{
					if (!fact.Contains(s)) return String.Format("Token {0} missing in fact", s);
				}
                else if (!s.StartsWith("$")) ok = true;
			}
            if (!ok) return "You need at least one real word in the fact";
			if (facts.ContainsKey(name.ToLower())) return "That has already been defined.";
            facts[name.ToLower()] = fact;
            return String.Format("{0} has been defined", name);
        }

		bool removeFact(string fact)
		{
			if (facts.ContainsKey(fact.ToLower())) {
				facts.Remove(fact.ToLower());
				return true;
			}
			return false;
		}
		
        public PluginResponse ChannelMessageReceived(MessageData data)
        {
            PluginResponse toReturn = new PluginResponse();
            String fact;
            if (!data.Direct) return toReturn;

			if (data.LowerCaseMessage.StartsWith("forget"))
			{
                if (data.Message.Length > 7) 
				{
                    toReturn.Claimed = true;
					String message = data.Message.Substring(7);
					if (removeFact(message)) 
					{
						host.SendToChannel(MessageType.Message,data.Channel, String.Format("{0} forgotten.", message));	
					}
					else 
					{
						host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0} not defined.", message));	
					}
                    
				}
				
			}
   	        else if ((fact = hasFact(data.LowerCaseMessage)) != null)
            {
                
                fact = fact.Replace("$who", data.SentFrom.Nick);
                fact = fact.Replace("!who", data.SentFrom.Nick);
                fact = fact.Trim();
                if (fact.StartsWith("<action>"))
                {
                    if (fact.Length > 8) host.SendToChannel(MessageType.Action, data.Channel, fact.Substring(8));
                }
                else host.SendToChannel(MessageType.Message, data.Channel, fact);
                toReturn.Claimed = true;             
            }
            else if (data.LowerCaseMessage.Contains(" is "))
            {
				string response = addFact(data.Message);
                host.SendToChannel(MessageType.Message, data.Channel, response);
                toReturn.Claimed = true;
                
            }

            return toReturn;
        }

        public void Closing()
        {
        
        }

        public void Tick()
        {
            if (DateTime.Now.Subtract(lastSaved).TotalMinutes > 2) Save();
        }


        public void Init()
        {
            LoadFacts();
			host.RegisterCommand(this, "forget");
        }
		
		public bool ForceSave()
		{
			try
			{
				Save();	
			}
			catch 
			{
				return false;	
			}
			return true;
		}


    }
}
