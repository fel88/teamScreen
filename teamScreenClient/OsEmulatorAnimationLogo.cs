using System;
using System.Drawing;

namespace teamScreenClient
{
    public class OsEmulatorAnimationLogo : OsEmulatorGuiElement
    {
        public float Angle = 0;
        public override void Draw(OsEmulatorDrawingContext dc)
        {
            Angle+=5;
            Angle %= 360;
            var radns = Angle * Math.PI / 180.0f;
            var rad = 50;
            var xx = Position.X + (float)(rad * Math.Cos(radns));
            var yy = Position.Y + (float)(rad * Math.Sin(radns));
            dc.Graphics.FillEllipse(Brushes.Blue, Position.X - 5, Position.Y - 5, 10, 10);
            dc.Graphics.FillEllipse(Brushes.Blue, xx - 10, yy - 10, 20, 20);
            base.Draw(dc);
        }
    }
}