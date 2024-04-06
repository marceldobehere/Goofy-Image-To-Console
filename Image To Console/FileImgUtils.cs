using SixLabors.ImageSharp;
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

        public static void Print(ImageCharColored[,] img, bool col, bool pad)
        {
            StringBuilder str = new StringBuilder();
            for (int y = 0; y < img.GetLength(1); y++)
            {
                for (int x = 0; x < img.GetLength(0); x++)
                {
                    ImageCharColored pxl = img[x, y];
                    if (col)
                        str.Append(PxlToStr(pxl));
                    else
                        str.Append(pxl.Chr);
                }
                if (col)
                    str.Append(PxlToStr(black));
                str.AppendLine("\x1b[0m");
            }
            str.AppendLine("\x1b[0m");

            Console.WriteLine(str.ToString());
            if (pad)
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

        public static void SaveAsConsolePng(ImageCharColored[,] img, string path)
        {
            Image<Rgba32> imgPng = new(img.GetLength(0) * 2, img.GetLength(1));
            for (int y = 0; y < img.GetLength(1); y++)
                for (int x = 0; x < img.GetLength(0); x++)
                {
                    ImageCharColored pxl = img[x, y];
                    pxl.Fg.A = (byte)charMap.IndexOf(pxl.Chr);
                    imgPng[x * 2, y] = pxl.Fg;
                    imgPng[x * 2 + 1, y] = pxl.Bg;
                }
            
            imgPng.SaveAsPng(path);
        }

        public static ImageCharColored[,] LoadConsolePng(string path)
        {
            Image<Rgba32> imgPng = Image.Load<Rgba32>(path);
            ImageCharColored[,] res = new ImageCharColored[imgPng.Width / 2, imgPng.Height];
            for (int y = 0; y < imgPng.Height; y++)
                for (int x = 0; x < imgPng.Width; x += 2)
                {
                    res[x / 2, y] = new ImageCharColored()
                    {
                        Fg = imgPng[x, y],
                        Bg = imgPng[x + 1, y],
                        Chr = charMap[imgPng[x, y].A]
                    };
                }

            return res;
        }
    }
}
