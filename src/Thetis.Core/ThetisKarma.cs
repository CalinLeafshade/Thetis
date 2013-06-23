using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thetis.Plugin;

namespace Thetis.Core
{
    public class ThetisKarma : IThetisPlugin
    {

        int lastDay = -1;
        IThetisPluginHost host;
        Dictionary<string, int> karmaSpent = new Dictionary<string, int>();
        Dictionary<string, int> karma = new Dictionary<string, int>();
        int karmaLimit = 5;

        void newDay()
        {
            karmaSpent.Clear();
        }

        public IThetisPluginHost Host
        {
            set { host = value; }
        }

        public string Name
        {
            get { return "Karma"; }
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
            PluginResponse res = new PluginResponse();
            if (message.Direct && message.LowerCaseMessage.StartsWith("karma "))
            {
                res.Claimed = true;
                if (message.LowerCaseMessage.Trim() == "karma chameleon")
                {
                    host.SendToChannel(MessageType.Message, message.Channel, "Karma karma karma karma karma chameleon");
                    return res;
                }
                String[] split = message.LowerCaseMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 1)
                {
                    host.SendToChannel(MessageType.Message, message.Channel, "I need a command or a nickname");
                }
                else if (split[1] == "give")
                {
                    if (split.Length < 4)
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "Not enough arguments");
                    }

                    int amount;
                    if (!Int32.TryParse(split[3], out amount))
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "The amount doesnt seem to be a whole number.");
                    }
                    else if (!(host.IsNickApproved(message.SentFrom.Nick) && (host.GetNickservStatus(message.SentFrom.Nick) == NickservStatus.RecognizedByPassword)))
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "Your nickname is not approved or you have not idented with nickserv.");
                    }
                    else
                    {
                        string nick = split[2];
                        if (!karmaSpent.ContainsKey(message.SentFrom.Nick.ToLower())) karmaSpent[message.SentFrom.Nick.ToLower()] = 0;
                        if (karmaSpent[message.SentFrom.Nick.ToLower()] + Math.Abs(amount) > karmaLimit)
                        {
                            host.SendToChannel(MessageType.Message, message.Channel, String.Format("You dont have that much karma to spend left today. You have {0} left.", karmaLimit - karmaSpent[message.SentFrom.Nick.ToLower()]));
                        }
                        else
                        {
                            if (!karma.ContainsKey(nick)) karma[nick] = 0;
                            karma[nick] += amount;
                            karmaSpent[message.SentFrom.Nick.ToLower()] -= Math.Abs(amount);
                            host.SendToChannel(MessageType.Message, message.Channel, String.Format("You have changed {0}'s karma by {1}. It now stands at {2}", message.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2], amount, karma[nick]));
                        }

                    }

                }
                else
                {
                    string nick = message.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];

                    if (!karma.ContainsKey(nick.ToLower())) karma[nick.ToLower()] = 0;
                    int k = karma[nick.ToLower()];
                    host.SendToChannel(MessageType.Message, message.Channel, String.Format("{0}'s karma currently stands at {1}.", nick, k));
                }
            }
            return res;
        }

        public void Closing()
        {
            ForceSave();
        }

        public void Tick()
        {
            if (DateTime.Now.Day != lastDay)
            {
                newDay();
                
            }
            lastDay = DateTime.Now.Day;
        }

        public void Init()
        {
            host.RegisterCommand(this, "Karma");
            host.RegisterCommand(this, "Karma Give");
        }

        public bool ForceSave()
        {
            // @TODO actually make shit save
            return true;
        }

        public string GetHelp(string command)
        {
            if (command == "Karma")
            {
                return "Gets the current karma of a user - Usage: Karma <nick>";
            }
            if (command == "Karma Give")
            {
                return "Gives karma to a user (max of 5 a day, can be negative) - Usage Karma Give <nick> <amount>";
            }
            return null;
        }
    }
}
