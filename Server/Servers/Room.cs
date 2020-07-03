using Google.Protobuf.Collections;
using ServerFramework.Clients;
using SocketDemoProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ServerFramework.Servers
{
    class Room
    {
        private RoomPack roomPack;

        private List<Client> clientList = new List<Client>();//房间内所有客户端
        /// <summary>
        /// 返回房间信息
        /// </summary>
        public RoomPack GetRoomPack
        {
            get
            {
                roomPack.CurrentNum = clientList.Count;
                return roomPack;
            }
        }

        public Room(Client client,RoomPack pack)
        {
            this.roomPack = pack;
            clientList.Add(client);
            client.Room = this;
        }

        public RepeatedField<PlayerPack> GetPlayerPacks()
        {
            RepeatedField<PlayerPack> playerPacks = new RepeatedField<PlayerPack>();

            foreach (Client client in clientList)
            {
                playerPacks.Add(client.GetPlayerPack());
            }

            return playerPacks;
        }
        
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="client">不广播给的客户端</param>
        public void Broadcast(Client client,MainPack pack)
        {
            foreach (Client c in clientList)
            {
                if (!c.Equals(client))
                {
                    c.Send(pack);
                    Console.WriteLine("向" + c.Socket.RemoteEndPoint + "广播消息");
                }
            }
        }

        public void Join(Client client)
        {
            clientList.Add(client);
            if (GetRoomPack.CurrentNum>= GetRoomPack.MaxNum)
            {
                GetRoomPack.RoomState = RoomState.Full;
            }
            client.Room = this;
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.PlayerList;
            foreach (PlayerPack playerPack in GetPlayerPacks())
            {
                pack.PlayerPack.Add(playerPack);
            }
            Broadcast(client,pack);
        }

        public void Exit(Server server,Client client)
        {
            MainPack pack = new MainPack();

            if (client==clientList[0])
            {
                //房主退出
                client.Room = null;
                pack.ActionCode = ActionCode.Exit;
                server.RemoveRoom(this);
            }
            else
            {
                clientList.Remove(client);
                roomPack.RoomState = RoomState.Waitting;
                client.Room = null;
                pack.ActionCode = ActionCode.PlayerList;
                foreach (PlayerPack playerPack in GetPlayerPacks())
                {
                    pack.PlayerPack.Add(playerPack);
                }
            }
            Broadcast(client, pack);
        }

        public ReturnCode StartGame(Client client)
        {
            if (client!=clientList[0])
            {
                return ReturnCode.Fail;
            }
            Thread startTimer = new Thread(StartTimerThread);
            startTimer.Start();
            return ReturnCode.Succeed;
        }

        private void StartTimerThread()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.Chat;
            pack.ChatStr = "房主已启动游戏";
            Broadcast(null, pack);
            Thread.Sleep(1000);
            for (int i = 5; i > 0; i--)
            {
                pack.ChatStr = i.ToString();
                Broadcast(null, pack);
                Thread.Sleep(1000);
            }
            pack.ActionCode = ActionCode.ServerStartGame;

            foreach (Client client in clientList)
            {
                PlayerPack playerPack = new PlayerPack();
                playerPack.Hp = client.GetPlayerPack().Hp;
                playerPack.PlayerName = client.GetPlayerPack().PlayerName;
                pack.PlayerPack.Add(playerPack);
            }

            Broadcast(null, pack);
        }

    }
}
