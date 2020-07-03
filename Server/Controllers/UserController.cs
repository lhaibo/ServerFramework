using ServerFramework.Clients;
using ServerFramework.Servers;
using SocketDemoProtocol;


namespace ServerFramework.Controller
{
    class UserController:BaseController
    {
        public UserController()
        {
            requestCode = RequestCode.User;
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server server,Client client,MainPack pack)
        {
            if(client.Logon(pack))
            {
                pack.ReturnCode = ReturnCode.Succeed;
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            if (client.Login(pack))
            {
                pack.ReturnCode = ReturnCode.Succeed;
                client.Username = pack.LoginPack.Username;
                client.Hp = 100;
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }
            return pack;
        }
    }
}
