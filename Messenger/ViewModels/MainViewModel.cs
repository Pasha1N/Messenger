using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Messenger.Client.Utilities;

namespace Messenger.Client.ViewModels
{
   public class MainViewModel: EventINotifyPropertyChanged
    {
        private static readonly EventWaitHandle serverStartedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "{94BF7448-1BF3-4A4A-A309-B328E02689FC}");
        private IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 2020);
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private string myName;
        public MainViewModel()
        {
            Connect();
        }

        public string MyName
        {
            get => myName;
            set => SetProperty(ref myName, value);
        }

        public bool Connect()
        {
            serverStartedEvent.WaitOne();
            serverSocket.Connect(endPoint);
            MessageBox.Show($"{serverSocket.RemoteEndPoint}");
            return true;
        }

        public bool Disconnect()
        {

            return true;
        }

        public bool SendMesssage()
        {


            return true;
        }



    }
}