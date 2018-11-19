using Messenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Server
{
    public class Server
    {
        private readonly EventWaitHandle serverStartedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "{94BF7448-1BF3-4A4A-A309-B328E02689FC}");
        private IPAddress ipAddress = IPAddress.Loopback;
        private int port = 2020;
        private TcpListener listener;
        private IList<User> users = new List<User>();
        private IDictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
        private BinaryFormatter formatter = new BinaryFormatter();

        public Server()
        {
            {
                // IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
                //  serverSocket.Bind(ipEndPoint);
                //   serverSocket.Listen(10);
            }
            startServer();
        }

        public void startServer()
        {
            bool work = true;

            while (work)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(" Server started");
                    Console.WriteLine($" IpServer-> {ipAddress}");
                    Console.ResetColor();
                    work = false;
                    listener = new TcpListener(ipAddress, port);
                    listener.Start();
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  To start server click the enter");
                    Console.ResetColor();
                }
            }

            serverStartedEvent.Set();
            Connect();
        } //+

        public void Connect() //+
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" Waiting for new client...");
            Console.ResetColor();
            // Task.Factory.StartNew(connect  => //to ask
            //   {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                TcpClient tcpClient = listener.AcceptTcpClient();
                Console.ResetColor();

                {
                    //ConsoleKeyInfo keyInfo = Console.ReadKey();

                    //if (keyInfo.Key == ConsoleKey.Escape)
                    //{
                    //    Console.ForegroundColor = ConsoleColor.Green;
                    //    listener.Stop();
                    //    Console.WriteLine("Server Stopped");
                    //    Console.ResetColor();
                    //}
                } //Server Stopped

                Task.Factory.StartNew(NewClient =>
                {
                    User user;
                    NetworkStream stream = tcpClient.GetStream();
                    user = (User)formatter.Deserialize(stream);

                    bool userFound = SearchUsers(user);
                    byte[] bytes = new byte[1];
                    bytes = BitConverter.GetBytes(userFound);
                    stream.Write(bytes, 0, bytes.Length);

                    if (!userFound)
                    {
                        users.Add(user);
                        clients.Add(user.IPAddress.ToString() + user.Port.ToString(), tcpClient);

                        //падает при поdключении нескольких пользователей если сервер выключен
                        SendMesssage(user);
                        UpdateUserCollection(tcpClient, user);

                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write(" User ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(user.Username + " " + user.IPAddress + ":" + user.Port);
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(" Connected.");
                            Console.Write(" User ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(user.Username);
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(" was notified to update list of connected users.");
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(" Waiting for new client...");
                            Console.ResetColor();
                        }
                        while (true)
                        {
                            Message message = (Message)formatter.Deserialize(stream);
                            Console.WriteLine($"Message-> {message.GetMessage}");
                            Console.WriteLine($" SenderName->{message.Sender.IPAddress}");
                            Console.WriteLine($" SenderUsername->{message.Sender.Username}");
                            Console.WriteLine($"Sender Port-> {message.Sender.Port}");
                            Console.WriteLine($"RecipientName->{message.Recipient.IPAddress}, RecipientUsername->{message.Recipient.Username}");
                            SendMesssage(message);
                        }
                    }
                }, null, TaskCreationOptions.LongRunning);

            }
            //   }, null, TaskCreationOptions.LongRunning);
        }

        public bool SearchUsers(User user)
        {
            bool enableCommandConnect = false;

            foreach (User item in users)
            {
                if (item.Username == user.Username)
                {
                    enableCommandConnect = true;
                }
            }
            return enableCommandConnect;
        }

        public void Disconnect(string Username)
        {

        }

        public void SendMesssage(Message message)
        {
            bool found = false;

            foreach (User user in users)
            {
                TcpClient tcpClient = null;

                foreach (KeyValuePair<string, TcpClient> client in clients)
                {
                    if ($"{user.IPAddress}{user.Port}"==client.Key)
                    {
                        tcpClient = client.Value;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    NetworkStream stream = tcpClient.GetStream();
                    byte[] bytes = new byte[4];
                    bytes = BitConverter.GetBytes(0);
                    int code = BitConverter.ToInt32(bytes, 0);
                    stream.Write(bytes, 0, bytes.Length);
                    formatter.Serialize(stream, message);
                    stream.Flush();

                    string date = DateTime.Now.ToShortTimeString(); //date = new DateTime();
                    string date1 = DateTime.Now.ToShortTimeString();

                    break;
                }
            }
        }

        public void SendMesssage(User user)//будет ли нарушение солида если здесь использовать обобщение?
        {
            foreach (User item in users)// ошипка: изменение колекцие ( не всегда );
            {
                TcpClient tcpClient = null;

                foreach (KeyValuePair<string, TcpClient> client in clients)
                {
                    if (client.Key == $"{item.IPAddress}{ item.Port}")
                    {
                        tcpClient = client.Value;
                        break;
                    }
                }

                if (tcpClient != null)
                {
                    NetworkStream stream = tcpClient.GetStream();

                    byte[] bytes = new byte[4];
                    bytes = BitConverter.GetBytes(1);
                    int code = BitConverter.ToInt32(bytes, 0);
                    stream.Write(bytes, 0, bytes.Length);
                    formatter.Serialize(stream, user);
                    stream.Flush();

                }
            }
        }

        //public void StopServer()
        //{ }
        //    listener.Stop();
        //}

        public void UpdateUserCollection(TcpClient whoNeedsUpgrade, User endPoint)
        {
            //  Task.Run(() => //при оборачивании происходит ошибка
            //  {
            NetworkStream stream = whoNeedsUpgrade.GetStream();

            foreach (User user in users)
            {
                if ($"{user.IPAddress}{user.Port}" != $"{endPoint.IPAddress}{endPoint.Port}")
                {

                    byte[] bytes = new byte[4];
                    bytes = BitConverter.GetBytes(1);
                    int s = BitConverter.ToInt32(bytes, 0);
                    stream.Write(bytes, 0, bytes.Length);
                    formatter.Serialize(stream, user);
                    stream.Flush();
                }
            }
            //});
        }
    }
}