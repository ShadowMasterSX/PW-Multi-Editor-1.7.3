using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NpcGen_Editor
{
    public class Extensions
    {
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            try
            {

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);

                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }
            catch (Exception e)
            {
                MessageBox.Show("Erro ResizeImage: " + e.Message);
            }
            return destImage;
        }

        public static string TimestampToDateHour(uint timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            origin = origin.AddSeconds(timestamp);
            return origin.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static Color getColorByItemID(int id)
        {
            switch (id)
            {
                case 0:
                    return Color.White;
                case 1:
                    return Color.LightBlue;
                case 2:
                    return Color.LightYellow;
                case 3:
                    return Color.MediumPurple;
                case 4:
                    return Color.Gold;
                case 5:
                    return Color.White;
                case 6:
                    return Color.Gray;
                case 7:
                    return Color.LightGreen;
            }
            return Color.Black;
        }
    }
}
