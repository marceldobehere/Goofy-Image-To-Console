using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Image_To_Console.ConsolePixelStuff;

namespace Image_To_Console
{
    public class FileImgUtils
    {
        public static void PadHeight(int imgH, int aH)
        {
            for (int y = imgH; y < aH; y++)
                Console.WriteLine();
        }

        public static void PrintFile(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string[] size = sr.ReadLine().Split('x');
                int w = int.Parse(size[0]);
                int h = int.Parse(size[1]);
                Console.SetWindowSize(w + 1, h + 1);
                while (!sr.EndOfStream)
                    Console.WriteLine(sr.ReadLine());
            }
        }

        public static string ColorToStr(Rgba32 color)
        {
            return $"{color.R};{color.G};{color.B}";
        }

        public static string PxlToStr(ImageCharColored pxl)
        {
            string str = "";
            str += $"\u001b[38;2;{ColorToStr(pxl.Fg)}m";
            str += $"\u001b[48;2;{ColorToStr(pxl.Bg)}m";
            str += pxl.Chr;

            return str;
        }

        public static void PrintPxl(ImageCharColored pxl)
        {
            string str = "";
            str += $"\u001b[38;2;{ColorToStr(pxl.Fg)}m";
            str += $"\u001b[48;2;{ColorToStr(pxl.Bg)}m";
            str += pxl.Chr;

            Console.Write(str);
        }

        public static void Print(ImageCharColored[,] img, bool col)
        {
            for (int y = 0; y < img.GetLength(1); y++)
            {
                for (int x = 0; x < img.GetLength(0); x++)
                {
                    ImageCharColored pxl = img[x, y];
                    if (col)
                        PrintPxl(pxl);
                    else
                        Console.Write(pxl.Chr);
                }
                if (col)
                    PrintPxl(black);
                Console.WriteLine("\x1b[0m");
            }
            Console.WriteLine("\x1b[0m");

            PadHeight(img.GetLength(1), Console.WindowHeight - 1);
        }

        public static void Save(ImageCharColored[,] img, bool col, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine($"{img.GetLength(0)}x{img.GetLength(1)}");
                for (int y = 0; y < img.GetLength(1); y++)
                {
                    for (int x = 0; x < img.GetLength(0); x++)
                    {
                        ImageCharColored pxl = img[x, y];
                        if (col)
                            sw.Write(PxlToStr(pxl));
                        else
                            sw.Write(pxl.Chr);
                    }
                    if (col)
                        sw.Write(PxlToStr(black));
                    if (col)
                        sw.WriteLine("\x1b[0m");
                    else
                        sw.WriteLine();
                }
            }
        }
    }
}
