using ServerFramework.Clients;
using ServerFramework.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SocketDemoProtocol;

namespace ServerFramework.Servers
{
    class Server
    {
        /// <summary>
        /// 服务端socket
        /// </summary>
        private Socket socket;

        public UDPServer udpServer;

        /// <summary>
        /// 所有连接的客户端
        /// </summary>
        private List<Client> clients;
        /// <summary>
        /// 所有用户创建的房间
        /// </summary>
        private List<Room> rooms;
        /// <summary>
        /// 控制层
        /// </summary>
        private ControllerManager controllerManager;



        /// <summary>
        /// 初始化服务器并开始监听端口
        /// </summary>
        /// <param name="ip">监听的ip</param>
        /// <param name="endPoint">监听的端口</param>
        /// <param name="maxNumClients">最大客户端连接数</param>
        public Server(string ip,int endPoint,int maxNumClients)
        {
            controllerManager = new ControllerManager(this);
            clients = new List<Client>();
            rooms = new List<Room>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), endPoint));
            socket.Listen(maxNumClients);
            StartAccept();
            Console.WriteLine("tcp服务已启动");
            udpServer = new UDPServer(6667, this, controllerManager);
        }

        /// <summary>
        /// 开始异步接收
        /// </summary>
        private void StartAccept()
        {
            socket.BeginAccept(AcceptCallBack, null);
            //Console.WriteLine(DateTime.Now+":开始等待"+clients.Count+"号连接....");
        }
        
        /// <summary>
        /// 接收连接回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {
            Socket client = socket.EndAccept(ar);
            //Console.WriteLine(DateTime.Now+":"+client.RemoteEndPoint+ "已连接....");
            clients.Add(new Client(client,this));
            //Console.WriteLine("已添加进客户端列表");
            StartAccept();
        }

        public Client GetClientFromUsername(string username)
        {
            foreach (Client c in clients)
            {
                if (c.Username==username)
                {
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="pack">处理请求需要的数据包</param>
        /// <param name="client">处理请求的client</param>
        public void HandleRequest(MainPack pack,Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="client">创建房间的client</param>
        /// <param name="mainPack">数据包</param>
        /// <returns></returns>
        public MainPack CreatRoom(Client client,MainPack mainPack)
        {
            try
            {
                Room room = new Room(client, mainPack.RoomPack[0]);
                rooms.Add(room);
                foreach (PlayerPack playerPack in room.GetPlayerPacks())
                {
                    mainPack.PlayerPack.Add(playerPack);
                }
                mainPack.ReturnCode = ReturnCode.Succeed;
                return mainPack;
            }
            catch (Exception)
            {
                mainPack.ReturnCode = ReturnCode.Fail;

                return mainPack;
            }
            
        }

        /// <summary>
        /// 删除客户端
        /// </summary>
        /// <param name="client">要删除的client</param>
        public void RemoveClient(Client client)
        {
            clients.Remove(client);
        }

        /// <summary>
        /// 查找房间
        /// </summary>
        /// <returns>含有所有房间的数据包</returns>
        public MainPack SearchRoom()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.SearchRoom;
            try
            {
                if (rooms.Count>0)
                {
                    foreach (Room room in rooms)
                    {
                        pack.ActionCode = ActionCode.SearchRoom;
                        pack.RoomPack.Add(room.GetRoomPack);
                    }
                }
                pack.ReturnCode = ReturnCode.Succeed;
            }
            catch (Exception)
            {
                pack.ReturnCode = ReturnCode.Fail;
            }

            return pack;
        }

        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="client">要加入房间的client</param>
        /// <param name="pack">包含要加入房间的房间名的数据包</param>
        /// <returns>包含ReturnCode的数据包</returns>
        public MainPack JoinRoom(Client client,MainPack pack)
        {
            foreach (Room room in rooms)
            {
                //找到房间
                if (room.GetRoomPack.RoomName.Equals(pack.JoinRoomName))
                {
                    if (room.GetRoomPack.RoomState==RoomState.Waitting)
                    {
                        //可以加入
                        room.Join(client);
                        pack.RoomPack.Add(room.GetRoomPack);
                        foreach (PlayerPack playerPack in room.GetPlayerPacks())
                        {
                            pack.PlayerPack.Add(playerPack);
                        }
                        pack.ReturnCode = ReturnCode.Succeed;
                        return pack;
                    }
                    else
                    {
                        //不可加入
                        pack.ReturnCode = ReturnCode.Fail;
                        return pack;
                    }
                }
            }
            //没有此房间
            pack.ReturnCode = ReturnCode.NoRoom;
            return pack;
        }

        /// <summary>
        /// 退出房间
        /// </summary>
        /// <param name="client">退出房间的client</param>
        /// <param name="pack">client发过来的数据包</param>
        /// <returns>包含ReturnCode的数据包</returns>
        public MainPack ExitRoom(Client client, MainPack pack)
        {
            if (client.Room==null)
            {
                pack.ReturnCode = ReturnCode.Fail;
                return null;
            }
            else
            {
                client.Room.Exit(this,client);
                pack.ReturnCode = ReturnCode.Succeed;
                return pack;
            }
        }

        /// <summary>
        /// 删除房间
        /// </summary>
        /// <param name="room"></param>
        public void RemoveRoom(Room room)
        {
            rooms.Remove(room);
        }

        /// <summary>
        /// 聊天
        /// </summary>
        /// <param name="client">发送聊天消息的client</param>
        /// <param name="pack">包含聊天消息的数据包</param>
        public void Chat(Client client,MainPack pack)
        {
            pack.ChatStr = client.Username + ":" + pack.ChatStr;
            client.Room.Broadcast(client, pack);
        }

    }
}
