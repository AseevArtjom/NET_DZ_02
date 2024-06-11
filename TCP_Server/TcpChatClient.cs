using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCP_Server
{
    public class TcpChatClient
    {
        public readonly string ServerAddress;
        public readonly int Port;
        private TcpClient client;
        public bool Running { get; private set; }

        public readonly int BufferSize = 2048;
        private NetworkStream msgStream = null;

        public readonly string Name;

        public TcpChatClient(string serverAddress, int port, string name)
        {
            client = new TcpClient();
            client.SendBufferSize = BufferSize;
            client.ReceiveBufferSize = BufferSize;
            Running = false;

            ServerAddress = serverAddress;
            Port = port;
            Name = name;
        }

        public void Connect()
        {
            client.Connect(ServerAddress, Port);

            EndPoint endPoint = client.Client.RemoteEndPoint;

            if (client.Connected)
            {
                Console.WriteLine($"Connected to the server at {endPoint}");

                msgStream = client.GetStream();
                byte[] msgBuffer = Encoding.UTF8.GetBytes(String.Format($"name :{Name}"));
                msgStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
        }

        public void SendMessages()
        {
            Running = true;

            while (Running)
            {
                Console.Write($"{Name}> ");
                string msg = Console.ReadLine();

                if ((msg.ToLower() == "quit") || (msg.ToLower() == "exit"))
                {
                    Console.WriteLine("Disconnecting...");
                    Running = false;
                }
                else if (msg != String.Empty)
                {
                    byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);
                    msgStream.Write(msgBuffer, 0, msgBuffer.Length);
                }
                Thread.Sleep(10);

                if (isDisconnected(client))
                {
                    Running = false;
                    Console.WriteLine("Server has disconnected...");
                }
            }

            CleanUpNetwork();
            Console.WriteLine("Disconnected...");
        }

        private void CleanUpNetwork()
        {
            msgStream?.Close();
            msgStream = null;
            client.Close();
        }

        private static bool isDisconnected(TcpClient client)
        {
            try
            {
                Socket socket = client.Client;
                return socket.Poll(10 * 1000, SelectMode.SelectRead) && (socket.Available == 0);
            }
            catch (SocketException ex)
            {
                return true;
            }
        }
    }
}
