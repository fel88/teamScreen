using System.Drawing;
using System.Windows.Forms;

namespace teamScreenClient
{
    public class OsEmulatorInputBox : OsEmulatorGuiElement
    {
        public override void MouseEvent(MouseEventType ev, MouseButtons mb, int x, int y)
        {
            var rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            if (rect.IntersectsWith(new Rectangle((int)x, (int)y, 1, 1)))
            {
                IsFocused = true;
            }

            base.MouseEvent(ev, mb, x, y);
        }

        public override void KeyboardEvent(KeyboardEventType down, KeyEventArgs args)
        {
            if (down != KeyboardEventType.Up) return;
            if (args.KeyCode == Keys.Back)
            {
                if (Text.Length > 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                }
            }
            else
            {
                Text += args.KeyCode;
            }

            base.KeyboardEvent(down, args);
        }

        public string Text;

        public int Width = 150;
        public int Height = 20;

        public override void Draw(OsEmulatorDrawingContext dc)
        {
            if (!IsFocused)
            {
                dc.Graphics.FillRectangle(Brushes.White, Position.X, Position.Y, Width, Height);
            }
            else
            {
                dc.Graphics.FillRectangle(Brushes.LightGreen, Position.X, Position.Y, Width, Height);
            }

            dc.Graphics.DrawString(Text, new Font("Arial", 16), Brushes.Black, Position.X + 2, Position.Y + 2);
            dc.Graphics.DrawRectangle(Pens.Black, Position.X, Position.Y, Width, Height);
            base.Draw(dc);
        }
    }
}