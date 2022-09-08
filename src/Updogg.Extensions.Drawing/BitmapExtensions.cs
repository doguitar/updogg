using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Updogg.Extensions.Drawing
{
    public static class BitmapExtensions
    {
        public static Bitmap Resize(this Bitmap original, int width, int height)
        {
            Rectangle destRect = new(0, 0, width, height);
            Bitmap? destImage = new(width, height);

            destImage.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using (Graphics? graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using ImageAttributes? wrapMode = new();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }
    }
}
