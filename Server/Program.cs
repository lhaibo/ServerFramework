using ServerFramework.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerFramework.Servers;
namespace ServerFramework
{
    class Program
    {

        static void Main(string[] args)
        {
            Server server = new Server("127.0.0.1", 6666, 5);
            Console.Read();
        }

    }
}
