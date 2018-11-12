using System;

namespace Messenger.Models
{
    [Serializable]
    public class Message
    {
        private User sender;
        private User recipient;
        private string message;

        public Message(User sender, User recipient, string message)
        {
            this.sender = sender;
            this.recipient = recipient;
            this.message = message;
        }

        public User Sender => sender;

        public string GetMessage => message;

        public User Recipient => recipient;
    }
}