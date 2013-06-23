using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Thetis.Plugin;
using System.IO;

namespace Thetis
{

    
    class Nickserv
    {

        Bot bot;

        List<String> approvedNicks = new List<string>();
        Dictionary<string, NickservStatus> statuses = new Dictionary<string, NickservStatus>();
        List<String> nicks = new List<string>();

        public Nickserv(Bot bot)
        {
            this.bot = bot;
            loadNicks();
            Timer timer = new Timer(5000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            QueryNickServ();
        }

        public void QueryNickServ()
        {

            foreach (String s in nicks)
            {
                bot.SendMessage("STATUS " + s, "nickserv");
            }

            nicks.Clear();
            bot.SendMessage("list *", "nickserv");
        }

        public bool ApproveNick(string nick)
        {
            if (!approvedNicks.Contains(nick))
            {
                approvedNicks.Add(nick.ToLower());
                save();
                return true;
            }
            return false;
        }

        private void loadNicks()
        {
            string path = bot.GetPluginPath();
            path += "\\approvedNicks.conf";
            int count = 0;
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                String s;
                
                while ((s = sr.ReadLine()) != null)
                {
                    approvedNicks.Add(s);
                    count++;
                }
                sr.Close();
            }
            bot.WriteToConsole(null, String.Format("Loaded {0} approved nicks", count));
        }

        private void save()
        {
            string path = bot.GetPluginPath();
            path += "\\approvedNicks.conf";
            StreamWriter fs = new StreamWriter(path);
            foreach (String s in approvedNicks)
            {
                fs.WriteLine(s);
            }
            fs.Close();
        }

        public bool IsNickApproved(string nick)
        {
            return approvedNicks.Contains(nick.ToLower());
        }

        public void QueryReceived(string msg)
        {
            String[] split = msg.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (split[0] == "STATUS")
                {
                    int status;
                    if (Int32.TryParse(split[2], out status))
                    {
                        statuses[split[1].ToLower()] = (NickservStatus)status;
                    }
                }
                if (split[1] == "[Hidden]")
                {
                    nicks.Add(split[0]);
                }
            }
            catch
            {

            }
        }

        public NickservStatus GetStatus(string nick)
        {
            if (statuses.ContainsKey(nick.ToLower()))
            {
                return statuses[nick.ToLower()];
            }
            return NickservStatus.NotRegisteredOrNotOnline;
        }


    }
}
