using System;

namespace Messenger.Models
{
    [Serializable]
    public class User
    {
        private string name;
        private string username;

        public User(string name, string username)
        {
            this.name = name;
            this.username = username;
        }

        public string Name => name;

        public string Username => username;
    }
}