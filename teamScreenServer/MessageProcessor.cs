using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace teamScreenServer
{
    public class MessageProcessor
    {
        public static List<Message> Messages = new List<Message>();
        public static void AddMessage(Message n)
        {
            lock (Messages)
            {
                Messages.Add(n);
            }
        }

        public static void Start()
        {
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    Process();
                }
            });
            th.IsBackground = true;
            th.Start();
        }

        public static ClientObject CurrentClient;
        private static void Process()
        {

            if (CurrentClient != null)
            {
                try
                {                    
                    lock (Messages)
                    {
                        if (Messages.Any())
                        {
                            var fr = Messages.First();
                            fr.Send(CurrentClient);
                            Messages.RemoveAt(0);
                        }
                    }

                }
                catch (Exception ex)
                {
                    CurrentClient = null;                    
                }
            }
        }

    }
    public class Message
    {
        public virtual void Send(ClientObject o)
        {

        }
    }
}
