using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PSImaging
{
    public class Pixel
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;

        public Pixel(byte b, byte g, byte r, byte a)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    public class PixelsData
    {
        public byte[] pixels;
        public int width;
        public int height;

        public PixelsData(int width, int height)
        {
            this.pixels = new byte[width * height * 4];
            this.width = width;
            this.height = height;
        }

        public PixelsData(byte[] pixels, int width, int height)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
        }

        public IList<Pixel> GetNeighborPixels(int x, int y, int distance)
        {
            IList<Pixel> ret = new List<Pixel>();
            for (var yi = y - distance; yi < y + distance; yi++)
            {
                for (var xi = x - distance; xi < x + distance; xi++)
                {
                    if (IsInBounds(xi, yi))
                    {
                        ret.Add(GetPixel(xi, yi));
                    }
                }
            }
            return ret;
        }

        public int GetIndex(int x, int y)
        {
            return (x + y * width) * 4;
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private Pixel GetPixel(int x, int y)
        {
            return new Pixel(pixels[GetIndex(x, y) + 0],
                             pixels[GetIndex(x, y) + 1],
                             pixels[GetIndex(x, y) + 2],
                             pixels[GetIndex(x, y) + 3]);
        }
    }

    public static class PixelUtil
    {
        public static int GetXPosition(int index, int width)
        {
            return index / 4 % width;
        }

        public static int GetYPosition(int index, int width)
        {
            return index / 4 / width;
        }
    }

    public static class BitmapConverter
    {
        public static PixelsData ConvertBitmapToPixels(Bitmap bitmap)
        {
            byte[] byteArray = ConvertBitmapToByteArray(bitmap);
            return new PixelsData(byteArray, bitmap.Width, bitmap.Height);
        }

        private static byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            var ret = new byte[bitmap.Width * bitmap.Height * 4];

            BitmapData bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

            Marshal.Copy(bitmapData.Scan0, ret, 0, ret.Length);

            bitmap.UnlockBits(bitmapData);

            return ret;
        }

        public static Bitmap ConvertPixelsToBitmap(PixelsData pixels)
        {
            return ConvertByteArrayToBitmap(pixels.pixels, pixels.width, pixels.height);
        }

        private static Bitmap ConvertByteArrayToBitmap(byte[] byteArray, int width, int height)
        {
            Bitmap ret = new Bitmap(width, height);

            BitmapData bitmapData = ret.LockBits(
                    new Rectangle(0, 0, ret.Width, ret.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);

            Marshal.Copy(byteArray, 0, bitmapData.Scan0, byteArray.Length);

            ret.UnlockBits(bitmapData);

            return ret;
        }
    }

    public static class ImageFormatResolver
    {
        public static ImageFormat ResolveFromExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".jpg":
                case ".jpeg": return ImageFormat.Jpeg;
                case ".png":
                default: return ImageFormat.Png;
            }
        }
    }
}