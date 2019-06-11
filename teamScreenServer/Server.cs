using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace teamScreenServer
{
    public class Server
    {
        public static int port = 7701;
        public static TcpListener listener;


        public static List<ConnectInfo> Infos = new List<ConnectInfo>();
        public static int Connects = 0;
        public static int CommandsProcessed = 0;
        public static Thread MainThread;
        public static void StartServer()
        {

            MainThread = new Thread(() =>
           {
               try
               {
                   listener = new TcpListener(IPAddress.Any, port);
                   listener.Start();

                   while (true)
                   {
                       TcpClient client = listener.AcceptTcpClient();
                       var addr = (client.Client.RemoteEndPoint as IPEndPoint).Address;
                       var ip = addr.ToString();

                       lock (Infos)
                       {
                           Infos.Add(new ConnectInfo() { Ip = addr.ToString() });
                       }                       

                       var clientObject = new ClientObject(client, Server.Infos.Last());

                       Thread clientThread = new Thread(new ThreadStart(clientObject.Process));

                       clientThread.Start();
                       Connects++;

                   }
               }
               catch (Exception ex)
               {
                   Console.WriteLine(ex.Message);
               }
               finally
               {
                   if (listener != null)
                       listener.Stop();
               }
           });

            MainThread.IsBackground = true;
            MainThread.Start();

        }

        public static Action<Bitmap> ImageCaptured;
        public static void OnImageCaptured(Bitmap bitmap)
        {
            if (ImageCaptured != null)
            {
                ImageCaptured(bitmap);
            }
        }
    }

    public class PingCommandProcessor : CommandProcessor
    {
        public override bool Process(CommandContext ctx)
        {
            var wrt = ctx.Writer;
            var str = ctx.Command;
            if (str.StartsWith("PING"))
            {
                wrt.WriteLine(true.ToString());
                wrt.Flush();
                return true;
            }
            return false;
        }
    }

    public class FrameCommandProcessor : CommandProcessor
    {


        public override bool Process(CommandContext ctx)
        {
            var str = ctx.Command;
            if (str.StartsWith("FRAME"))
            {
                var ar1 = str.Split(new char[] { ';' }).ToArray();
                ChunkCommandProcessor.IsDelta = false;
                if (ar1[1] == "DELTA")
                {
                    ChunkCommandProcessor.IsDelta = true;
                }
                
                return true;
            }
            return false;
        }
    }


    public abstract class CommandProcessor
    {
        public abstract bool Process(CommandContext ctx);
    }
    public class CommandContext
    {
        public StreamWriter Writer;
        public StreamReader Reader;
        public ClientObject Ctx;
        public ConnectInfo Info;
        public string Command;
    }
}
