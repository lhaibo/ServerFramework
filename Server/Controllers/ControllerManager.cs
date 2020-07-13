using SocketDemoProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ServerFramework.Servers;
using ServerFramework.Clients;
using ServerFramework.Controllers;

namespace ServerFramework.Controller
{
    class ControllerManager
    {
        private Dictionary<RequestCode, BaseController> controllerDictionary = new Dictionary<RequestCode, BaseController>();

        private Server server;

        public ControllerManager(Server server)
        {
            this.server = server;
            UserController userController = new UserController();
            controllerDictionary.Add(userController.RequestCode, userController);
            RoomController roomController = new RoomController();
            controllerDictionary.Add(roomController.RequestCode, roomController);
            GameController gameController = new GameController();
            controllerDictionary.Add(gameController.RequestCode, gameController);
        }

        public void HandleRequest(MainPack pack,Client client,bool isUdp=false)
        {
            if(controllerDictionary.TryGetValue(pack.RequestCode,out BaseController controller))
            {
                string methodName = pack.ActionCode.ToString();
                MethodInfo method = controller.GetType().GetMethod(methodName);
                if (method==null)
                {
                    Console.WriteLine("没有找到指定的事件处理" + pack.ActionCode.ToString());
                    return;
                }
                object[] obj;

                if (isUdp)
                {
                    obj = new object[] { client, pack };
                    method.Invoke(controller, obj);
                }
                else
                {
                    obj = new object[] { server, client, pack };
                    object ret = method.Invoke(controller, obj);
                    if (ret != null)
                    {
                        client.Send(ret as MainPack);
                    }
                }
            }
            else
            {
                Console.WriteLine("没有找到对应的Controller处理");
            }
        }
    }
}
