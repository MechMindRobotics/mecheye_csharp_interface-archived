using NetMQ;
using NetMQ.Sockets;
using System;


namespace Mechmind_CameraAPI_Csharp
{
    class ZmqClient
    {
        const int PORT = 5577;
        RequestSocket client;
        private string addr = "";
        private byte[] reqbuf;
        private byte[] resbuf;
        public ZmqClient()
        { 
            client = new RequestSocket();
        }
        public Status setAddr(string ip)
        {
            if (ip.Length == 0)
                return Status.Error;
            if (addr.Length != 0)
                client.Disconnect(addr);
            addr = "tcp://" + ip + ":" + PORT.ToString();
            Console.WriteLine("Connect to " + addr);
            try
            {
                client.Connect(addr);
            }
            catch (Exception e)
            {
                Console.WriteLine("Network Error.Please check your IP address and network connection!");
                return Status.Error;
            }
            return Status.Success;
        }
        public void disconnect()
        {
            client.Disconnect(addr);
            addr = "";
        }
        public bool empty()
        {
            return addr.Length == 0;
        }
        public byte[] sendReq(string request)
        {
            try
            {
                reqbuf = System.Text.Encoding.Default.GetBytes(request); 
                client.SendFrame(reqbuf);
                client.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(Constant.TIMEOUTMS),out resbuf);
            }
            catch (Exception e)
            {
                Console.WriteLine("Receive Error. Please assure your IP address and network connection and review the camera settings!");
                System.Environment.Exit(0);
            }
            return resbuf;
        }
    }
}
