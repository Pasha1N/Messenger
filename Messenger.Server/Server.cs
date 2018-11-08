using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Server
{
    public class Server
    {
        private readonly EventWaitHandle serverStartedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "{94BF7448-1BF3-4A4A-A309-B328E02689FC}");
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPAddress ipAddress = IPAddress.Loopback;
        private int port = 2020;

        public Server()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(10);
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

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine($"Client connected {clientSocket.RemoteEndPoint}.");




            }
        }

        public bool Connect()
        {

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

        public bool GetMessage()
        {
            return true;
        }

    }
}