using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Image_To_Console.ConsolePixelStuff;
using static Image_To_Console.FileImgUtils;

namespace Image_To_Console
{
    public class ImgConversion
    {
        public static ImageCharColored[,] ConvertImage(Image<Rgba32> src, int w, int h)
        {
            double yDiv = (src.Height / 2) / (double)h;
            double xDiv = src.Width / (double)w;

            double commonDiv = Math.Max(xDiv, yDiv);

            h = (int)((src.Height / 2) / commonDiv);
            w = (int)((src.Width) / commonDiv);

            ImageCharColored[,] res = new ImageCharColored[w, h];

            Console.WriteLine($"> Actually converting Img to {w}x{h}");

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {

                    int aX1 = (x * src.Width) / w;
                    int aY1 = (y * src.Height) / h;
                    int ax2 = ((x + 1) * src.Width - 1) / w;
                    int ay2 = ((y + 1) * src.Height - 1) / h;

                    int aw = ax2 - aX1 + 1;
                    int ah = ay2 - aY1 + 1;

                    Rgba32[,] sub = new Rgba32[aw, ah];
                    for (int subX = 0; subX < aw; subX++)
                        for (int subY = 0; subY < ah; subY++)
                            sub[subX, subY] = src[aX1 + subX, aY1 + subY];

                    res[x, y] = FindBestCharForArea(sub);
                    PrintPxl(res[x, y]);
                }
                Console.WriteLine("\x1b[0m");
            }
            PadHeight(res.GetLength(1), Console.WindowHeight - 1);

            return res;
        }

        public static (Rgba32 fg, Rgba32 bg) GetColorsFromCharAndArea(ImageChar c, Rgba32[,] area)
        {
            Rgba32 fg;
            Rgba32 bg;

            long fgSumR = 0, fgSumG = 0, fgSumB = 0; int fgCount = 0;
            long bgSumR = 0, bgSumG = 0, bgSumB = 0; int bgCount = 0;

            for (int x = 0; x < chrWidth; x++)
                for (int y = 0; y < chrHeight; y++)
                {
                    long rSum = 0, gSum = 0, bSum = 0;

                    int aX1 = (x * area.GetLength(0)) / chrWidth;
                    int aY1 = (y * area.GetLength(1)) / chrHeight;

                    int aX2 = ((x + 1) * area.GetLength(0) - 1) / chrWidth;
                    int aY2 = ((y + 1) * area.GetLength(1) - 1) / chrHeight;

                    for (int subX = aX1; subX <= aX2; subX++)
                        for (int subY = aY1; subY <= aY2; subY++)
                        {
                            Rgba32 pxlColor = area[subX, subY];
                            rSum += pxlColor.R;
                            gSum += pxlColor.G;
                            bSum += pxlColor.B;
                        }

                    rSum /= (aX2 - aX1 + 1) * (aY2 - aY1 + 1);
                    gSum /= (aX2 - aX1 + 1) * (aY2 - aY1 + 1);
                    bSum /= (aX2 - aX1 + 1) * (aY2 - aY1 + 1);

                    Rgba32 avg = new Rgba32((byte)rSum, (byte)gSum, (byte)bSum, 0);

                    if (c.Pxls[x, y])
                    {
                        fgSumR += avg.R;
                        fgSumG += avg.G;
                        fgSumB += avg.B;
                        fgCount++;
                    }
                    else
                    {
                        bgSumR += avg.R;
                        bgSumG += avg.G;
                        bgSumB += avg.B;
                        bgCount++;
                    }
                }

            if (fgCount == 0)
                fgCount = 1;
            if (bgCount == 0)
                bgCount = 1;

            fgSumR /= fgCount;
            fgSumG /= fgCount;
            fgSumB /= fgCount;
            fg = new Rgba32((byte)fgSumR, (byte)fgSumG, (byte)fgSumB, 0);

            bgSumR /= bgCount;
            bgSumG /= bgCount;
            bgSumB /= bgCount;
            bg = new Rgba32((byte)bgSumR, (byte)bgSumG, (byte)bgSumB, 0);

            return (fg, bg);
        }

        public static long ScoreDiff(ImageChar c, Rgba32 fg, Rgba32 bg, Rgba32[,] area)
        {
            long diff = 0;
            for (int x = 0; x < chrWidth; x++)
                for (int y = 0; y < chrHeight; y++)
                {
                    bool pxl = c.Pxls[x, y];
                    long rSum = 0, gSum = 0, bSum = 0;

                    int aX1 = (x * area.GetLength(0)) / chrWidth;
                    int aY1 = (y * area.GetLength(1)) / chrHeight;

                    int aX2 = ((x + 1) * area.GetLength(0) - 1) / chrWidth;
                    int aY2 = ((y + 1) * area.GetLength(1) - 1) / chrHeight;

                    for (int subX = aX1; subX <= aX2; subX++)
                        for (int subY = aY1; subY <= aY2; subY++)
                        {
                            Rgba32 pxlColor = area[subX, subY];
                            rSum += pxlColor.R;
                            gSum += pxlColor.G;
                            bSum += pxlColor.B;
                        }

                    rSum /= (aX2 - aX1 + 1) * (aY2 - aY1 + 1);
                    gSum /= (aX2 - aX1 + 1) * (aY2 - aY1 + 1);
                    bSum /= (aX2 - aX1 + 1) * (aY2 - aY1 + 1);

                    Rgba32 avg = new Rgba32((byte)rSum, (byte)gSum, (byte)bSum, 0);
                    diff += Math.Abs(avg.R - (pxl ? fg.R : bg.R));
                    diff += Math.Abs(avg.G - (pxl ? fg.G : bg.G));
                    diff += Math.Abs(avg.B - (pxl ? fg.B : bg.B));
                }

            return diff;
        }

        public static ImageCharColored FindBestCharForArea(Rgba32[,] area)
        {
            ImageCharColored best = new ImageCharColored();
            best.Chr = '?';
            best.Fg = new Rgba32(255, 255, 255);
            best.Bg = new Rgba32(0, 0, 0);

            long bestScore = long.MaxValue;
            for (int i = 0; i < mapArray.Length; i++)
            {
                ImageChar c = mapArray[i];
                (Rgba32 fg, Rgba32 bg) = GetColorsFromCharAndArea(c, area);

                long diff = ScoreDiff(c, fg, bg, area);
                if (diff < bestScore)
                {
                    bestScore = diff;
                    best.Chr = c.Chr;
                    best.Fg = fg;
                    best.Bg = bg;
                }
            }

            return best;
        }
    }
}
