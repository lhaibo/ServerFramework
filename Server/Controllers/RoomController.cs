using ServerFramework.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketDemoProtocol;
using ServerFramework.Servers;
using ServerFramework.Clients;

namespace ServerFramework.Controllers
{
    class RoomController:BaseController
    {
        
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }

        public MainPack CreatRoom(Server server, Client client, MainPack pack)
        {
            return server.CreatRoom(client, pack);
        }

        public MainPack SearchRoom(Server server, Client client, MainPack pack)
        {
            return server.SearchRoom();
        }

        public MainPack JoinRoom(Server server, Client client, MainPack pack)
        {
            return server.JoinRoom(client, pack);
        }

        public MainPack Exit(Server server, Client client, MainPack pack)
        {
            return server.ExitRoom(client, pack);
        }

        public MainPack Chat(Server server,Client client,MainPack pack)
        {
            server.Chat(client, pack);
            return null;
        }

        public MainPack StartGame(Server server, Client client, MainPack pack)
        {
            pack.ReturnCode= client.Room.StartGame(client);
            return pack;
        }

    }
}
