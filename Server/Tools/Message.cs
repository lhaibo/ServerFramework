using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketDemoProtocol;
using Google.Protobuf;
namespace ServerFramework.Tools
{
    class Message
    {
        private byte[] buffer = new byte[1024];

        private int startIndex;

        public byte[] Buffer 
        { 
            get => buffer; 
            set => buffer = value; 
        }
        public int StartIndex 
        { 
            get => startIndex; 
            set => startIndex = value; 
        }

        public int RemSize
        {
            get => buffer.Length - startIndex;
        }

        public void ReadBuffer(int len,Action<MainPack> HandleRequest)
        {
            startIndex += len;
            if (startIndex <= 4)
            {
                return;
            }
            int count = BitConverter.ToInt32(buffer, 0);
            while (true)
            {
                if (startIndex>=(count+4))
                {
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, count);
                    HandleRequest(pack);
                    Array.Copy(buffer, count + 4, buffer, 0, startIndex - count - 4);
                    startIndex -= (count + 4);
                }
                else
                {
                    break;
                }
            }
        
        }

        public static byte[] PackData(MainPack pack)
        {
            byte[] data = pack.ToByteArray();
            byte[] head = BitConverter.GetBytes(data.Length);
            return head.Concat(data).ToArray();
        }
    }
}
