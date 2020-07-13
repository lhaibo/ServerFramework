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
    class GameController:BaseController
    {
        public GameController()
        {
            requestCode = RequestCode.Game;
        }

        public MainPack GameExit(Server server,Client client,MainPack pack)
        {
            client.Room.GameExit(server,client);
            return null;
        }

        public MainPack UpdatePos(Client client, MainPack pack)
        {
            client.Room.BroadcastTo(client, pack);
            return null;
        }

        public MainPack Shoot(Client client, MainPack pack)
        {
            client.Room.BroadcastTo(client, pack);
            return null;
        }

    }
}
