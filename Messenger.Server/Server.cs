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

                Task.Factory.StartNew(foo =>
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    User user;

                    NetworkStream stream = tcpClient.GetStream();
                    user = (User)formatter.Deserialize(stream);
                    users.Add(user);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" User ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(user.Username);
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

                }, null, TaskCreationOptions.LongRunning);

            }
        }

        public void Disconnect(string Username)
        {

        }

        public void SendMesssage(Message message)
        {
            foreach (User user in users)
            {
                if (message.Recipient.IPAddress.Equals(user.IPAddress))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    IPEndPoint iPEndPoint = new IPEndPoint(user.IPAddress, user.Port);
                    TcpClient tcpClient = new TcpClient(iPEndPoint);
                    NetworkStream stream = tcpClient.GetStream();
                    formatter.Serialize(stream, message);
                }
            }
        }

        public void GiveTistTheUsers(string name)
        {



        }

        //public void StopServer()
        //{ }
        //    listener.Stop();
        //}

        public void UpdateUserCollection()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            for (int i = 0; i < users.Count; i++)
            {

                Utilities.EndPoint endPoint = new Utilities.EndPoint();
            //    endPoint.GetEndPoint(namesUsers[i]);

                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(endPoint.IPAddress, endPoint.Port);
//User user=new User(endPoint.IPAddress,endPoint)
                NetworkStream stream = tcpClient.GetStream();

           //     formatter.Serialize()



            }


        }

    }
}