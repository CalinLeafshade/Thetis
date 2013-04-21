using System;
using System.Collections.Generic;
using System.Text;
using Thetis.Plugin;
using System.Xml;
using System.IO;

namespace Thetis.Core
{

    public class Message
    {
        public String From;
        public String Text;
        public String For;
        public DateTime When;
        public bool Authorized;
        public bool Said;

        public override string ToString()
        {
            TimeSpan ts = DateTime.Now.Subtract(When);
            StringBuilder sb = new StringBuilder();
            if (ts.Days > 0) sb.Append(String.Format("{0} days", ts.Days));
            if (ts.Hours > 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(String.Format("{0} hours", ts.Hours));

            }
            if (ts.Minutes > 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(String.Format("{0} minutes", ts.Minutes));

            }
            if (ts.Seconds > 0)
            {
                if (sb.Length > 0) sb.Append(" and ");
                sb.Append(String.Format("{0} seconds", ts.Seconds));

            }
            sb.Append(" ago");
            return String.Format("{0}, {1} told me to tell you: \"{2}\" [{3}, {4}]", For, From, Text, sb.ToString(), Authorized ? "Authorized" : "Unauthorized");
        }

        public Message(String from, String ffor, String text, bool authorized)
        {
            From = from;
            For = ffor;
            Text = text;
            When = DateTime.Now;
            Authorized = authorized;
            Said = false;
        }

        public Message(XmlNode xml) 
        {
            foreach (XmlNode n in xml.ChildNodes)
            {
                switch (n.Name)
                {
                    case "From":
                        From = n.InnerText;
                        break;
                    case "For":
                        For = n.InnerText;
                        break;
                    case "Authorized":
                        Authorized = n.InnerText == "True";
                        break;
                    case "Text":
                        Text = n.InnerText;
                        break;
                    case "When":
                        When = DateTime.FromFileTime(Int64.Parse(n.InnerText));
                        break;
                    default:
                        break;
                }
            }
        }

        public void WriteTo(XmlWriter xml)
        {
            if (Said) return;
            xml.WriteStartElement("Message");
            xml.WriteStartElement("From");
            xml.WriteString(From);
            xml.WriteEndElement();
            xml.WriteStartElement("Text");
            xml.WriteString(Text);
            xml.WriteEndElement();
            xml.WriteStartElement("For");
            xml.WriteString(For);
            xml.WriteEndElement();
            xml.WriteStartElement("When");
            xml.WriteString(When.ToFileTime().ToString());
            xml.WriteEndElement();
            xml.WriteEndElement();
        }

    }

    public class ThetisTell :IThetisPlugin 
    {
		
		
		
        List<Message> messages = new List<Message>();
        DateTime lastSaved;

        public IThetisPluginHost Host
        {
            set { host = value; }
        }

        IThetisPluginHost host;

        public string Name
        {
            get { return "Tell"; }
        }

        public bool IgnoreClaimed
        {
            get { return true; }
        }

		public int Priority 
		{
			get { return 0;}
		}

        public PluginResponse ChannelMessageReceived(MessageData data)
        {

            PluginResponse toReturn = new PluginResponse();

            tell(data.SentFrom.Nick, data.Channel);

            if (data.Direct && data.LowerCaseMessage.StartsWith("nick "))
            {
                string[] spl = data.LowerCaseMessage.Split(' ');
                if (spl.Length > 1)
                {
                    NickservStatus s = host.GetNickservStatus(spl[1]);
                    host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0} is {1}", spl[1], s.ToString()));
                }
            }

            if (data.Direct && data.LowerCaseMessage.StartsWith("tell "))
            {
                toReturn.Claimed = true;
                String message = data.Message.Substring(5); // remove "tell "
                int space = message.IndexOf(' ');
                if (space > -1)
                {
                    String name = message.Substring(0, space);
                    String theTell = message.Substring(space + 1);
                    addMessage(data.SentFrom.Nick, name, theTell);
                    host.SendToChannel(MessageType.Message, data.Channel, String.Format("Ok {0}, I'll tell {1} that when i see them.", data.SentFrom.Nick, name));
                }
                else
                {
                    host.SendToChannel(MessageType.Message, data.Channel, "Tell who?");
                }
                
            }
            
            return toReturn;

        }

        /// <summary>
        /// Tells a user if they have any tells in the DB
        /// </summary>
        /// <param name="who">User who just spoke</param>
        /// <param name="channel">The relevant channel</param>
        private void tell(string who, string channel)
        {
            for (int i = 0, max = messages.Count; i < max; i++)
            {
                if (messages[i].For.ToLower() == who.ToLower() && !messages[i].Said)
                {
                    messages[i].Said = true;
                    host.SendToChannel(MessageType.Message, channel, messages[i].ToString());
                }
            }
            messages.RemoveAll(item => item.Said == true);
        }

        private void addMessage(string who, string name, string message)
        {
            bool au = host.GetNickservStatus(who) == NickservStatus.RecognizedByPassword;
            messages.Add(new Message(who, name, message, au));
            Save();
        }
		
		public String GetHelp(String command)
		{
			command = command.Trim().ToLower();
			if (command == "tell") return "Gives a user a message next time they speak. Usage: tell <nickname> <message>.";
			return null;
		}
		
        private void Save()
        {
            string path = host.GetPluginPath();
            path += "tells/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += "tells.xml";

            XmlWriter xml = new XmlTextWriter(path, null);
            xml.WriteStartDocument();
			xml.WriteStartElement("Tells");
            foreach (Message m in messages)
            {
                m.WriteTo(xml);
            }
			xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Close();
            host.WriteToConsole(this, "Saved.");
            lastSaved = DateTime.Now;
        }

        private void load()
        {
            string path = host.GetPluginPath();
            path += "tells/tells.xml";
            if (File.Exists(path))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
				
                foreach (XmlNode x in xml.FirstChild.ChildNodes)
                {
                    if (x.Name == "Message") messages.Add(new Message(x));
                }
            }
        }

        public void Closing()
        {
            
        }

        public void Tick()
        {
            
        }

        public void Init()
        {
            load();   
			host.RegisterCommand(this, "tell");
        }
		
		public bool ForceSave()
		{
			try 
			{
				Save();	
			}
			catch (Exception e)
			{
				return false;
			}
			return true;
		}
    }
}
