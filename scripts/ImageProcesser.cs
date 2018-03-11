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

    public class EdgeDrawer : ImageProcesser
    {
        private const bool X = true;
        private const bool _ = false;

        private bool[][][] masks = new bool[][][]
        {
            // level 0
            new bool[][]
            {
                new bool[] {_, X, _},
                new bool[] {X, X, X},
                new bool[] {_, X, _}
            },
            // level 1
            new bool[][]
            {
                new bool[] {X, X, X},
                new bool[] {X, X, X},
                new bool[] {X, X, X}
            },
            // level 2
            new bool[][]
            {
                new bool[] {_, _, X, _, _},
                new bool[] {_, X, X, X, _},
                new bool[] {X, X, X, X, X},
                new bool[] {_, X, X, X, _},
                new bool[] {_, _, X, _, _}
            },
            // level 3
            new bool[][]
            {
                new bool[] {_, X, X, X, _},
                new bool[] {X, X, X, X, X},
                new bool[] {X, X, X, X, X},
                new bool[] {X, X, X, X, X},
                new bool[] {_, X, X, X, _}
            },
            // level 4
            new bool[][]
            {
                new bool[] {X, X, X, X, X},
                new bool[] {X, X, X, X, X},
                new bool[] {X, X, X, X, X},
                new bool[] {X, X, X, X, X},
                new bool[] {X, X, X, X, X}
            },
        };

        private int level = 0;
        private Pixel drawingColor = new Pixel("000000");
        private Pixel backGroundColor = new Pixel("FFFFFF");

        public override PixelsData Process(PixelsData source)
        {
            var ret = source.Copy();
            for (var yi = 0; yi < ret.height; yi++)
            {
                for (var xi = 0; xi < ret.width; xi++)
                {
                    if (source.GetPixel(xi, yi).Equals(backGroundColor))
                    {
                        continue;
                    }

                    for (var vi = 0; vi < masks[level].Length; vi++)
                    {
                        for (var ui = 0; ui < masks[level][0].Length; ui++)
                        {
                            if (!masks[level][vi][ui]) {
                                continue;
                            }

                            var cursorX = getCursorPositionX(xi, ui);
                            var cursorY = getCursorPositionY(yi, vi);
                            if (!source.IsInBounds(cursorX, cursorY)
                                || source.GetPixel(cursorX, cursorY).NotEquals(backGroundColor))
                            {
                                continue;
                            }

                            ret.pixels[ret.GetIndex(cursorX, cursorY) + 0] = drawingColor.B;
                            ret.pixels[ret.GetIndex(cursorX, cursorY) + 1] = drawingColor.G;
                            ret.pixels[ret.GetIndex(cursorX, cursorY) + 2] = drawingColor.R;
                            ret.pixels[ret.GetIndex(cursorX, cursorY) + 3] = 0xFF; // ignore alpha channel for this filter
                        }
                    }
                }
            }
            return ret;
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        public void SetDrawingColor(string drawingColor)
        {
            this.drawingColor = new Pixel(drawingColor);

        }

        public void SetBackGroundColor(string backGroundColor)
        {
            this.backGroundColor = new Pixel(backGroundColor);
        }

        private int getCursorPositionX(int xi, int ui)
        {
            return xi + ui - masks[level][0].Length / 2;
        }

        private int getCursorPositionY(int yi, int vi)
        {
            return yi + vi - masks[level].Length / 2;
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
                if (drawingPixel.Equals(pixelFrom))
                {
                    ret.pixels[i + 0] = pixelTo.B;
                    ret.pixels[i + 1] = pixelTo.G;
                    ret.pixels[i + 2] = pixelTo.R;
                    ret.pixels[i + 3] = drawingPixel.A; // keep the alpha value as-is
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

    public class ColorCleaner : ImageProcesser
    {
        private Pixel allowedColor;
        private Pixel resultColor;

        public override PixelsData Process(PixelsData source)
        {
            var ret = source.Copy();
            for (var i = 0; i < ret.pixels.Length; i += 4)
            {
                var drawingPixel = new Pixel(source.pixels[i + 0],
                                             source.pixels[i + 1],
                                             source.pixels[i + 2],
                                             source.pixels[i + 3]);
                if (!drawingPixel.Equals(allowedColor))
                {
                    ret.pixels[i + 0] = resultColor.B;
                    ret.pixels[i + 1] = resultColor.G;
                    ret.pixels[i + 2] = resultColor.R;
                    ret.pixels[i + 3] = drawingPixel.A; // keep the alpha value as-is
                }
            }
            return ret;
        }

        public void SetAllowedColor(string allowedColor)
        {
            this.allowedColor = new Pixel(allowedColor);
        }

        public void SetResultColor(string resultColor)
        {
            this.resultColor = new Pixel(resultColor);
        }
    }
}
