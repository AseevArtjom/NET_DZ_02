using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCP_Server;

namespace NET_DZ_02
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter name : ");
            string name = Console.ReadLine();

            string address = "127.0.0.1";
            int port = 8080;

            TcpChatClient client = new TcpChatClient(address,port,name);

            client.Connect();
            client.SendMessages();
        }
    }
}
