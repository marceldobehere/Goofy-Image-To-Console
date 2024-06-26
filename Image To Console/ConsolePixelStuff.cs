﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_To_Console
{
    public class ConsolePixelStuff
    {
        public struct ImageChar
        {
            public char Chr;
            public bool[,] Pxls;
        }

        public struct ImageCharColored
        {
            public char Chr;
            public Rgba32 Fg, Bg;
        }

        public const string charMap = "▀▁▂▃▄▅▆▇█▉▊▋▌▍▎▏▐░▒▓▔▕▖▗▘▙▚▛▜▝▞▟ ";
        public static ImageChar[] mapArray = new ImageChar[charMap.Length];
        public static int chrWidth, chrHeight;
        public static ImageCharColored black = new ImageCharColored() { Chr = '#', Fg = new Rgba32(0,0,0), Bg = new Rgba32(0, 0, 0) };

        public static void CreateCharMap(string path)
        {
            Image<Rgba32> charmapImg;
            try
            {
                charmapImg = Image.Load<Rgba32>(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"> ERROR: Program could not find the map image! (\"{path}\")");
                Console.WriteLine($"  Please make sure that the map image is in the same location as your executable!");
                Console.WriteLine($"  Error: {e.Message}");
                Console.ReadLine();
                Environment.Exit(1);
                return;
            }
            Console.WriteLine($"> Size: {charmapImg.Bounds.Width}x{charmapImg.Bounds.Height}");

            chrWidth = charmapImg.Bounds.Width;
            chrHeight = charmapImg.Bounds.Height / charMap.Length;

            Console.WriteLine($"> Creating Base Char Map");
            for (int i = 0; i < charMap.Length; i++)
            {
                mapArray[i] = new ImageChar();
                mapArray[i].Chr = charMap[i];
                mapArray[i].Pxls = new bool[chrWidth, chrHeight];

                for (int x = 0; x < chrWidth; x++)
                    for (int y = 0; y < chrHeight; y++)
                        mapArray[i].Pxls[x, y] = charmapImg[x, y + i * chrHeight].R > 127;

            }
        }
    }
}
