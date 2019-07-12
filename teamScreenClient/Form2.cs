using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace teamScreenClient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            LoadConfig();
            textBox2.Text = GetLocalIPAddress();
        }

        private void LoadConfig()
        {
            if (!File.Exists("config.xml")) return;
            var doc = XDocument.Load("config.xml");
            foreach (var descendant in doc.Descendants("setting"))
            {
                var nm = descendant.Attribute("name").Value;
                var vl = descendant.Attribute("value").Value;
                switch (nm)
                {
                    case "port":
                        textBox3.Text = vl;
                        break;
                    case "ip":
                        textBox1.Text = vl;
                        break;
                }
                
            }
        }

                

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


        private TeamScreenTcpClient client = null;
        private int cnt = 0;
        public ScreenCaptureDevice Device = new EmulatorScreenCaptureDevice();

        ManualResetEvent mre = new ManualResetEvent(false);
        private bool isImgSendPause = true;

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            if (!checkBox1.Checked)
            {
                Device = new OsScreenCaptureDevice();
            }


            try
            {
                client = new TeamScreenTcpClient();
                client._Connect(textBox1.Text, int.Parse(textBox3.Text));

                Thread th2 = new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {

                            if (isImgSendPause)
                            {
                                mre.WaitOne();
                            }

                            var screen = Device.CaptureScreen();
                            client.SendImg(screen);                            
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                });
                th2.IsBackground = true;
                th2.Start();
                Thread th = new Thread(() =>
                  {
                      try
                      {
                          while (true)
                          {
                              var ln = client.reader.ReadLine();

                              if (ln == "STARTIMG")
                              {
                                  isImgSendPause = false;
                                  mre.Set();
                              }
                              if (ln == "STOPIMG")
                              {
                                  isImgSendPause = true;
                              }                              

                              if (ln.StartsWith("MOUSEPOS"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var x = uint.Parse(aa[0]);
                                  var y = uint.Parse(aa[1]);
                                  Device.SetCursor(x, y);
                                  cnt++;
                              }

                              if (ln.StartsWith("MOUSEUP"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var x = int.Parse(aa[0]);
                                  var y = int.Parse(aa[1]);

                                  var mb = (MouseButtons)(uint.Parse(aa[2]));
                                  Device.MouseEvent(MouseEventType.Up, mb, x, y);


                                  cnt++;

                              }
                              if (ln.StartsWith("MOUSEDOWN"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var x = int.Parse(aa[0]);
                                  var y = int.Parse(aa[1]);

                                  var mb = (MouseButtons)(uint.Parse(aa[2]));
                                  Device.MouseEvent(MouseEventType.Down, mb, x, y);


                                  cnt++;

                              }
                              if (ln.StartsWith("MOUSEDOUBLE"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var x = int.Parse(aa[0]);
                                  var y = int.Parse(aa[1]);

                                  var mb = (MouseButtons)(uint.Parse(aa[2]));
                                  Device.MouseEvent(MouseEventType.Double, mb, x, y);


                                  cnt++;

                              }

                              if (ln.StartsWith("KEYDOWN"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var keycode = (Keys)int.Parse(aa[0]);
                                  var alt = bool.Parse(aa[1]);
                                  var shift = bool.Parse(aa[2]);
                                  var control = bool.Parse(aa[3]);


                                  KeyEventArgs args = new KeyEventArgs(keycode);

                                  Device.KeyboardEvent(KeyboardEventType.Down, args);


                                  cnt++;

                              }
                              if (ln.StartsWith("KEYUP"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var keycode = (Keys)int.Parse(aa[0]);
                                  var alt = bool.Parse(aa[1]);
                                  var shift = bool.Parse(aa[2]);
                                  var control = bool.Parse(aa[3]);


                                  KeyEventArgs args = new KeyEventArgs(keycode);

                                  Device.KeyboardEvent(KeyboardEventType.Up, args);


                                  cnt++;

                              }
                              if (ln.StartsWith("MOUSEWHEEL"))
                              {

                                  var spl1 = ln.Split(new char[] { '=' }).ToArray();
                                  var a1 = spl1[1];
                                  var aa = a1.Split(new char[] { ';' }).ToArray();
                                  var delta = int.Parse(aa[0]);

                                  Device.MouseEvent(MouseEventType.Wheel, MouseButtons.Middle, delta, 0);


                                  cnt++;

                              }

                          }
                      }
                      catch (Exception ex)
                      {
                          
                          MessageBox.Show("Connection lost");
                          button1.Enabled = true;
                      }
                  });
                th.IsBackground = true;
                th.Start();
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message, Text);
            }
        }


        public static DialogResult ShowError(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
    public enum MouseEventType
    {
        Down, Up, Wheel, Double
    }
    public enum KeyboardEventType
    {
        Down, Up
    }
}
