using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thetis.Plugin;
using System.Net;
using System.IO;
using Thetis.Core;
using System.Text.RegularExpressions;

namespace Thetis.Games
{

    public class SteamGame
    {

        public String Name = "";
        public TimeSpan TimePlayed = TimeSpan.Zero;
        public String Friendly = "";

        public SteamGame(string json)
        {
            string[] split = Regex.Split(json, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            foreach (string attr in split)
            {
                string stripped = attr.Replace("\"", "");
                int firstcolon = stripped.IndexOf(':');
                if (firstcolon < 0) continue;
                string[] kvp = new string[2] { stripped.Substring(0, firstcolon), stripped.Substring(firstcolon + 1, (stripped.Length - firstcolon) - 1) };
                if (kvp[0] == "name") Name = kvp[1];
                else if (kvp[0] == "hours_forever")
                {
                    
                    double hours = 0;
                    double.TryParse(kvp[1].Replace(",",""), out hours);
                    TimePlayed = TimeSpan.FromHours(hours);
                }
                else if (kvp[0] == "friendlyURL" && kvp[1] != "false")
                {
                    Friendly = kvp[1];
                }
            }
        }
    }

    public class SteamGameList
    {

        public List<SteamGame> list = new List<SteamGame>();

        static String downloadString(string url) // TODO Should possibly factor this into the core bot since its a common task
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

        public SteamGameList()
        {

        }

        public void AddGame(SteamGame game)
        {
            list.Add(game);
        }

        public SteamGame FindByName(string name)
        {
            foreach (SteamGame g in list) // first pass for identicals
            {
                if (g.Name.ToLower() == name.ToLower()) return g;
                if (g.Friendly.ToLower() == name.ToLower()) return g;
            }
            foreach (SteamGame g in list)
            {
                if (g.Name.ToLower().Contains(name.ToLower())) return g;
                if (g.Friendly.ToLower().Contains(name.ToLower())) return g;
            }
            return null;
        }

        public static SteamGameList FromUsername(string user)
        {
            SteamGameList sgl = new SteamGameList();
            String gamelist = downloadString(String.Format("http://steamcommunity.com/id/{0}/games?tab=all", user));
            if (gamelist.Contains("var rgGames = "))
            {
                int start = gamelist.IndexOf("var rgGames = ") + 14;
                int end = gamelist.IndexOf("\n", start);
                gamelist = gamelist.Substring(start, end - start);
                string[] games = gamelist.Split(new string[1]{"},{"}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string g in games)
                {
                    sgl.AddGame(new SteamGame(g));
                }
                return sgl;
            }
            else
            {
                return null;
            }
        }

    }

    public class ThetisSteam : IThetisPlugin
    {
        IThetisPluginHost host;
        public IThetisPluginHost Host
        {
            set { host = value; }
        }

        public string Name
        {
            get { return "Steam"; }
        }

        public int Priority
        {
            get { return 0; }
        }

        public bool IgnoreClaimed
        {
            get { return true; }
        }

        public PluginResponse ChannelMessageReceived(MessageData message)
        {
            PluginResponse pr = new PluginResponse();
            if (message.Direct && message.LowerCaseMessage.StartsWith("steam"))
            {
                pr.Claimed = true;
                String[] split = message.Message.Split(' ');
                if (split.Length > 2)
                {
                    string command = split[1];
                    SteamGameList sgl = SteamGameList.FromUsername(split[2]);
                    if (sgl == null)
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, String.Format("Couldn't find user {0}", split[2]));
                    }
                    else
                    {
                        switch (command.ToLower())
                        {
                            case "count":
                                host.SendToChannel(MessageType.Message, message.Channel, String.Format("{0} has {1} games", split[2], sgl.list.Count));
                                break;
                            case "random":
                                Random r = new Random();
                                SteamGame rg = sgl.list[r.Next(sgl.list.Count)];
                                host.SendToChannel(MessageType.Message, message.Channel, String.Format("How 'bout some {0}, {1}?", rg.Name, split[2]));
                                break;
                            case "totaltime":
                                TimeSpan time = TimeSpan.Zero;
                                foreach (SteamGame g in sgl.list)
                                {
                                    time += g.TimePlayed;
                                }
                                host.SendToChannel(MessageType.Message, message.Channel, String.Format("{0} has played steam games for {1}", split[2], Utilities.FormatTimespan(time)));
                                break;
                            case "time":
                                if (split.Length > 3)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    for (int i = 3; i < split.Length; i++)
                                    {
                                        sb.Append(split[i]);
                                        sb.Append(" ");
                                    }
                                    SteamGame game = sgl.FindByName(sb.ToString().Trim());
                                    if (game != null)
                                    {
                                        host.SendToChannel(MessageType.Message, message.Channel, String.Format("You have played {0} for {1}", game.Name, Utilities.FormatTimespan(game.TimePlayed)));
                                    }
                                    else
                                    {
                                        host.SendToChannel(MessageType.Message, message.Channel, String.Format("Couldn't find {0}", sb.ToString()));
                                    }
                                }
                                else
                                {
                                    host.SendToChannel(MessageType.Message, message.Channel, "Not enough parameters");
                                }
                                break;
                            default:
                                host.SendToChannel(MessageType.Message, message.Channel, String.Format("{0} is not an accepted command, {1}", split[1], message.SentFrom.Nick));
                                break;
                        }
                    }
                }
                else
                {
                    host.SendToChannel(MessageType.Message, message.Channel, "Not enough parameters");
                }
            }
            return pr;
        }

        public void Closing()
        {
            
        }

        public void Tick()
        {
            
        }

        public void Init()
        {
            
        }

        public bool ForceSave()
        {
            return true;
        }

        public string GetHelp(string command)
        {
            return "TODO Steam stuff";
        }
    }
}
