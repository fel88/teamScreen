using System.Drawing;
using System.IO;

namespace teamScreenClient
{
    public class OsEmulatorDirectory : OsEmulatorGuiElement
    {
        public static Bitmap DirSimple;
        public static Bitmap DirSelected;
        public bool IsSelected;
        public DirectoryInfo Info { get; set; }
        public bool OverrideName;
        public string Name;
        public override void Draw(OsEmulatorDrawingContext dc)
        {
            var ico1 = DefaultIcons.FolderLarge;
            var bmp1 = Bitmap.FromHicon(ico1.Handle);
            var ico2 = DefaultIcons.FolderLargeSelected;
            var bmp2 = Bitmap.FromHicon(ico2.Handle);
            if (IsSelected)
            {

                dc.Graphics.DrawImage(bmp2, Position.X, Position.Y);
            }
            else
            {
                dc.Graphics.DrawImage(bmp1, Position.X, Position.Y);
            }

            var str = Info.Name;
            if (OverrideName)
            {
                str = Name;
            }
            dc.Graphics.DrawString(str, new Font("Arial", 16), Brushes.Black, Position.X+30,Position.Y);


            base.Draw(dc);
        }
    }
}