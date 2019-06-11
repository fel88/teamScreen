using System.Drawing;
using System.Windows.Forms;

namespace teamScreenClient
{
    public class OsEmulatorGuiElement
    {
        public PointF Position;
        public bool IsFocused;

        public virtual void MouseEvent(MouseEventType ev, MouseButtons mb, int x, int y)
        {
            
            
        }

        public virtual void KeyboardEvent(KeyboardEventType down, KeyEventArgs args)
        {


        }


        public virtual void Draw(OsEmulatorDrawingContext dc)
        {

        }
    }
}