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

        public Client(Socket socket,Server server)
        {
            userData = new UserData();
            message = new Message();
            this.Socket = socket;
            this.server = server;
            StartReceive();
        }

        void StartReceive()
        {
            Socket.BeginReceive(message.Buffer,message.StartIndex,message.RemSize,SocketFlags.None,ReceiveCallBack,null);
            Console.WriteLine(DateTime.Now + ":开始等待"+ Socket.RemoteEndPoint+ "发送消息....");
        }

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
        private void Close()
        {
            if (room!=null)
            {
                room.Exit(server,this);
            }
            server.RemoveClient(this);
            socket.Close();
        }
        public void Send(MainPack pack)
        {
            Socket.Send(Message.PackData(pack));
        }

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
