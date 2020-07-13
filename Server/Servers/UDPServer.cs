using ServerFramework.Clients;
using ServerFramework.Controller;
using ServerFramework.Tools;
using SocketDemoProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerFramework.Servers
{
    class UDPServer
    {
        Socket udpServer;//udpsocket
        IPEndPoint bindEP;//本地监听ip
        EndPoint remoteEP;//远程ip

        Server server;

        ControllerManager controllerManager;

        Byte[] buffer = new Byte[1024];

        Thread receiveThread;

        public UDPServer(int port,Server server,ControllerManager controllerManager)
        {
            this.server = server;
            this.controllerManager = controllerManager;
            udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bindEP = new IPEndPoint(IPAddress.Any, port);
            remoteEP = (EndPoint)bindEP;
            udpServer.Bind(bindEP);
            receiveThread = new Thread(ReceiveMsg);
            receiveThread.Start();
            Console.WriteLine("upd服务已启动...");
        }

        

        ~UDPServer()
        {
            if (receiveThread!=null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
        }


        private void ReceiveMsg()
        {
            while (true)
            {
                int len = udpServer.ReceiveFrom(buffer, ref remoteEP);
                MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 0, len);
                HandleRequest(pack, remoteEP);
            }
        }

        private void HandleRequest(MainPack pack, EndPoint remoteEP)
        {
            Client client = server.GetClientFromUsername(pack. Username);

            if (client.RemoteEP == null)
            {
                client.RemoteEP = remoteEP;
            }

            controllerManager.HandleRequest(pack, client, true);

        }

        public void SendTo(MainPack pack, EndPoint remoteEP)
        {
            byte[] buff = Message.PackDataUDP(pack);
            udpServer.SendTo(buff, buff.Length, SocketFlags.None, remoteEP);
        }
    }
}
