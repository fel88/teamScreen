using System;

namespace teamScreenServer
{
    public class ConnectInfo
    {
        public ClientObject ClientObject;
        public string Ip { get; set; }
        public DateTime ConnectTimestamp { get; set; }        
    }
}