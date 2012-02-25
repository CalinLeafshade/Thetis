using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using Thetis.Plugin;

namespace Thetis
{
    class BotManager
    {
		
        Dictionary<String, Bot> bots = new Dictionary<string,Bot>();
        List<Type> plugins = new List<Type>();
        String mainPath;
        bool running;
        Listener listener = new Listener();

        public String MainPath
        {
            get { return mainPath; }
            set { mainPath = value; }
        }

        public MainConsole mConsole;

        public BotManager() 
        {
            listener.MessageReceived += new EventHandler<ListenerEvent>(listener_MessageReceived);
            listener.Listen();
        }

        void listener_MessageReceived(object sender, ListenerEvent e)
        {
            foreach (KeyValuePair<String, Bot> b in bots)
            {
                if (b.Value.Name == e.Server)
                {
                    b.Value.Inject(e.Channel, e.Message);
                }

            }
        }

        void loadGlobalSettings()
        {
            mainPath = Directory.GetCurrentDirectory();
        }

        

        void loadServers()
        {
            
            
            XmlDocument config = new XmlDocument();
            config.Load("servers.xml");
            foreach (XmlNode n in config.GetElementsByTagName("Server"))
            {
                Bot b = new Bot(this);
                foreach (XmlNode cn in n.ChildNodes)
                {
                    switch (cn.Name)
                    {
                        case "Name":
                            b.Name = cn.InnerText;
                            break;
                        case "Address":
                            b.AddServer(cn.InnerText);
                            break;
                        case "Port":
                            b.Port = Int32.Parse(cn.InnerText);
                            break;
                        case "Channel":
                            b.AddChannel(cn.InnerText);
                            break;
                        case "Nick":
                            b.AddNick(cn.InnerText);
                            break;
                        default:
                            break;

                    }
                }
                foreach (Type t in plugins)
                {
                    ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                    {
                        //blam
                    }
                    else b.AddPlugin((IThetisPlugin)ctor.Invoke(null));
                }
                bots.Add(b.Name, b);
            }
            
        }

        void loadPlugins()
        {
            String[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Thetis*.dll");
            foreach (String s in files)
            {
                Assembly assem = Assembly.LoadFile(s);
                foreach (Type t in assem.GetExportedTypes())
                {
                    if (t.IsClass)
                    {
                        foreach (Type iface in t.GetInterfaces())
                        {
                            
                            if (iface == typeof(IThetisPlugin))
                            {
                                WriteToConsole(ConsoleColor.Cyan, "Adding plugin {0}", t.Name);
								plugins.Add(t);
                                break;
                            }
                        }
                    }
                }

            }
        }

        void connectServers()
        {
            foreach (KeyValuePair<String, Bot> pair in bots)
            {
                pair.Value.BeginConnect();
            }
        }

        void killServers()
        {
            foreach (KeyValuePair<String, Bot> pair in bots)
            {
                pair.Value.Disconnect();
            }
        }
		
		public void WriteToConsole(String message)
		{
			WriteToConsole(ConsoleColor.DarkGray, message, null);
		}
		
		public void WriteToConsole(String message, params object[] obj)
		{
			WriteToConsole(ConsoleColor.DarkGray, message, obj);
		}
		
		public void WriteToConsole(ConsoleColor color, String message, params object[] obj)
		{
            
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				if (obj != null) message = String.Format(message, obj);
				ConsoleColor old = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.WriteLine(message);
				Console.ForegroundColor = old;
			}
			else 
			{
				mConsole.WriteLine(color, message, obj);	
			}
				
		}

        bool processInput(String input)
        {
            if (input.ToLower().Trim() == "quit")
            {
                running = false;
                return true;
            }
            

            return false;
        }

        public void Run(string[] args)
        {
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				Console.BackgroundColor = ConsoleColor.Black;
				Console.Clear();
				WriteToConsole("Using *nix, Falling back to simple console");

			}
			else
			{
            	mConsole = new MainConsole();
          		mConsole.Init();
			}

            loadPlugins();
            loadGlobalSettings();
            loadServers();
            connectServers();


            running = true;

          	if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				while (running)
            	{
					String input = Console.ReadLine();
                	Console.WriteLine("Command: {0}", input);
                    if (!processInput(input)) Console.WriteLine("ERROR: No such command");
                	
                	
				}

			}
			else 
			{
            	while (running)
            	{
                	String input = Console.ReadLine();
                	mConsole.WriteLine(ConsoleColor.Yellow, "Command: {0}", input);
                                   	
                	if (!processInput(input)) mConsole.WriteLine(ConsoleColor.Red, "ERROR: No such command",null);

            	}
			}

            killServers();

            
        }


    }
}
