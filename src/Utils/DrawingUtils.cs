using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ScreenCapture.Utils
{
    public static class DrawingUtils
    {
        public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            try
            {
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;

                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    memory.Close();

                    return bitmapImage;
                }
            }
            catch { return default; }
        }

        public static Bitmap LoadBitmapIntoMemory(string imagePath)
        {
            if (!File.Exists(imagePath))
                return default;

            Bitmap bmp;
            try
            {
                using (var sr = new System.IO.StreamReader(imagePath))
                {
                    bmp = new Bitmap(sr.BaseStream);
                    sr.Close();
                }
            }
            catch { return default; }

            return bmp;
        }

        public static BitmapImage LoadImageIntoMemory(string imagePath)
        {
            if (!File.Exists(imagePath))
                return default;

            BitmapImage bitmapImage;
            try
            {
                using (var sr = new System.IO.StreamReader(imagePath))
                {
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = sr.BaseStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    sr.Close();
                }
            }
            catch { return default; }

            return bitmapImage;
        }
    }
}
