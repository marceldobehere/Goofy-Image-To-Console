using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Image_To_Console.ConsolePixelStuff;
using static Image_To_Console.FileImgUtils;
using static Image_To_Console.ImgConversion;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();

        string imgName = "conv.png";
        if (args.Length == 1)
        {
            if (args[0].EndsWith(".txt"))
            {
                Console.WriteLine("> Printing saved image");
                PrintFile(args[0]);
                Console.ReadLine();
                return;
            }
            else
                imgName = args[0];
        }


        Console.WriteLine($"> Loading Map Img");
        CreateCharMap("map.png");

        Console.WriteLine($"> Loading Src Img");
        Image<Rgba32> srcImg = Image.Load<Rgba32>(imgName);
        Console.WriteLine($"> Size: {srcImg.Bounds.Width}x{srcImg.Bounds.Height}");

        Console.WriteLine("Press Enter to convert.");
        Console.ReadLine();

        Console.WriteLine($"> Converting Src Img to MAX {Console.WindowWidth}x{Console.WindowHeight - 1}");
        ImageCharColored[,] converted = ConvertImage(srcImg, Console.WindowWidth, Console.WindowHeight - 1);

        Console.WriteLine($"> Printing (no col)");
        Print(converted, false);

        Console.WriteLine($"> Printing");
        Print(converted, true);

        Save(converted, true, "out.txt");
        Save(converted, false, "out_nocol.txt");

        Console.ReadLine();
    }
}


