using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml.Linq;


namespace teamScreenServer
{
    public partial class Form1 : Form
    {       
        public Form1()
        {
            InitializeComponent();
            LoadSettings();
            label3.Text = Server.port + "";
            Server.StartServer();
            Server.ImageCaptured += img1;         

            MessageProcessor.Start();     

            KeyPreview = true;
            KeyUp += Form1_KeyUp;            
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;

            pictureBox1.SizeChanged += PictureBox1_SizeChanged;
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;

        }

        void LoadSettings()
        {
            var doc = XDocument.Load("config.xml");
            foreach (var item in doc.Descendants("setting"))
            {
                var nm = item.Attribute("name").Value;
                var vl = item.Attribute("value").Value;
                switch (nm)
                {
                    case "port":
                        Server.port = int.Parse(vl);
                        break;
                }
            }
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (CurrentClient != null)
            {
                AddMessage(new MouseWheelMessage() { Delta = e.Delta });
            }
        }

        public Graphics gr;
        public Bitmap bmp;

        private void PictureBox1_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            gr = Graphics.FromImage(bmp);
            pictureBox1.Image = bmp;
        }                

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (CurrentClient != null)
            {
                e.SuppressKeyPress = true;
                AddMessage(new KeyUpMessage() { Key = e });
            }
        }

        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (CurrentClient != null)
            {
                e.SuppressKeyPress = true;
                AddMessage(new KeyDownMessage() { Key = e });
            }
            base.OnKeyDown(e);
        }
                
                
        private Bitmap bb;
        public void img1(Bitmap b)
        {
            bb = b;            
        }        

        public void UpdateConnectsInfos()
        {            
            var ips = Server.Infos.GroupBy(z => z.Ip).ToArray();
            List<ListViewItem> toDel = new List<ListViewItem>();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (!ips.Any(z => z.Key == listView1.Items[i].Tag as string))
                {
                    toDel.Add(listView1.Items[i]);
                }
            }

            foreach (var listViewItem in toDel)
            {
                listView1.Items.Remove(listViewItem);
            }

            foreach (var ip in ips)
            {
                bool exist = false;
                for (int l = 0; l < listView1.Items.Count; l++)
                {
                    var lvi = listView1.Items[l];
                    var strip = lvi.Tag as string;
                    if (strip == ip.Key)
                    {
                        exist = true;
                        break;
                    }
                }

                if (exist) continue;
                string nm = "";


                listView1.Items.Add(new ListViewItem(new string[] { ip.Key + "", ip.Count() + "" })
                {
                    Tag = ip.Key
                });
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateConnectsInfos();
        }

        public ClientObject CurrentClient
        {
            get { return MessageProcessor.CurrentClient; }
            set { MessageProcessor.CurrentClient = value; }
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var str = listView1.SelectedItems[0].Tag as string;
                var aa = Server.Infos.First(z => z.Ip == str);
                CurrentClient = aa.ClientObject;
                AddMessage(new StartImgMessage());
            }
        }

        
        private int sx, sy;
        private float zoom = 0.5f;
        public void Redraw()
        {
            if (gr == null) return;
            gr.Clear(Color.White);
            if (bb != null)
            {
                gr.DrawImage(bb, new Rectangle(sx, sy, (int)(bb.Width * zoom), (int)(bb.Height * zoom)), new Rectangle(0, 0, bb.Width, bb.Height), GraphicsUnit.Pixel);
            }

            pictureBox1.Invalidate();
        }

        Point lastPoint = new Point();        
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (CurrentClient != null)
            {
                try
                {
                    var pos = pictureBox1.PointToClient(Cursor.Position);
                    if (pictureBox1.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
                    {
                        pos = GetScaledCursor();
                        if (lastPoint.X != pos.X || lastPoint.Y != pos.Y)
                        {
                            lastPoint = pos;
                            MessageProcessor.AddMessage(new MouseMoveMessage(pos));
                        }
                    }
                }
                catch (Exception ex)
                {
                    CurrentClient = null;
                    MessageBox.Show("client lost");
                }
            }
        }

        public Point GetScaledCursor()
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);

            pos.X -= sx;
            pos.Y -= sy;
            pos.X = (int)(pos.X / zoom);
            pos.Y = (int)(pos.Y / zoom);
            return pos;
        }

        public class MouseUpMessage : Message
        {
            public Point Pos;
            public MouseButtons Button;
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("MOUSEUP=" + Pos.X + ";" + Pos.Y + ";" + (int)Button);
                o.wrt.Flush();
            }
        }

        public class KeyDownMessage : Message
        {
            public KeyEventArgs Key;
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("KEYDOWN=" + (int)Key.KeyCode + ";" + Key.Alt + ";" + Key.Shift + ";" + Key.Control);
                o.wrt.Flush();
            }
        }

        public class KeyUpMessage : Message
        {
            public KeyEventArgs Key;
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("KEYUP=" + (int)Key.KeyCode + ";" + Key.Alt + ";" + Key.Shift + ";" + Key.Control);
                o.wrt.Flush();
            }
        }

        public class MouseWheelMessage : Message
        {

            public int Delta;
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("MOUSEWHEEL=" + Delta);
                o.wrt.Flush();
            }
        }

        public class MouseDownMessage : Message
        {
            public Point Pos;
            public MouseButtons Button;
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("MOUSEDOWN=" + Pos.X + ";" + Pos.Y + ";" + (int)Button);
                o.wrt.Flush();
            }
        }
        public class MouseMoveMessage : Message
        {
            public MouseMoveMessage(Point p)
            {
                Pos = p;
            }
            public Point Pos;

            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("MOUSEPOS=" + Pos.X + ";" + Pos.Y);
                o.wrt.Flush();
            }
        }
        public class StartImgMessage : Message
        {
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("STARTIMG");
                o.wrt.Flush();
            }
        }
        public class StopImgMessage : Message
        {
            public override void Send(ClientObject o)
            {
                o.wrt.WriteLine("STOPIMG");
                o.wrt.Flush();
            }
        }
                

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (CurrentClient != null)
            {
                var pos1 = pictureBox1.PointToClient(Cursor.Position);
                AddMessage(new MouseUpMessage() { Button = e.Button, Pos = GetScaledCursor() });
            }
        }

        public void AddMessage(Message m)
        {
            MessageProcessor.AddMessage(m);
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            if (pictureBox1.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                pictureBox1.Focus();
            }
            Redraw();            
        }        

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var pos = Cursor.Position;
            pos = trackBar1.PointToClient(pos);
            if (trackBar1.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {

                var val = 0.2f + (trackBar1.Value / 5f);
                zoom = val;
            }
        }
                

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //Form2 f = new Form2();
            //f.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (CurrentClient != null)
            {
                var pos1 = pictureBox1.PointToClient(Cursor.Position);
                AddMessage(new MouseDownMessage() { Button = e.Button, Pos = GetScaledCursor() });
            }
        }
    }
}
