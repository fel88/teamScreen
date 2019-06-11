using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace teamScreenClient
{
    public class OsScreenCaptureDevice : ScreenCaptureDevice
    {
        public override Bitmap CaptureScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);

            var gr = Graphics.FromImage(bmp);
            gr.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size,
                CopyPixelOperation.SourceCopy);
            var c = Cursor.Position;
            int w = 3;
            gr.DrawEllipse(new Pen(Color.Red, 2), c.X - w, c.Y - w, w * 2, w * 2);
            gr.Dispose();
            return bmp;
        }

        public override void SetCursor(uint x, uint y)
        {
            Cursor.Position = new Point((int)x, (int)y);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, int cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public override void MouseEvent(MouseEventType ev, MouseButtons mb, int x, int y)
        {
            Cursor.Position = new Point((int)x, (int)y);
            if (mb == MouseButtons.Middle)
            {
                if (ev == MouseEventType.Wheel)
                {
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, x, 0);
                }
                if (ev == MouseEventType.Down)
                {
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, x, y, 0, 0);
                }

                if (ev == MouseEventType.Up)
                {
                    mouse_event(MOUSEEVENTF_MIDDLEUP, x, y, 0, 0);
                }
            }

            if (mb == MouseButtons.Left)
            {
                if (ev == MouseEventType.Double)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                    }
                }

                if (ev == MouseEventType.Down)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                }

                if (ev == MouseEventType.Up)
                {
                    mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                }
            }

            if (mb == MouseButtons.Right)
            {
                if (ev == MouseEventType.Down)
                {
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
                }

                if (ev == MouseEventType.Up)
                {
                    mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
                }
            }

        }
        const int VK_UP = 0x26; //up key
        const int VK_DOWN = 0x28;  //down key
        const int VK_LEFT = 0x25;
        const int VK_RIGHT = 0x27;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        int press(byte code)
        {
            //Press the key
            keybd_event((byte)code, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            return 0;
        }
        int release(byte code)
        {
            //0x45 scan code
            //Release the key
            keybd_event((byte)code, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            return 0;
        }
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public override void KeyboardEvent(KeyboardEventType down, KeyEventArgs args)
        {
            var a = (int)args.KeyCode;
            if (down == KeyboardEventType.Down)
            {
                press((byte)a);
            }

            if (down == KeyboardEventType.Up)
            {

                release((byte)a);
            }
        }
    }
}