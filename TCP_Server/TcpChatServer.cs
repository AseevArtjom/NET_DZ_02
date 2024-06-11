using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCP_Server
{
    public class TcpChatServer
    {
        private TcpListener listener;

        private List<TcpClient> clients = new List<TcpClient>();

        private Dictionary<TcpClient, string> names = new Dictionary<TcpClient, string>();

        private Queue<string> messageQueue = new Queue<string>();

        public readonly int Port;

        public readonly int BufferSize = 2048;
        public bool Running { get; private set; }

        public TcpChatServer(int port)
        {
            Port = port;
            Running = false;
            listener = new TcpListener(IPAddress.Any, Port);
        }

        public void Disable()
        {
            Running = false;
            Console.WriteLine("The server is disabled...");
        }

        public void Run()
        {
            Console.WriteLine("Starting server...");
            listener.Start();
            Running = true;
            while (Running)
            {
                if (listener.Pending())
                {
                    NewConnection();
                }

                CheckForDisconnect();
                CheckForNewMessages();
                sendMessage();

                Thread.Sleep(10);
            }

            foreach (TcpClient cl in clients)
            {
                CleanUpClient(cl);
            }
            listener.Stop();
        }

        private void NewConnection()
        {
            bool ok = false;
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            client.SendBufferSize = BufferSize;
            client.ReceiveBufferSize = BufferSize;

            EndPoint endPoint = client.Client.RemoteEndPoint;

            byte[] messageBuffer = new byte[BufferSize];

            int BytesRead = stream.Read(messageBuffer, 0, messageBuffer.Length);

            if (BytesRead > 0)
            {
                string msg = Encoding.UTF8.GetString(messageBuffer, 0, BytesRead);

                if (msg.StartsWith("name :"))
                {
                    string Name = msg.Substring(msg.IndexOf(":") + 1).Trim();

                    if ((Name != string.Empty) && (!names.ContainsValue(Name)))
                    {
                        ok = true;
                        names.Add(client, Name);
                        clients.Add(client);

                        Console.WriteLine($"{endPoint} is Client with name : {Name}");
                        messageQueue.Enqueue(String.Format($"{Name} has joined the chat"));
                    }
                    else
                    {
                        Console.WriteLine($"Duplicate or empty name: {Name}");
                    }
                }
            }

            if (!ok)
            {
                client.Close();
            }
        }

        private void CheckForDisconnect()
        {
            foreach (TcpClient cl in clients.ToArray())
            {
                if (isDisconnected(cl))
                {
                    string name = names[cl];
                    Console.WriteLine($"{name} has left");

                    messageQueue.Enqueue(String.Format($"{name} has left the chat"));

                    clients.Remove(cl);
                    names.Remove(cl);
                    CleanUpClient(cl);
                }
            }
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

        private void CheckForNewMessages()
        {
            foreach (TcpClient cl in clients)
            {
                int messageLenght = cl.Available;
                if (messageLenght > 0)
                {
                    byte[] msgBuffer = new byte[messageLenght];
                    cl.GetStream().Read(msgBuffer, 0, msgBuffer.Length);

                    string msg = string.Format($"{names[cl]}: {Encoding.UTF8.GetString(msgBuffer)}");
                    Console.WriteLine(msg);
                    messageQueue.Enqueue(msg);
                }
            }
        }

        private void sendMessage()
        {
            foreach (string msg in messageQueue)
            {
                byte[] msgBuffer = Encoding.UTF8.GetBytes(msg);

                foreach (TcpClient cl in clients)
                {
                    cl.GetStream().Write(msgBuffer, 0, msgBuffer.Length);
                }
            }
            messageQueue.Clear();
        }

        private static void CleanUpClient(TcpClient client)
        {
            client.GetStream().Close();
            client.Close();
        }
    }
}
