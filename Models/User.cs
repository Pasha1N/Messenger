using System;
using System.Net;

namespace Messenger.Models
{
    [Serializable]
    public class User
    {
        private IPAddress iPAddress;
        private int port;
        private string username;

        public User(IPAddress iPAddress, string username, int port)
        {
            this.iPAddress = iPAddress;
            this.username = username;
            this.port = port;
        }

        public IPAddress IPAddress => iPAddress;

        public string Username => username;

        public int Port => port;
    }
}