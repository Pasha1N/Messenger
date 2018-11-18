using Messenger.Models;
using Messenger.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Client.ViewModels 
{
    public class MessageViewModel
    {
        private Message message;
        private string prepositionSender = string.Empty;
        private string prepositionRecipient = string.Empty;

        public MessageViewModel(Message message )
        {
            this.message = message;
        }

        public User Sender => message.Sender;

        public string GetMessage => message.GetMessage;

        public User Recipient => message.Recipient;

        public string PrepositionSender
        {
            get => prepositionSender;
            set => prepositionSender = value;
        }

        public string PrepositionRecipient
        {
            get => prepositionRecipient;
            set => prepositionRecipient = value;
        }
    }
}