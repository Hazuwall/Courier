using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Courier
{
    public static class DrawingHelper
    {
        public static byte[] ArrayToImageData(double[] array)
        {
            double max = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] > max)
                    max = array[i];

            int length = array.Length;
            length += 4 - (length % 4);
            byte[] bytes = new byte[length];
            for (int i = 0; i < array.Length; i++)
                bytes[i] = (byte)(array[i] / max * 255);
            return bytes;
        }

        public static byte[] ArrayToImageData(double[,] array)
        {
            double max = 0;
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    if (array[i, j] > max)
                        max = array[i, j];

            int stride = array.GetLength(1);
            stride += 4 - (stride % 4);
            byte[] bytes = new byte[array.GetLength(0) * stride];
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    bytes[i * stride + j] = (byte)(array[i, j] / max * 255);
            return bytes;
        }

        public static BitmapImage ToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save("1.bmp", ImageFormat.Bmp);
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static BitmapImage ToImageSource(byte[] imageData, int width, int height)
        {
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
            {
                PutInBitmap(imageData, bitmap);
                bitmap.Palette = ConvertPaletteToGrayScale(bitmap.Palette);
                return ToImageSource(bitmap);
            }
        }

        public static void PutInBitmap(byte[] imageData, Bitmap bitmap)
        {
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0,
                bitmap.Width,
                bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            
            IntPtr pointer = bmpData.Scan0;
            Marshal.Copy(imageData, 0, pointer, imageData.Length);

            bitmap.UnlockBits(bmpData);
        }

        public static ColorPalette ConvertPaletteToGrayScale(ColorPalette palette)
        {
            Color[] entries = palette.Entries;
            for (int i = 0; i < 256; i++)
                entries[i] = Color.FromArgb((byte)i, (byte)i, (byte)i);
            return palette;
        }
    }
}
