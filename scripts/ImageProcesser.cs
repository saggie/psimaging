using System.Collections.Generic;
using System.Linq;

namespace PSImaging
{
    public abstract class ImageProcesser
    {
        public abstract PixelsData Process(PixelsData sourcePixels);
    }

    public class GrayScaleConverter : ImageProcesser
    {
        public override PixelsData Process(PixelsData source)
        {
            var ret = new PixelsData(source.width, source.height);
            for (var i = 0; i < ret.pixels.Length; i += 4)
            {
                var grayValue = (byte)((source.pixels[i + 0] +
                                        source.pixels[i + 1] +
                                        source.pixels[i + 2]) / 3);
                ret.pixels[i + 0] = grayValue;
                ret.pixels[i + 1] = grayValue;
                ret.pixels[i + 2] = grayValue;
                ret.pixels[i + 3] = source.pixels[i + 3]; // keep the alpha value as-is
            }
            return ret;
        }
    }

    public class FrameAdder : ImageProcesser
    {
        public int borderSize = 1;

        public override PixelsData Process(PixelsData source)
        {
            var newWidth = source.width + borderSize * 2;
            var newHeight = source.height + borderSize * 2;
            var ret = new PixelsData(newWidth, newHeight);

            var sourceIndex = 0;
            for (var drawingIndex = 0; drawingIndex < ret.pixels.Length; drawingIndex += 4)
            {
                // when drawing pixel is in border, set the value to black
                if (IsInBorder(drawingIndex, newWidth, source.width, source.height))
                {
                    ret.pixels[drawingIndex + 0] = 0;
                    ret.pixels[drawingIndex + 1] = 0;
                    ret.pixels[drawingIndex + 2] = 0;
                    ret.pixels[drawingIndex + 3] = 0xFF;
                }
                else
                {
                    ret.pixels[drawingIndex + 0] = source.pixels[sourceIndex + 0];
                    ret.pixels[drawingIndex + 1] = source.pixels[sourceIndex + 1];
                    ret.pixels[drawingIndex + 2] = source.pixels[sourceIndex + 2];
                    ret.pixels[drawingIndex + 3] = source.pixels[sourceIndex + 3];
                    sourceIndex += 4;
                }
            }
            return ret;
        }

        private bool IsInBorder(int drawingIndex, int newWidth,
                                int originalWidth, int originalHeight)
        {
            return PixelUtil.GetXPosition(drawingIndex, newWidth) < borderSize ||
                   PixelUtil.GetXPosition(drawingIndex, newWidth) >= (originalWidth + borderSize) ||
                   PixelUtil.GetYPosition(drawingIndex, newWidth) < borderSize ||
                   PixelUtil.GetYPosition(drawingIndex, newWidth) >= (originalHeight + borderSize);
        }
    }

    public class MedianFilter : ImageProcesser
    {
        public int distance = 1;

        public override PixelsData Process(PixelsData source)
        {
            var ret = new PixelsData(source.width, source.height);
            for (var yi = 0; yi < ret.height; yi++)
            {
                for (var xi = 0; xi < ret.width; xi++)
                {
                    var neighborPixels = source.GetNeighborPixels(xi, yi, distance);
                    var medianPixel = neighborPixels.OrderBy(pixel => pixel.A + pixel.G + pixel.B)
                                                    .ElementAt(neighborPixels.Count / 2);
                    ret.pixels[ret.GetIndex(xi, yi) + 0] = medianPixel.B;
                    ret.pixels[ret.GetIndex(xi, yi) + 1] = medianPixel.G;
                    ret.pixels[ret.GetIndex(xi, yi) + 2] = medianPixel.R;
                    ret.pixels[ret.GetIndex(xi, yi) + 3] = 0xFF; // ignore alpha channel for this filter
                }
            }
            return ret;
        }
    }

    public class ColorReplacer : ImageProcesser
    {
        private Pixel pixelFrom;
        private Pixel pixelTo;

        public override PixelsData Process(PixelsData source)
        {
            var ret = source.Copy();
            for (var i = 0; i < ret.pixels.Length; i += 4)
            {
                var drawingPixel = new Pixel(source.pixels[i + 0],
                                             source.pixels[i + 1],
                                             source.pixels[i + 2],
                                             source.pixels[i + 3]);
                if (drawingPixel.HasSameRgb(pixelFrom))
                {
                    ret.pixels[i + 0] = pixelTo.B;
                    ret.pixels[i + 1] = pixelTo.G;
                    ret.pixels[i + 2] = pixelTo.R;
                    ret.pixels[i + 3] = source.pixels[i + 3]; // keep the alpha value as-is
                }
            }
            return ret;
        }

        public void SetColorsToReplace(string from, string to)
        {
            this.pixelFrom = new Pixel(from);
            this.pixelTo = new Pixel(to);
        }
    }
}
