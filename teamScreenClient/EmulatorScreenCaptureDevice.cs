using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace teamScreenClient
{
    public class EmulatorScreenCaptureDevice : ScreenCaptureDevice
    {
        public EmulatorScreenCaptureDevice()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            var gr = Graphics.FromImage(bmp);
            ctx.Graphics = gr;
            ctx.Bitmap = bmp;

            UpdateDirectoriesElements("C:\\");

            Elements.Add(new OsEmulatorInputBox() { Position = new PointF(500, 20) });
            Elements.Add(new OsEmulatorAnimationLogo() { Position = new PointF(Width - 800, 100) });
        }

        public Point Cursor;
        public int Width = 1920;
        public int Height = 1080;
        public List<OsEmulatorGuiElement> Elements = new List<OsEmulatorGuiElement>();
        OsEmulatorDrawingContext ctx = new OsEmulatorDrawingContext();
        public Bitmap DesktopBackground;

        public void UpdateDirectoriesElements(string path)
        {
            lock (Elements)
            {


                int xx = 0;
                int yy = 0;
                Elements.RemoveAll(z => z is OsEmulatorDirectory || z is OsEmulatorFile);
                try
                {
                    var dir = new DirectoryInfo(path);

                    if (dir.Parent != null)
                    {
                        OsEmulatorDirectory back = new OsEmulatorDirectory();
                        back.Info = dir.Parent;
                        back.OverrideName = true;
                        back.Name = "..";
                        Elements.Add(back);
                        yy += 30;
                    }

                    foreach (var directoryInfo in dir.GetDirectories())
                    {
                        OsEmulatorDirectory s = new OsEmulatorDirectory();
                        s.Info = directoryInfo;
                        s.Position = new PointF(xx, yy);
                        yy += 30;

                        Elements.Add(s);
                    }

                    foreach (var directoryInfo in dir.GetFiles())
                    {
                        OsEmulatorFile s = new OsEmulatorFile();
                        s.Info = directoryInfo;
                        s.Position = new PointF(xx, yy);
                        yy += 30;
                        Elements.Add(s);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public override Bitmap CaptureScreen()
        {
            ctx.Graphics.Clear(Color.CadetBlue);
            lock (Elements)
            {
                foreach (var element in Elements)
                {
                    element.Draw(ctx);
                }
            }

            int w = 3;
            var c = Cursor;
            ctx.Graphics.DrawEllipse(new Pen(Color.Red, 2), c.X - w, c.Y - w, w * 2, w * 2);            
            return (Stuff.CloneViaCopyBytes(ctx.Bitmap));
        }

        public override void SetCursor(uint x, uint y)
        {
            Cursor = new Point((int)x, (int)y);
        }

        public override void MouseEvent(MouseEventType ev, MouseButtons mb, int x, int y)
        {
            foreach (var osEmulatorGuiElement in Elements)
            {
                osEmulatorGuiElement.IsFocused = false;
                osEmulatorGuiElement.MouseEvent(ev, mb, x, y);
            }
            if (mb == MouseButtons.Left && ev == MouseEventType.Up)
            {
                bool was = false;
                OsEmulatorDirectory selected = null;
                foreach (var directory in Elements.OfType<OsEmulatorDirectory>())
                {
                    if (directory.IsSelected)
                    {
                        selected = directory;

                    }
                    directory.IsSelected = false;
                }

                DirectoryInfo toOpen = null;
                foreach (var directory in Elements.OfType<OsEmulatorDirectory>())
                {
                    Rectangle r = new Rectangle((int)directory.Position.X, (int)directory.Position.Y, 30, 30);
                    if (r.IntersectsWith(new Rectangle(Cursor.X, Cursor.Y, 1, 1)))
                    {
                        if (selected == directory)
                        {
                            toOpen = directory.Info;
                            was = true;
                        }
                        directory.IsSelected = true;

                    }

                }

                if (was)
                {
                    UpdateDirectoriesElements(toOpen.FullName);
                }

            }
        }

        public override void KeyboardEvent(KeyboardEventType down, KeyEventArgs args)
        {
            var fr = Elements.FirstOrDefault(z => z.IsFocused);
            if (fr != null)
            {
                fr.KeyboardEvent(down, args);
            }
        }       
    }
}