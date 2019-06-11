using System.Drawing;
using System.IO;

namespace teamScreenClient
{
    public class OsEmulatorFile: OsEmulatorGuiElement
    {
        
        public bool IsSelected;
        public FileInfo Info { get; set; }

        public override void Draw(OsEmulatorDrawingContext dc)
        {
            var ico = Icon.ExtractAssociatedIcon(Info.FullName);
            var bmp = Bitmap.FromHicon(ico.Handle);
            if (IsSelected)
            {

                dc.Graphics.DrawImage(bmp, Position.X, Position.Y);
            }
            else
            {
                dc.Graphics.DrawImage(bmp, Position.X, Position.Y);
            }
            dc.Graphics.DrawString(Info.Name, new Font("Arial", 16), Brushes.Black, Position.X + 30, Position.Y);


            base.Draw(dc);
        }
    }
}