using NetMQ;
using NetMQ.Sockets;
using System;


namespace Mechmind_CameraAPI_Csharp
{
    class ZmqClient
    {
      
           
        const int port = 5577;
        RequestSocket client;
        private string addr = "";
        private byte[] reqbuf;
        private byte[] resbuf;
        public ZmqClient()
        { 
            client = new RequestSocket();
        }
        public int setAddr(string ip)
        {
            if (ip.Length == 0)
                return -1;
            if (addr.Length != 0)
                client.Disconnect(addr);
            addr = "tcp://" + ip + ":" + port.ToString();
            Console.WriteLine("Connect to " + addr);
            client.Connect(addr);
            return 0;
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
                reqbuf = System.Text.Encoding.Default.GetBytes(request); ;
                client.SendFrame(reqbuf);
                resbuf = client.ReceiveFrameBytes();
                return resbuf;
            }  
            catch (Exception e)
            {
                Console.WriteLine("Network Error. Please check your ip address and network connection!");
                System.Environment.Exit(0);
            }
            return null;
        }
        
        
        
    }
}
