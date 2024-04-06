using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
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
            else if (args[0].EndsWith(".cpng"))
            {
                ImageCharColored[,] img = LoadConsolePng(args[0]);
                Console.WriteLine("> Printing saved image");
                int w = img.GetLength(0);
                int h = img.GetLength(1);
                Console.SetWindowSize(w + 1, h + 1);
                Print(img, true, false);
                Console.ReadLine();
                return;
            }
            else if (args[0].EndsWith(".gif"))
            {
                ConvertGif(args[0]);
                Console.ReadLine();
                return;
            }
            else if (Directory.Exists(args[0]))
            {
                PlayFolder(args[0]);
                Console.ReadLine();
                return;
            }
            else
                imgName = args[0];
        }


        Console.WriteLine($"> Loading Map Img");
        CreateCharMap("map.png");

        Console.WriteLine($"> Loading Src Img \"{imgName}\"");
        Image<Rgba32> srcImg;
        try
        {
            srcImg = Image.Load<Rgba32>(imgName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"> ERROR: Program could not find the source image! (\"{imgName}\")");
            Console.WriteLine($"  Please make sure that the source image is in the same location as your executable!");
            Console.WriteLine($"  Error: {e.Message}");
            Console.ReadLine();
            Environment.Exit(1);
            return;
        }
        Console.WriteLine($"> Size: {srcImg.Bounds.Width}x{srcImg.Bounds.Height}");

        Console.WriteLine("Press Enter to convert.");
        Console.ReadLine();

        Console.WriteLine($"> Converting Src Img to MAX {Console.WindowWidth - 1}x{Console.WindowHeight - 1}");
        ImageCharColored[,] converted = ConvertImage(srcImg.Frames[0], Console.WindowWidth - 2, Console.WindowHeight - 2, false);

        Console.WriteLine($"> Printing (no col)");
        Print(converted, false, true);

        Console.WriteLine($"> Printing");
        Print(converted, true, true);

        Save(converted, true, "out.txt");
        Save(converted, false, "out_nocol.txt");
        SaveAsConsolePng(converted, "out.cpng");

        Console.ReadLine();
    }

    public static void PlayFolder(string folderName)
    {
        Console.WriteLine($"> Playing Folder {folderName}");

        Console.WriteLine($"> Getting Files.");
        string[] files = Directory.GetFiles(folderName);

        List<string> sorted = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".cpng"))
                sorted.Add(files[i]);
        }

        if (sorted.Count == 0)
            return;

        // Sort by frame number
        Console.WriteLine("> Sorting Files");
        sorted.Sort((string a, string b) =>
        {
            string a1 = Path.GetFileNameWithoutExtension(a);
            string b1 = Path.GetFileNameWithoutExtension(b);
            int a2 = int.Parse(a1.Substring(5));
            int b2 = int.Parse(b1.Substring(5));
            return a2 - b2;
        });

        Console.WriteLine("> Loading Images");
        List<ImageCharColored[,]> imgs = new List<ImageCharColored[,]>();
        for (int i = 0; i < sorted.Count; i++)
        {
            imgs.Add(LoadConsolePng(sorted[i]));
        }

        Console.WriteLine("> Press Enter to play...");
        Console.ReadLine();

        int w = imgs[0].GetLength(0);
        int h = imgs[0].GetLength(1);
        Console.SetWindowSize(w + 2, h + 2);

        Console.Clear();
        for (int i = 0; i < imgs.Count; i++)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            Print(imgs[i], true, false);
        }
    }

    public static void ConvertGif(string imgName)
    {
        string folderName = Path.GetFileNameWithoutExtension(imgName);

        Console.WriteLine($"> Output Folder: \"{folderName}\"");
        if (Directory.Exists(folderName))
            Directory.Delete(folderName, true);
        Directory.CreateDirectory(folderName);


        Console.WriteLine($"> Loading Map Img");
        CreateCharMap("map.png");

        Console.WriteLine($"> Loading Src Img \"{imgName}\"");
        Image<Rgba32> srcImg;
        try
        {
            srcImg = Image.Load<Rgba32>(imgName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"> ERROR: Program could not find the source image! (\"{imgName}\")");
            Console.WriteLine($"  Please make sure that the source image is in the same location as your executable!");
            Console.WriteLine($"  Error: {e.Message}");
            Console.ReadLine();
            Environment.Exit(1);
            return;
        }
        Console.WriteLine($"> Size: {srcImg.Bounds.Width}x{srcImg.Bounds.Height}");

        Console.WriteLine("Press Enter to convert.");
        Console.ReadLine();

        Console.WriteLine($"> Converting Src Img to MAX {Console.WindowWidth - 1}x{Console.WindowHeight - 1}");
        ConvertImage(srcImg.Frames[0], Console.WindowWidth - 1, Console.WindowHeight - 1, false);

        Console.WriteLine();
        Console.CursorVisible = false;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string threadStr = "\r> Starting Threads: ";
        string conversionStr = "\r> Waiting for Threads to finish: ";
        Console.Write(threadStr);

        List<Thread> threads = new();
        int fCount = srcImg.Frames.Count;
        int fStep = fCount / 20;
        int tCount = Math.Max(fCount / fStep, 1);

        object lockObj = new();
        ulong framesDone = 0;
        bool printFrameStatus = false;
        
        for (int frameI = 0, tI = 0; frameI < fCount; frameI += fStep, tI++)
        {
            Console.Write(threadStr);
            int tDone = threads.Count(t => !t.IsAlive);
            Console.Write($"Thread: {tI}/{tCount} (Threads Started: {Math.Round((tI / (double)tCount) *10000)/100} %) (Frames Done: {Math.Round((tDone / (double)tCount) * 10000) / 100} %)                 ");

            int frameS = frameI;
            Thread t = new(() =>
            {
                for (int frameI2 = frameS; frameI2 < frameS + fStep && frameI2 < fCount; frameI2++)
                {
                    //Console.WriteLine($"> Converting Frame {frameI1 + 1}/{srcImg.Frames.Count}");
                    ImageFrame<Rgba32> frame = srcImg.Frames[frameI2];

                    ImageCharColored[,] converted = ConvertImage(frame, Console.WindowWidth - 1, Console.WindowHeight - 1, true);

                    SaveAsConsolePng(converted, folderName + $"/frame{frameI2}.cpng");

                    if (printFrameStatus)
                    {
                        int fDone = (int)Interlocked.Increment(ref framesDone);
                        int tDone = threads.Count(t => !t.IsAlive);
                        lock (lockObj)
                        {
                            Console.Write(conversionStr);
                            Console.Write($" {fDone}/{fCount} Frames Done ({Math.Round((fDone / (double)fCount)*10000)/100} %), {tDone}/{tCount} Threads done     ");
                        }
                    }
                }
            });
            threads.Add(t);
            t.Start();
        }
        Console.WriteLine();
        Console.WriteLine($"> Started Threads ({Math.Round(stopwatch.ElapsedMilliseconds / 10.0)/100} s)");

        Console.Write(conversionStr);
        printFrameStatus = true;

        foreach (Thread t in threads)
        {
            t.Join();
        }
        Console.WriteLine();
        Console.WriteLine($"> Converted Frames ({Math.Round(stopwatch.ElapsedMilliseconds / 10.0) / 100} s)");
        stopwatch.Stop();

        Console.CursorVisible = true;

        Console.WriteLine("> Done");
    }
}


