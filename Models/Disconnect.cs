using System;

namespace Messenger.Models
{
    [Serializable]
    public class DisconnectUser
    {
        User user;

        public DisconnectUser(User user)
        {
            this.user = user;
        }

        public User WhomToDisconnect => user;

    }
}