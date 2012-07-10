using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Meebey.SmartIrc4net;
using System.Xml;
using Thetis.Plugin;
using System.IO;

namespace Thetis
{
	
	class Command
	{
		public IThetisPlugin Plugin	;
		public String Name;
		
		public Command(IThetisPlugin plugin, String name)
		{
			Plugin = plugin;
			Name = name;
		}
	}
	
    class Bot : IThetisPluginHost
    {
		
        public bool Connected { get; set; }
        public String Name { get; set; }
        public int Port { get; set; }
		System.Timers.Timer pluginTicker = new System.Timers.Timer();
				
        bool running = true;

        List<String> servers = new List<string>();
        List<String> channels = new List<string>();
        List<String> nicks = new List<string>();
        List<IThetisPlugin> plugins = new List<IThetisPlugin>();
        
		List<Command> commands = new List<Command>();
		
        IrcClient client;
        BotManager manager;
        
        public Bot(BotManager bm)
        {
            manager = bm;
        }
		
		public void ReloadPlugin(IThetisPlugin plugin)
		{
			manager.ReloadPlugin(plugin);
		}
		
		
        public void AddPlugin(IThetisPlugin plugin)
        {
			try 
			{
            	plugin.Host = this;
            	plugin.Init();
            	plugins.Add(plugin);
			}
			catch (Exception e)
			{
				manager.WriteToConsole(ConsoleColor.Red, "Plugin {0} failed to initialise. Error:{1}", plugin.Name, e.Message);	
			}
        }

		public void SortPlugins ()
		{
			plugins.Sort((plugin1,plugin2) =>
    			{	
        			return Comparer<Int32>.Default.Compare(plugin1.Priority, plugin2.Priority);
    			}
			);
		}

        public void AddServer(String address)
        {
            if (!servers.Contains(address)) servers.Add(address);
        }

        public void AddChannel(String channel)
        {
            if (!channels.Contains(channel)) channels.Add(channel);
        }

        public void AddNick(String nick)
        {
            if (!nicks.Contains(nick)) nicks.Add(nick);
        }

        public void BeginConnect()
        {
            Thread t = new Thread(new ThreadStart(Connect));
            t.Start();
        }

        public void Disconnect()
        {
            client.AutoReconnect = false;
            
            client.RfcQuit("Quitting");
            running = false;
            
        }
		
		void client_OnError (object sender, IrcEventArgs e)
		{
			manager.WriteToConsole(ConsoleColor.Red, e.Data.Message, null);
		}

        public void Connect()
        {
            client = new IrcClient();
            client.AutoRejoin = true;
            client.AutoRejoinOnKick = true;
            client.AutoRelogin = true;
            client.AutoRetry = true;
            client.AutoReconnect = true;
            client.OnConnected += new EventHandler(client_OnConnected);
			client.OnErrorMessage += new IrcEventHandler(client_OnError);
            client.OnJoin += new JoinEventHandler(client_OnJoin);
            client.OnChannelMessage += new IrcEventHandler(client_OnChannelMessage);
            client.OnDisconnected += new EventHandler(client_OnDisconnected);
            
            client.Connect(servers.ToArray(), Port);
            
            client.Login(nicks[0], "ThetisBot");
            client.RfcJoin(channels.ToArray());

			pluginTicker.Interval = 1000;
			pluginTicker.Elapsed += HandlePluginTickerElapsed;
			pluginTicker.Start();

            client.Listen();
            

        }

        void HandlePluginTickerElapsed (object sender, System.Timers.ElapsedEventArgs e)
        {
        	foreach (IThetisPlugin p in plugins)
                    {
                        p.Tick();
                    }
        }

        MessageData ConvertToPluginData(IrcMessageData data)
        {
            MessageData md = new MessageData();
            md.Channel = data.Channel;
            md.RawMessage = data.Message.Trim();
            md.Message = md.RawMessage;
            md.Direct = false;
            md.TimeReceived = DateTime.Now;
            md.SentFrom.Nick = data.Nick;
            md.SentFrom.UserMask = data.From;

            String message = md.RawMessage.ToLower();

            foreach (String nick in nicks)
            {
                String s = nick.ToLower();

                if (message.StartsWith(s + " ") ||
                    message.StartsWith(s + ", ") ||
                    message.StartsWith(s + ": "))
                {
                    int space = message.IndexOf(' ');
                    if (space != -1)
                    {
                        md.Message = md.RawMessage.Substring(space + 1);
                        md.Direct = true;
                        break;
                    }
                }
                else if (message.EndsWith(" " + s))
                {
                    int space = message.LastIndexOf(' ');
                    if (space != -1)
                    {
                        md.Message = md.RawMessage.Substring(0, space);
                        md.Direct = true;
                        break;
                    }
                }
            }

            return md;

        }

        void client_OnChannelMessage(object sender, IrcEventArgs e)
        {
            
            MessageData data = ConvertToPluginData(e.Data);

            runPlugins(data);
        
            manager.WriteToConsole("ChanMess {0}", e.Data.Message);
        }

        private void runPlugins(MessageData data)
        {
			if (data.Direct)
			{
				if (data.Message.ToLower() == "admin forcesave") // TODO: Factor this out elsewhere
				{   	
					client.SendMessage(SendType.Message, data.Channel, "Saving plugin data...");
					foreach (IThetisPlugin tp in plugins)
					{
						bool isGood = tp.ForceSave();
						if (!isGood) 
						{
							manager.WriteToConsole(ConsoleColor.Red, "Failed saving {0}", tp.Name);	
							client.SendMessage(SendType.Message, data.Channel, String.Format("Failed saving: {0}", tp.Name));
						}
						
					}
					return;
				}
                if (data.Message.ToLower().StartsWith("help")) // TODO: Factor this out elsewhere
				{   
					string[] split = data.Message.ToLower().Split(' ');
					if (split.Length == 1) 
					{
                        StringBuilder cmds = new StringBuilder();
						foreach (Command cmd in commands)
						{
							if (cmds.Length > 0) cmds.Append(", ");
                            cmds.Append(cmd.Name);
						}
							
						client.SendMessage(SendType.Message, data.Channel, String.Format("I know these commands:	{0}", cmds.ToString()));
					}
					else 
					{
						foreach (Command cmd in commands)
						{
							if (cmd.Name.ToLower().Trim() == split[1].ToLower().Trim()) client.SendMessage(SendType.Message, data.Channel, cmd.Plugin.GetHelp(split[1]));;
						}
						
					}
					
					return;
				}
			}
			
            bool claimed = false;
            foreach (IThetisPlugin tp in plugins)
            {
                if (claimed && tp.IgnoreClaimed) continue;
                try
                {
                    PluginResponse response = tp.ChannelMessageReceived(data);
                    if (response.Claimed) claimed = true;
                }
                catch (Exception e)
                {
                    client.SendMessage(SendType.Message, data.Channel, String.Format("Caught unhandled exception from: {0}. Exception Data: {1}", tp.Name, e.Message));
                }
                
                
            }
        }

        void client_OnDisconnected(object sender, EventArgs e)
        {
            Connected = false;
        }

        void client_OnReadLine(object sender, ReadLineEventArgs e)
        {
            manager.WriteToConsole(e.Line);
        }

        void client_OnJoin(object sender, JoinEventArgs e)
        {
			manager.WriteToConsole("{0} joined {1}.", e.Who, e.Channel);
			           
        }

        void client_OnConnected(object sender, EventArgs e)
        {
            Connected = true;
			manager.WriteToConsole(ConsoleColor.Yellow, "Connected", null);
        }

        public string ServerName
        {
            get { return Name; }
        }

        public void SendMessage(string message, string destination)
        {
            client.SendMessage(SendType.Message, destination, message);
        }

        public string GetPluginPath()
        {
            string path = manager.MainPath + "/plugins/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public void WriteToConsole(IThetisPlugin from, string message)
        {
			manager.WriteToConsole(ConsoleColor.Cyan, "Plugin - {0} - {1}", from.Name, message);
        }
	
		public void RegisterCommand (IThetisPlugin from, string command)
		{
			commands.Add(new Command(from, command));
		}
	
        public List<String> GetChannelUsers(string channel)
        {
            List<WhoInfo> wi = (List<WhoInfo>)client.GetWhoList("*");
            List<String> toReturn = new List<string>();
            for (int i = wi.Count - 1; i >= 0; i--)
            {
                if (wi[i].Channel.ToLower() == channel.ToLower()) toReturn.Add(wi[i].Nick);
            }
            return toReturn;
        }
        
        public void SendToChannel(MessageType type, string channelName, string message)
        {
            SendType sendType = SendType.Message; // Need to convert this since SendType resides in the IrcLib assembly
            if (type == MessageType.Action) sendType = SendType.Action;
            else if (type == MessageType.Notice) sendType = SendType.Notice;

            client.SendMessage(sendType, channelName, message);

        }

        public void Inject(string channel, string message)
        {
            client.SendMessage(SendType.Message, channel, message);
        }
    }
}
