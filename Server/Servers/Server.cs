using ServerFramework.Clients;
using ServerFramework.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServerFramework.DAO;
using SocketDemoProtocol;

namespace ServerFramework.Servers
{
    class Server
    {
        private Socket socket;
        private List<Client> clients;
        private List<Room> rooms;
        private ControllerManager controllerManager;
        
        public Server(string ip,int endPoint,int maxNumClients)
        {
            controllerManager = new ControllerManager(this);
            clients = new List<Client>();
            rooms = new List<Room>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), endPoint));
            socket.Listen(maxNumClients);
            StartAccept();
        }

        private void StartAccept()
        {
            socket.BeginAccept(AcceptCallBack, null);
            Console.WriteLine(DateTime.Now+":开始等待"+clients.Count+"号连接....");
        }

        

        private void AcceptCallBack(IAsyncResult ar)
        {
            Socket client = socket.EndAccept(ar);
            Console.WriteLine(DateTime.Now+":"+client.RemoteEndPoint+ "已连接....");
            clients.Add(new Client(client,this));
            Console.WriteLine("已添加进客户端列表");
            StartAccept();
        }

        public void HandleRequest(MainPack pack,Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }

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

        public void RemoveClient(Client client)
        {
            clients.Remove(client);
        }

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

        public void RemoveRoom(Room room)
        {
            rooms.Remove(room);
        }

        public void Chat(Client client,MainPack pack)
        {
            pack.ChatStr = client.Username + ":" + pack.ChatStr;
            client.Room.Broadcast(client, pack);
        }
    }
}
