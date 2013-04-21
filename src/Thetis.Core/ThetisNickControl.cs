using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thetis.Plugin;

namespace Thetis.Core
{
    public class ThetisNickControl : IThetisPlugin
    {

        IThetisPluginHost host;

        public IThetisPluginHost Host
        {
            set { host = value; }
        }

        public string Name
        {
            get { return "NickControl"; }
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
            if (message.Direct)
            {
                if (message.LowerCaseMessage.StartsWith("nickstatus "))
                {
                    res.Claimed = true;
                    String[] split = message.LowerCaseMessage.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 1)
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "Status of whom?");
                    }
                    else
                    {
                        string nick = split[1];
                        NickservStatus s = host.GetNickservStatus(nick);
                        bool approved = host.IsNickApproved(nick);
                        bool isAdmin = host.IsAdmin(nick);
                        StringBuilder sb = new StringBuilder();
                        if (s == NickservStatus.NotRegisteredOrNotOnline)
                        {
                            sb.Append("Nickname is not registered, ");
                        }
                        else if (s == NickservStatus.NotIdentified)
                        {
                            sb.Append("Nickname is recognised but user is not idenfied, ");
                        }
                        else if (s == NickservStatus.RecognizedByAccessList || s == NickservStatus.RecognizedByPassword)
                        {
                           sb.Append("Nickname is recognised and user is idenfied, ");
                        }
                            
                        sb.AppendFormat("Nickname is{0} approved as a member", approved ? "" : " not");
                       
                        if (isAdmin)
                        {
                            sb.Append(", User is an admin");
                        }
                        sb.Append(".");

                        host.SendToChannel(MessageType.Message, message.Channel, sb.ToString());
                    }
                }
                else if (message.LowerCaseMessage.StartsWith("approve "))
                {
                    res.Claimed = true;
                    String[] split = message.LowerCaseMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 1)
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "Approve whom?");
                    }
                    else if (!host.IsAdmin(message.SentFrom.Nick))
                    {
                        host.SendToChannel(MessageType.Message, message.Channel, "You are not an admin!");
                    }
                    else
                    {
                        string nick = split[1];
                        bool approved = host.ApproveNick(nick);
                        if (approved)
                        {
                            host.SendToChannel(MessageType.Message, message.Channel, "Nick approved.");
                        }
                        else
                        {
                            host.SendToChannel(MessageType.Message, message.Channel, "Nick already approved.");
                        }
                    }
                }
            }
            return res;
        }

        public void Closing()
        {
           
        }

        public void Tick()
        {
           
        }

        public void Init()
        {
            host.RegisterCommand(this, "Approve");
            host.RegisterCommand(this, "NickStatus");
        }

        public bool ForceSave()
        {
            return true;
        }

        public string GetHelp(string command)
        {
            if (command == "Approve")
            {
                return "Approves a nick as a full member - Usage Approve <nick>";
            }
            else if (command == "NickStatus")
            {
                return "Gets the status of a nick in the DB - Usage NickStatus <nick>";
            }
            return null;
        }
    }
}
