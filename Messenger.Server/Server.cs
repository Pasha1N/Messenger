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
        private ICollection<string> namesUsers = new List<string>();

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
                namesUsers.Add(tcpClient.Client.RemoteEndPoint.ToString());
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

                    //using (NetworkStream stream = tcpClient.GetStream())// information about the connected
                   // {
                        NetworkStream stream = tcpClient.GetStream();
                        user = (User)formatter.Deserialize(stream);
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
                  //  }

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" Waiting for new client...");
                    Console.ResetColor();

                    while (true)
                    {
                        //   try
                        //{
                        //  using (NetworkStream stream = tcpClient.GetStream())
                        //{
                        NetworkStream stream1 = tcpClient.GetStream();
                                Message message = (Message)formatter.Deserialize(stream1);
                                Console.WriteLine($"Message-> {message.GetMessage}");
                                Console.WriteLine($" SenderName->{message.Sender.Name}, SenderUsername->{message.Sender.Username}");
                                Console.WriteLine($"RecipientName->{message.Recipient.Name}, RecipientUsername->{message.Recipient.Username}");
                               // SendMesssage(message);
                            //}
                     //   }
                     //   catch (InvalidOperationException)
                      //  {
                       //     break;
                       // }
                    }
                    
                }, null, TaskCreationOptions.LongRunning);
             
            }
        }

        public void Disconnect(string Username)
        {

        }

        public void SendMesssage(Message message)
        {
            foreach (string name in namesUsers)
            {
                if (message.Recipient.Name.Equals(name))
                {
                    Utilities.EndPoint endPoint = new Utilities.EndPoint();
                    endPoint = endPoint.GetEndPoint(message.Recipient.Name);
                    IPEndPoint iPEndPoint = new IPEndPoint(endPoint.IPAddress, endPoint.Port);
                    TcpClient tcpClient = new TcpClient(iPEndPoint);
                    BinaryFormatter formatter = new BinaryFormatter();

                   // using (NetworkStream stream = tcpClient.GetStream())
                    //{
                        NetworkStream stream = tcpClient.GetStream();
                        formatter.Serialize(stream, message);
                   // }
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

    }
}