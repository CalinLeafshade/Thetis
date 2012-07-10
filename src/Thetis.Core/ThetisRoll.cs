using System;
using System.Collections.Generic;

using Thetis.Plugin;

namespace Thetis.Core
{
    public class ThetisRoll : IThetisPlugin
    {
        private const int MAX_DIFFERENT_DICE = 3;

        public ThetisRoll()
        {
        }

        private int rollDemBones(String diceStr)
        {
            string[] diceTypes = diceStr.ToLower().Split(new Char[] { 'd' });
            int numDie = Convert.ToInt32(diceTypes[0]);
            int dieType = Convert.ToInt32(diceTypes[1]);

            if (dieType < 2)
            {
                throw new ArgumentException("Thats just not possible.");
            }

            int total = 0;
            Random dice = new Random();
            for (int i = numDie; i > 0; i--)
            {
                total += dice.Next(dieType) + 1;
            }

            return total;
        }

        private List<int> doDiceRoll(MessageData data)
        {
            List<int> rollResults = new List<int>();
            string[] split = data.LowerCaseMessage.Split(' ');
            if (split.Length == 1)
            {
                throw new ArgumentException("Roll what?");
            }
            
            int len = split.Length;

            //+1 to take the command into account, which is the 1 item.
            if (len > MAX_DIFFERENT_DICE + 1)
            {
                throw new ArgumentException("Who needs that many dice?!");
            }

            for (int i = 1; i < len; i++)
            {
                rollResults.Add(rollDemBones(split[i]));
            }

            return rollResults;
        }

        private String formatOutput(List<int> results)
        {
            String individualRolls = "";
            if (results.Count > 1)
            {
                individualRolls = String.Join(", ", Array.ConvertAll(results.ToArray(), i => i.ToString()));
                individualRolls += ", ";
            }

            int total = 0;
            foreach (int r in results)
            {
                total += r;
            }
            
            return String.Format("{0}{1} {2}", individualRolls, (results.Count > 1) ? "Total" : "Rolled",  total);
        }

        #region IThetisPlugin implementation
		
		public int Priority 
		{
			get { return 0;}
		}
		
        public PluginResponse ChannelMessageReceived(MessageData data)
        {
            PluginResponse toReturn = new PluginResponse();

            if (data.Direct && data.LowerCaseMessage.StartsWith("roll"))
            {
                String msgStr;
                try
                {

                    List<int> rollResults = doDiceRoll(data);
                    msgStr = formatOutput(rollResults);
                }
                catch (ArgumentException e)
                {
                    msgStr = e.Message;
                }
                catch (Exception e)
                {
                    msgStr = "I don't have that, die.";
                }

                host.SendToChannel(MessageType.Message, data.Channel, String.Format("{0}: {1}", data.SentFrom.Nick, msgStr));
            }
            
            return toReturn;
        }


        IThetisPluginHost host;
        public IThetisPluginHost Host
        {
            set
            {
                host = value;
            }
        }

        public string Name
        {
            get { return "ThetisRoll"; }
        }

        public bool IgnoreClaimed
        {
            get { return false; }
        }

        public void Closing()
        {
            
        }

        public void Tick()
        {
            
        }

        public void Init()
        {
            host.RegisterCommand(this, "roll");
        }

        public bool ForceSave()
        {
            return true;
        }

        public string GetHelp(string command)
        {
            return String.Format("Roll dem bones, Thetis roll XdY to roll X Y sided dice. Roll multiple types of die, seperated by spaces. Supports up to {0} different dice at a time.", MAX_DIFFERENT_DICE );
        }
        #endregion
    }
}
