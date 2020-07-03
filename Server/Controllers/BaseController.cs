using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketDemoProtocol;



namespace ServerFramework.Controller
{
    abstract class BaseController
    {
        protected RequestCode requestCode = RequestCode.RequestNone;

        public RequestCode RequestCode { get => requestCode;}
    }
}
