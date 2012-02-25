using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Plugin
{
    public class MessageData
    {

        private String lowerCaseMessage = "";
        private String message;

        public String LowerCaseMessage
        {
            get
            {
                return lowerCaseMessage;
            }
        }

        public String Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                lowerCaseMessage = message.ToLower();
            }
        }

        public User SentFrom { get; set; }
        public String RawMessage { get; set; }
        public DateTime TimeReceived { get; set; }
        public bool Direct { get; set; }
        public String Channel { get; set; }

        public MessageData()
        {
            SentFrom = new User();
        }


    }
}
