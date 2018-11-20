using Messenger.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Server
{
    public class Server
    {
        private IPAddress ipAddress = IPAddress.Loopback;
        private int port = 2020;
        private TcpListener listener;
        private IList<User> users = new List<User>();
        private IDictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
        private BinaryFormatter formatter = new BinaryFormatter();

        public Server()
        {
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

            Connect();
        }

        public void Connect()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" Waiting for new client...");
            Console.ResetColor();

            Task.Factory.StartNew(Connect =>
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    TcpClient tcpClient = listener.AcceptTcpClient();
                    Console.ResetColor();

                    Task.Factory.StartNew(() =>
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
                                Message message = (Message)formatter.Deserialize(stream);//try 
                                Console.Write($" User ({message.Sender.Username}) Send a message to");
                                Console.WriteLine($"User ({message.Recipient.Username})");
                                SendMesssage(message);
                            }
                        }
                    });
                }
            }, null, TaskCreationOptions.LongRunning);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" To close the server, click ( Escape )");
            Console.ResetColor();

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    listener.Stop();
                    Console.WriteLine("The server was closed");
                    Console.ResetColor();
                    break;
                }
            }
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

        public void SendMesssage(Message message)
        {
            bool found = false;

            foreach (User user in users)
            {
                TcpClient tcpClient = null;

                foreach (KeyValuePair<string, TcpClient> client in clients)
                {
                    if ($"{user.IPAddress}{user.Port}" == client.Key)
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
                    break;
                }
            }
        }

        public void SendMesssage(User user) //нужно использовать обобщение
        {
            foreach (User item in users)
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
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine(" To close the server, click ( Escape )");
        //        Console.ResetColor();

        //        while (true)
        //        {
        //            ConsoleKeyInfo keyInfo = Console.ReadKey();

        //            if (keyInfo.Key == ConsoleKey.Escape)
        //            {
        //                Console.ForegroundColor = ConsoleColor.Green;
        //                listener.Stop();
                       
        //                listenerIsClose = true;
        //                Console.WriteLine("Server Stopped");
        //                Console.ResetColor();
        //            }
        //        }
        //    });
        //}

        public void UpdateUserCollection(TcpClient whoNeedsUpgrade, User endPoint)
        {
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
        }
    }
}