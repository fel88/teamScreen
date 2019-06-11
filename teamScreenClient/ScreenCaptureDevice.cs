using System.Drawing;
using System.Windows.Forms;

namespace teamScreenClient
{
    public abstract class ScreenCaptureDevice
    {
        public abstract Bitmap CaptureScreen();
        public abstract void SetCursor(uint u, uint u1);
        public abstract void MouseEvent(MouseEventType ev, MouseButtons mb, int x, int y);

        public abstract void KeyboardEvent(KeyboardEventType down, KeyEventArgs args);

    }
}