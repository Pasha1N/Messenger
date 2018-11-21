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
using System.IO;

namespace Messenger.Client.ViewModels
{
    public class MainViewModel : EventINotifyPropertyChanged
    {
        private IPAddress myIpAddress;
        private IPAddress ipEndPoint = IPAddress.Loopback;
        private int endPointPort = 2020;
        private string myName;
        private Command commandConnect;
        private Command commandSend;
        private Command commandDisconnect;
        private TcpClient tcpClient = null;
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
        private User I;
        private bool enableCommandConnect = true;
        private bool visibilityOfUsernameError = false;
        private bool visibilityOfServerNameError = false;
        private bool enableCommandDisconnect = false;
        private bool haveIBeenDisconnected = true;
        private object sync = new object();

        public MainViewModel()
        {
            commandConnect = new DelegateCommand(Connect, EnableCommandConnect);
            commandSend = new DelegateCommand(SendMessage);
            commandDisconnect = new DelegateCommand(Disconnect, EnableCommandDisconnect);
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
        }

        public ICommand CommandConnect => commandConnect;//+

        public ICommand CommandSend => commandSend;

        public IEnumerable<User> Users => users; //+

        public ICommand CommandDisconnect => commandDisconnect;

        public IEnumerable<MessageViewModel> MessageViewModels => messageViewModels;

        public bool EnableUsernameField
        {
            get => enableUsernameField;
            set => SetProperty(ref enableUsernameField, value);
        }

        public bool EnableCommandDisconnect()
        {
            return enableCommandDisconnect;
        }

        public bool VisibilityOfUsernameError
        {
            get => visibilityOfUsernameError;
            set => SetProperty(ref visibilityOfUsernameError, value);
        }

        public bool VisibilityOfServerNameError
        {
            get => visibilityOfServerNameError;
            set => SetProperty(ref visibilityOfServerNameError, value);
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
            lock (sync)
            {
                Task.Factory.StartNew(() =>
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(ipEndPoint, endPointPort);
                    utilitiesEndPoint = utilitiesEndPoint.GetEndPoint(tcpClient.Client.LocalEndPoint.ToString());
                    myIpAddress = utilitiesEndPoint.IPAddress;
                    myPort = utilitiesEndPoint.Port;

                    I = new User(myIpAddress, MyName, myPort);
                    NetworkStream stream = tcpClient.GetStream();
                    formatter.Serialize(stream, I);
                    stream.Flush();
                    bool isConnected = false;
                    byte[] bytes = new byte[1];
                    stream.Read(bytes, 0, bytes.Length);
                    isConnected = BitConverter.ToBoolean(bytes, 0);

                    if (!isConnected)
                    {
                        WaitForMessage();

                        enableCommandConnect = false;
                        VisibilityOfUsernameError = false;
                        enableCommandDisconnect = true;
                        haveIBeenDisconnected = false;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            commandConnect.OnCanExecuteChanged(EventArgs.Empty);
                            commandDisconnect.OnCanExecuteChanged(EventArgs.Empty);
                        });
                    }
                    else
                    {
                        VisibilityOfUsernameError = true;
                        tcpClient.Close();
                    }
                });
            }

            lock (sync)
            {
                NetworkStream dataToDisable = tcpClient.GetStream(); // why here null

                //while (true)
                //{
                //    byte[] bytes = new byte[1];
                //    dataToDisable.Read(bytes, 0, bytes.Length);
                //    bool isDisconnected = BitConverter.ToBoolean(bytes,0);
                //    if (isDisconnected)
                //    {
                //        break;
                //    }
                //}
            }
        }

        public void Disconnect()
        {
            tcpClient.Close();



            enableCommandDisconnect = false;
            enableCommandConnect = true;
            haveIBeenDisconnected = true;
            commandDisconnect.OnCanExecuteChanged(EventArgs.Empty);
            commandConnect.OnCanExecuteChanged(EventArgs.Empty);
        }

        public void SendMessage() //+
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

                    try
                    {
                        stream.Read(bytes, 0, bytes.Length);
                    }
                    catch (IOException)
                    {
                    }

                    key = BitConverter.ToInt32(bytes, 0);

                    //if (!haveIBeenDisconnected)
                    //{
                    //    break;
                    //}

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