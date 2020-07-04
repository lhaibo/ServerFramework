using ServerFramework.DAO;
using ServerFramework.Servers;
using ServerFramework.Tools;
using SocketDemoProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework.Clients
{
    class Client
    {
        private Socket socket;
        private Message message;
        private UserData userData;
        private Server server;

        private string username;
        private Room room;
        private int hp;
        private PostionPack pos;

        public UserData UserData { get => userData;}
        public string Username { get => username; set => username = value; }
        public Room Room { get => room; set => room = value; }
        public Socket Socket { get => socket; set => socket = value; }
        public int Hp { get => hp; set => hp = value; }
        public PostionPack Pos { get => pos; set => pos = value; }

        /// <summary>
        /// 初始化并开始异步接收
        /// </summary>
        /// <param name="socket">连接上的客户端socket</param>
        /// <param name="server">该客户端连接上的服务端</param>
        public Client(Socket socket,Server server)
        {
            userData = new UserData();
            message = new Message();
            this.Socket = socket;
            this.server = server;
            StartReceive();
        }

        /// <summary>
        /// 开始异步接收
        /// </summary>
        private void StartReceive()
        {
            Socket.BeginReceive(message.Buffer,message.StartIndex,message.RemSize,SocketFlags.None,ReceiveCallBack,null);
            Console.WriteLine(DateTime.Now + ":开始等待"+ Socket.RemoteEndPoint+ "发送消息....");
        }

        /// <summary>
        /// 异步接收回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                if (Socket == null || Socket.Connected == false)
                {
                    Console.WriteLine("socket == null || socket.Connected == false");
                    return;
                }

                int len = Socket.EndReceive(ar);
                if (len == 0)
                {
                    return;
                }
                message.ReadBuffer(len, HandleRequest);
                Console.WriteLine(DateTime.Now + ":已接收" + Socket.RemoteEndPoint + "发送的消息....");
                StartReceive();
            }
            catch (Exception e)
            {
                Close();
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 客户端断开连接，如果在房间内退出房间，然后将其从服务端删除，并关闭socket
        /// </summary>
        private void Close()
        {
            if (room!=null)
            {
                room.Exit(server,this);
            }
            server.RemoveClient(this);
            socket.Close();
        }

        /// <summary>
        /// 向远程客户端发送消息
        /// </summary>
        /// <param name="pack">要发送的消息包</param>
        public void Send(MainPack pack)
        {
            socket.Send(Message.PackData(pack));
        }

        /// <summary>
        /// 处理消息包
        /// </summary>
        /// <param name="pack">要处理的消息包</param>
        private void HandleRequest(MainPack pack)
        {
            server.HandleRequest(pack, this);
        }


        public bool Logon(MainPack pack)
        {
            return userData.Logon(pack);
  
        }

        public bool Login(MainPack pack)
        {
            return userData.Login(pack);
        }

        public PlayerPack GetPlayerPack()
        {
            PlayerPack playerPack = new PlayerPack();
            playerPack.PlayerName = username;
            playerPack.WinCount = 2;
            playerPack.Hp = hp;
            playerPack.PostionPack = pos;
            return playerPack;
        }
    }
}
