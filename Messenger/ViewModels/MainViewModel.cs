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
        private string myName;
        private Command commandConnect;
        private Command commandSend;
        private TcpClient tcpClient = new TcpClient();
        //   private bool enableToolTip = false;//---
        private ICollection<User> users = new ObservableCollection<User>();
        private User selectedUser;
        private string stringMessage;
        private int myPort;
        private Utilities.EndPoint utilitiesEndPoint = new Utilities.EndPoint();
        private ICollection<MessageViewModel> messageViewModels = new ObservableCollection<MessageViewModel>();
        private bool enableMessageWriting = false;
        private bool enableMessageSending = false;
        private bool enableUsernameField = true;
        private BinaryFormatter formatter = new BinaryFormatter();

        public MainViewModel()
        {
            commandConnect = new DelegateCommand(Connect, EnableCommandConnect);
            commandSend = new DelegateCommand(SendMesssage);
        }

        public string IPEndPoint
        {
            get => ipEndPoint.ToString() + ":" + endPointPort;
            set
            {
                if (!$"{ipEndPoint}:{endPointPort}".Equals(value))
                {
                    utilitiesEndPoint = utilitiesEndPoint.GetEndPoint(value);
                    ipEndPoint = utilitiesEndPoint.IPAddress;
                    endPointPort = utilitiesEndPoint.Port;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(IPEndPoint)));
                }
            }
        } //+

        public ICommand CommandConnect => commandConnect;//+

        public ICommand CommandSend => commandSend;

        public IEnumerable<User> Users => users; //+

        public IEnumerable<MessageViewModel> MessageViewModels => messageViewModels;

        public string TimeNow => DateTime.Now.ToLongTimeString();

        public bool EnableUsernameField
        {
            get => enableUsernameField;
            set => SetProperty(ref enableUsernameField, value);
        }

        public string StringMessage
        {
            get => stringMessage;
            set
            {
                EnableMessageSending = value.Length > 0;
                stringMessage = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(StringMessage)));
            }
        }

        public bool EnableMessageWriting
        {
            get => enableMessageWriting;
            set => SetProperty(ref enableMessageWriting, value);
        }

        public bool EnableMessageSending
        {
            get => enableMessageSending;
            set => SetProperty(ref enableMessageSending, value);
        }

        public string MyName //?
        {
            get => myName;
            set => SetProperty(ref myName, value);
        }

        //    public bool EnableToolTip { get; private set; }

        public User SelectedUser
        {
            get => selectedUser;
            set
            {
                selectedUser = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(SelectedUser)));
                EnableMessageWriting = SelectedUser != null;
            }
        }

        public void Connect()
        {
            if (EnableCommandConnect())
            {
                Task.Factory.StartNew(() =>
                {
                    tcpClient.Connect(ipEndPoint, endPointPort);
                    utilitiesEndPoint = utilitiesEndPoint.GetEndPoint(tcpClient.Client.LocalEndPoint.ToString());
                    myIpAddress = utilitiesEndPoint.IPAddress;
                    myPort = utilitiesEndPoint.Port;

                    User I = new User(utilitiesEndPoint.IPAddress, MyName, utilitiesEndPoint.Port);
                    NetworkStream stream = tcpClient.GetStream();
                    formatter.Serialize(stream, I);
                    stream.Flush();
                    WaitForMessage();
                });
            }
            commandConnect.OnCanExecuteChanged(EventArgs.Empty);
        }

        public bool Disconnect()
        {

            return true;
        }

        public void SendMesssage() //+
        {
            User sender = new User(myIpAddress, MyName, myPort);
            User receiver = new User(SelectedUser.IPAddress, SelectedUser.Username, SelectedUser.Port);
            Message message = new Message(sender, receiver, stringMessage);
            NetworkStream stream = tcpClient.GetStream();
            formatter.Serialize(stream, message);
            stream.Flush();

            MessageViewModel messageViewModel = new MessageViewModel(message);
            messageViewModel.PrepositionRecipient = $"{sender.Username}";
            messageViewModel.PrepositionSender = "Me";
            messageViewModels.Add(messageViewModel);
        }

        public bool EnableCommandConnect()
        {
            bool enableCommandConnect = true;

            foreach (User user in users)
            {
                if (user.Username == myName)
                {
                    enableCommandConnect = false;
                }
            }
            return enableCommandConnect;
        }

        public void WaitForMessage()
        {
            Task.Factory.StartNew(wait =>
            {
                while (true)
                {
                    NetworkStream stream = tcpClient.GetStream();
                    int key = 0;
                    byte[] bytes = new byte[4];
                    stream.Read(bytes, 0, bytes.Length);
                    key = BitConverter.ToInt32(bytes, 0);

                    if (key == 0)
                    {
                        Message message = (Message)formatter.Deserialize(stream);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageViewModel messageViewModel = new MessageViewModel(message);
                            messageViewModel.PrepositionRecipient = "Me";
                            messageViewModel.PrepositionSender = $"{message.Sender.Username}";
                            messageViewModels.Add(messageViewModel);
                        });
                    }
                    else
                    {
                        User user = (User)formatter.Deserialize(stream);

                        if (user.Username != MyName)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                users.Add(user);
                            });
                        }

                    }
                }
            }, null, TaskCreationOptions.LongRunning);
        }
    }
}