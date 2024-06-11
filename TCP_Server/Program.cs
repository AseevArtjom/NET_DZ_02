using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Server
{
    internal class Program
    {
        public static TcpChatServer chat;

        static void Main(string[] args)
        {
            int port = 8080;
            chat = new TcpChatServer(port);
            chat.Run();
        }
    }
}
