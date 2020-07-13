using ServerFramework.DAO;
using ServerFramework.Servers;
using ServerFramework.Tools;
using SocketDemoProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerFramework.Clients
{
    class Client
    {
        /// <summary>
        /// 客户端socket
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 客户端的网络地址
        /// </summary>
        public EndPoint RemoteEP { get; set; }

        /// <summary>
        /// 网络传输消息
        /// </summary>
        private Message message;

        /// <summary>
        /// 用户数据
        /// </summary>
        private UserData userData;

        /// <summary>
        /// 服务器
        /// </summary>
        private Server server;

        /// <summary>
        /// 用户名
        /// </summary>
        private string username;

        public void SendTo(MainPack pack)
        {
            if (RemoteEP == null) return;

            server.udpServer.SendTo(pack, RemoteEP);
        }

        /// <summary>
        /// client所在房间
        /// </summary>
        private Room room;

        /// <summary>
        /// 客户端玩家生命值
        /// </summary>
        private int hp;

        /// <summary>
        /// 客户端玩家坐标数据包
        /// </summary>
        private PostionPack pos;

        /// <summary>
        /// 用户数据属性
        /// </summary>
        public UserData UserData { get => userData;}

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get => username; set => username = value; }

        /// <summary>
        /// 客户端所在房间
        /// </summary>
        public Room Room { get => room; set => room = value; }

        /// <summary>
        /// 客户端socket
        /// </summary>
        public Socket Socket { get => socket; set => socket = value; }

        /// <summary>
        /// 客户端玩家生命值
        /// </summary>
        public int Hp { get => hp; set => hp = value; }

        /// <summary>
        /// 客户端玩家坐标数据包
        /// </summary>
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
            this.socket = socket;
            this.server = server;
            StartReceive();
        }

        /// <summary>
        /// 开始异步接收
        /// </summary>
        private void StartReceive()
        {
            socket.BeginReceive(message.Buffer,message.StartIndex,message.RemSize,SocketFlags.None,ReceiveCallBack,null);
            //Console.WriteLine(DateTime.Now + ":开始等待"+ socket.RemoteEndPoint+ "发送消息....");
        }

        /// <summary>
        /// 异步接收回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                if (socket == null || socket.Connected == false)
                {
                    Console.WriteLine("socket == null || socket.Connected == false");
                    return;
                }

                int len = socket.EndReceive(ar);
                if (len == 0)
                {
                    return;
                }
                message.ReadBuffer(len, HandleRequest);
                //Console.WriteLine(DateTime.Now + ":已接收" + socket.RemoteEndPoint + "发送的消息....");
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

        /// <summary>
        /// 客户端注册账号
        /// </summary>
        /// <param name="pack">包含注册信息的数据包</param>
        /// <returns>注册是否成功</returns>
        public bool Logon(MainPack pack)
        {
            return userData.Logon(pack);
  
        }

        /// <summary>
        /// 客户端登录账号
        /// </summary>
        /// <param name="pack">登录数据包</param>
        /// <returns></returns>
        public bool Login(MainPack pack)
        {
            return userData.Login(pack);
        }

        /// <summary>
        /// 获取玩家数据包
        /// </summary>
        /// <returns>玩家数据包</returns>
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
