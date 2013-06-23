using System;
using System.Collections.Generic;
using System.Text;

namespace Thetis.Plugin
{
	/// <summary>
	/// A class representing a message from a channel to a plugin.
	/// </summary>
    public class MessageData
    {

        private String lowerCaseMessage = "";
        private String message;
		
		/// <summary>
		/// Gets a lower case version of the message (without the bot nick if direct) for comparison purposes.
		/// </summary>
		/// <value>
		/// The lower case message.
		/// </value>
        public String LowerCaseMessage
        {
            get
            {
                return lowerCaseMessage;
            }
        }
		
		/// <summary>
		/// Gets or sets the message from the bot without the bot nick if the message is direct.
		/// </summary>
		/// <value>
		/// The message.
		/// </value>
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
		
		/// <summary>
		/// Gets or sets the user data of the sender.
		/// </summary>
		/// <value>
		/// The sender.
		/// </value>
        public User SentFrom { get; set; }
		
		/// <summary>
		/// Gets or sets the raw, unaltered message.
		/// </summary>
		/// <value>
		/// The raw message.
		/// </value>
        public String RawMessage { get; set; }
		
		/// <summary>
		/// Gets or sets the time the message was received.
		/// </summary>
		/// <value>
		/// The time received.
		/// </value>
        public DateTime TimeReceived { get; set; }
		
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Thetis.Plugin.MessageData"/> is directed at the bot.
        /// </summary>
        /// <value>
        /// <c>true</c> if directed at the bot; otherwise, <c>false</c>.
        /// </value>
        public bool Direct { get; set; }
		
		/// <summary>
		/// Gets or sets the channel from which the message originated.
		/// </summary>
		/// <value>
		/// The channel.
		/// </value>
        public String Channel { get; set; }
		
		public MessageData()
        {
            SentFrom = new User();
        }


    }
}
