using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace teamScreenClient
{
    public class TeamScreenTcpClient : TcpClient
    {
        public NetworkStream stream;
        public StreamReader reader;
        public StreamWriter writer;
        public int SessionId;
                
        public void _Connect(string ip, int port)
        {
            Connect(ip, port);
            stream = GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
        }

        public Bitmap LastScreen = null;
                
        public void SendImg(Bitmap bmp)
        {            
            Bitmap toSend = bmp;
            bool deltaframe = false;

            if (LastScreen != null)
            {
                deltaframe = true;
                toSend = Stuff.Diff(LastScreen, bmp);
            }
            LastScreen = Stuff.CloneViaCopyBytes(bmp);
            writer.WriteLine("FRAME;" + (deltaframe ? "DELTA" : "FULL"));
            writer.Flush();
            MemoryStream ms = new MemoryStream();
            toSend.Save(ms, ImageFormat.Png);

            var detid = 0;
            int chunkSize = 1024 * 10;
            var bts = ms.ToArray();
            for (int i = 0; i < bts.Length; i += chunkSize)
            {
                var last = bts.Length - i;
                var arr = bts.Skip(i).Take(Math.Min(last, chunkSize)).ToArray();
                string str2 = "CHUNK=" + detid + ";" + bts.Length + ";" + arr.Length + ";" + i + ";";
                var b64 = Convert.ToBase64String(arr);
                var md5 = Stuff.CreateMD5(b64);
                str2 += b64 + ";" + md5;
                
                writer.WriteLine(str2);
                writer.Flush();

            }
            ms.Dispose();
            writer.WriteLine("IMGEND");
            writer.Flush();
        }
    }
}