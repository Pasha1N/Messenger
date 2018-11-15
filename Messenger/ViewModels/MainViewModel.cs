using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Messenger.Client.Commands;
using Messenger.Utilities;
using Messenger.Models;

namespace Messenger.Client.ViewModels
{
    public class MainViewModel : EventINotifyPropertyChanged
    {
        private static readonly EventWaitHandle serverStartedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "{94BF7448-1BF3-4A4A-A309-B328E02689FC}");
        private IPAddress myIpAddress;
        private IPAddress ipEndPoint = IPAddress.Loopback;
        private int endPointPort = 2020;
        private string myUsername;
        private Command commandConnect;
        private Command commandSend;
        private TcpClient tcpClient = new TcpClient();
        private bool enableToolTip = false;//---
        private ICollection<User> users = new ObservableCollection<User>();
        private User selectedUser;
        private string stringMessage;
        private int myPort;

        public MainViewModel()
        {
            commandConnect = new DelegateCommand(Connect);
            commandSend = new DelegateCommand(SendMesssage);
            users.Add(new User(IPAddress.Parse("1.1.1.1"), "Jon.S",2025));
        }

        public string IPEndPoint
        {
            get => ipEndPoint.ToString() + ":" + endPointPort;
            set
            {
                Utilities.EndPoint endPoint = new Utilities.EndPoint();
                endPoint = endPoint.GetEndPoint(value);
                ipEndPoint = endPoint.IPAddress;
                endPointPort = endPoint.Port;
            }
        } //+

        public ICommand CommandConnect => commandConnect;//+

        public ICommand CommandSend => commandSend;

        public ICollection<User> Users => users; //+

        public string StringMessage
        {
            get => stringMessage;
            set => SetProperty(ref stringMessage, value);
        }



        public string MyUsername //?
        {
            get => myUsername;
            set => SetProperty(ref myUsername, value);
        }

        public bool EnableToolTip { get; private set; }

        public User SelectedUser
        {
            get => selectedUser;
            set => SetProperty(ref selectedUser, value);
        } //+

        public void Connect()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            serverStartedEvent.WaitOne();

            tcpClient.Connect(ipEndPoint, endPointPort);
            Utilities.EndPoint endPoint = new Utilities.EndPoint();
            endPoint = endPoint.GetEndPoint(tcpClient.Client.LocalEndPoint.ToString());
            myIpAddress = endPoint.IPAddress;
            myPort = endPoint.Port;

            User I = new User(endPoint.IPAddress, MyUsername, endPoint.Port);
            NetworkStream stream = tcpClient.GetStream();
       //     int key;
        //    byte[] bytes = new byte[4];
        //    stream.Read(bytes, 0, bytes.Length);
         //   key = BitConverter.ToInt32(bytes,0);

          //  if(key==0)
         //   {

         //   }
          //  else
          //  {
                formatter.Serialize(stream, I);
                stream.Flush();
          //  }
           
        }//+

        public bool Disconnect()
        {

            return true;
        }

        public void SendMesssage() //+
        {
            BinaryFormatter formatter = new BinaryFormatter();
            User sender = new User(myIpAddress, MyUsername, myPort);
             User receiver = new User(SelectedUser.IPAddress, SelectedUser.Username, SelectedUser.Port);
          //  User receiver=new User(IPAddress.Parse("2.2.2.2"),"Gosha",6529);
            Message message = new Message(sender, receiver, stringMessage);

            NetworkStream stream = tcpClient.GetStream();
            formatter.Serialize(stream, message);
            stream.Flush();
        }

        public void UpdateUserCollection()
        {



        }
    }
}