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
        string givenInputPath = ""; // first arg
        string givenOutputPath = ""; // -o PATH
        string givenSize = ""; // -size 100x100
        string givenFPS = ""; // -fps 30
        bool silent = false;

        if (args.Length > 0)
            givenInputPath = args[0];
        
        int start = 1;
        if (args.Length > 0 && args[0].StartsWith("-"))
            start = 0;
        
        for (int i = start; i < args.Length; i++)
        {
            if (args[i] == "-o" || args[i] == "--o")
            {
                if (i + 1 < args.Length)
                    givenOutputPath = args[++i];
                else
                {
                    Console.WriteLine("> ERROR: No output path given after -o");
                    Console.ReadLine();
                    return;
                }
            }
            else if (args[i] == "-size" || args[i] == "--size")
            {
                if (i + 1 < args.Length)
                    givenSize = args[++i];
                else
                {
                    Console.WriteLine("> ERROR: No size given after -size");
                    Console.ReadLine();
                    return;
                }
            }
            else if (args[i] == "-fps" || args[i] == "--fps")
            {
                if (i + 1 < args.Length)
                    givenFPS = args[++i];
                else
                {
                    Console.WriteLine("> ERROR: No fps given after -fps");
                    Console.ReadLine();
                    return;
                }
            }
            else if (args[i] == "-s" || args[i] == "--silent")
            {
                silent = true;
            }
            else if (args[i] == "-help" || args[i] == "--help")
            {
                Console.WriteLine("> Image To Console");
                Console.WriteLine("  Converts images to console output / or views them");
                Console.WriteLine("  ");
                Console.WriteLine("> Arguments:");
                Console.WriteLine("    [input] - Path to the image to convert/view.");
                Console.WriteLine("    -o, --o [output] - Path to save the output to.");
                Console.WriteLine("    -size, --size [width]x[height] - Size of the output.");
                Console.WriteLine("    -fps, --fps [fps] - FPS of the folder video when displayed.");
                Console.WriteLine("    -s, --silent - Silent mode.");
                Console.WriteLine("    -help, --help - Show this help message.");
                Console.WriteLine("  ");
                Console.WriteLine("> Input Formats (Convert): ");
                Console.WriteLine("    .png, .jpg, .jpeg, .gif");
                Console.WriteLine("  ");
                Console.WriteLine("> Input Formats (View): ");
                Console.WriteLine("    .txt, .cpng, [folder with .cpng files]");
                Console.WriteLine("  ");
                Console.WriteLine("> Output Formats: ");
                Console.WriteLine("    .txt, .cpng, [folder for gifs]");
                Console.WriteLine("  ");
                Console.WriteLine("> Examples:");
                Console.WriteLine("    \"Image To Console.exe\" image.png -o out.cpng -size 100x100");
                Console.WriteLine("    \"Image To Console.exe\".exe image.png -o out.txt");
                Console.WriteLine("    \"Image To Console.exe\".exe image.png -o out.cpng");
                Console.WriteLine("    \"Image To Console.exe\".exe image.gif");
                Console.WriteLine("    \"Image To Console.exe\".exe image.gif -o outFolder");
                Console.WriteLine("    \"Image To Console.exe\".exe folder -fps 20");
                Console.WriteLine("    \"Image To Console.exe\".exe out.cpng");
                Console.WriteLine(" ");
                Console.WriteLine("> Press Enter to exit.");
                Console.ReadLine();
                return;
            }
            else
            {
                Console.WriteLine($"> ERROR: Unknown argument \"{args[i]}\"");
                Console.ReadLine();
                return;
            }
        }

        if (givenInputPath == "")
            givenInputPath = "conv.png";
        if (givenOutputPath == "")
        {
            if (givenInputPath.EndsWith(".gif"))
                givenOutputPath = "out";
            else
                givenOutputPath = "out.cpng";
        }
        if (givenSize == "")
            givenSize = $"{Console.WindowWidth - 2}x{Console.WindowHeight - 2}";
        if (givenFPS == "")
            givenFPS = "30";

        int sizeX, sizeY;
        if (!int.TryParse(givenSize.Split('x')[0], out sizeX))
        {
            Console.WriteLine($"> ERROR: Could not parse size X \"{givenSize.Split('x')[0]}\"");
            Console.ReadLine();
            return;
        }
        if (!int.TryParse(givenSize.Split('x')[1], out sizeY))
        {
            Console.WriteLine($"> ERROR: Could not parse size Y \"{givenSize.Split('x')[1]}\"");
            Console.ReadLine();
            return;
        }

        int fps;
        if (!int.TryParse(givenFPS, out fps))
        {
            Console.WriteLine($"> ERROR: Could not parse fps \"{givenFPS}\"");
            Console.ReadLine();
            return;
        }

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();

        Console.WriteLine($"> Input Path: \"{givenInputPath}\"");
        Console.WriteLine($"> Output Path: \"{givenOutputPath}\"");
        Console.WriteLine($"> Size: \"{sizeX}x{sizeY}\"");
        Console.WriteLine($"> FPS: \"{fps}\"");
        Console.WriteLine($"> Silent: {silent}");
        Console.WriteLine();
        Console.ReadLine();


        if (givenInputPath.EndsWith(".txt"))
        {
            Console.WriteLine("> Printing saved image");
            PrintFile(givenInputPath);
            if (!silent)
                Console.ReadLine();
            return;
        }
        else if (givenInputPath.EndsWith(".cpng"))
        {
            ImageCharColored[,] img = LoadConsolePng(givenInputPath);
            Console.WriteLine("> Printing saved image");
            int w = img.GetLength(0);
            int h = img.GetLength(1);
            Console.SetWindowSize(w + 1, h + 1);
            Print(img, true, false);

            if (!silent)
                Console.ReadLine();
            return;
        }
        else if (givenInputPath.EndsWith(".gif"))
        {
            ConvertGif(givenInputPath, givenOutputPath, sizeX, sizeY, silent);
            if (!silent)
                Console.ReadLine();
            return;
        }
        else if (Directory.Exists(givenInputPath))
        {
            PlayFolder(givenInputPath, fps);

            if (!silent)
                Console.ReadLine();
            return;
        }
        else
        {
            Console.WriteLine($"> Loading Map Img");
            CreateCharMap("map.png");

            Console.WriteLine($"> Loading Src Img \"{givenInputPath}\"");
            Image<Rgba32> srcImg;
            try
            {
                srcImg = Image.Load<Rgba32>(givenInputPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"> ERROR: Program could not find the source image! (\"{givenInputPath}\")");
                Console.WriteLine($"  Please make sure that the source image is in the same location as your executable!");
                Console.WriteLine($"  Error: {e.Message}");
                Console.ReadLine();
                Environment.Exit(1);
                return;
            }
            Console.WriteLine($"> Size: {srcImg.Bounds.Width}x{srcImg.Bounds.Height}");


            if (!silent)
            {
                Console.WriteLine("Press Enter to convert.");
                Console.ReadLine();
            }

            Console.WriteLine($"> Converting Src Img to MAX {sizeX}x{sizeY}");
            ImageCharColored[,] converted = ConvertImage(srcImg.Frames[0], sizeX, sizeY, silent);

            if (!silent)
            {
                Console.WriteLine($"> Printing (no col)");
                Print(converted, false, true);

                Console.WriteLine($"> Printing");
                Print(converted, true, true);
            }

            if (givenOutputPath.EndsWith(".txt"))
                Save(converted, true, givenOutputPath);
            else if (givenOutputPath.EndsWith(".cpng"))
                SaveAsConsolePng(converted, givenOutputPath);
            else
            {
                Console.WriteLine($"> ERROR: Unknown output format \"{givenOutputPath}\"");
                Console.ReadLine();
                return;
            }

            if (!silent)
                Console.ReadLine();
        }
    }

    public static void PlayFolder(string folderName, int fps)
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
        {
            Console.WriteLine($"> ERROR: No .cpng files found in folder \"{folderName}\"");
            return;
        }

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
        Console.SetWindowSize(w + 4, h + 4);

        Console.Clear();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < imgs.Count; i++)
        {
            //Console.SetCursorPosition(0, 0);
            //Console.Write($"\r\x1b[{}A");
            Console.Write($"\r\x1b[0;0H");
            Console.CursorVisible = false;
            Print(imgs[i], true, false);

            int fpsTime = (i * 1000 / fps);
            while (stopwatch.ElapsedMilliseconds < fpsTime)
                Thread.Sleep(1);
        }
    }

    public static void ConvertGif(string imgName, string folderName, int sizeX, int sizeY, bool silent)
    {
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
        // Get FPS from GIF
        int fps = 100/srcImg.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay;
        Console.WriteLine($"> FPS: {fps}");

        if (!silent)
        {
            Console.WriteLine("Press Enter to convert.");
            Console.ReadLine();
        }

        Console.WriteLine($"> Converting Src Img to MAX {sizeX}x{sizeY}");
        if (!silent)
            ConvertImage(srcImg.Frames[0], sizeX, sizeY, false);

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
            Console.Write($"Thread: {tI}/{tCount} (Threads Started: {Math.Round((tI / (double)tCount) *10000)/100} %)     ");

            int frameS = frameI;
            Thread t = new(() =>
            {
                for (int frameI2 = frameS; frameI2 < frameS + fStep && frameI2 < fCount; frameI2++)
                {
                    //Console.WriteLine($"> Converting Frame {frameI1 + 1}/{srcImg.Frames.Count}");
                    ImageFrame<Rgba32> frame = srcImg.Frames[frameI2];

                    ImageCharColored[,] converted = ConvertImage(frame, sizeX, sizeY, true);

                    SaveAsConsolePng(converted, folderName + $"/frame{frameI2}.cpng");

                    if (printFrameStatus)
                    {
                        lock (lockObj)
                        {
                            int fDone = (int)Interlocked.Increment(ref framesDone);
                            int tDone = threads.Count(t => !t.IsAlive);
                            Console.Write(conversionStr);
                            Console.Write($" {fDone}/{fCount} Frames Done ({Math.Round((fDone / (double)fCount)*10000)/100} %), {tDone}/{tCount} Threads done     ");
                        }
                    }
                    else
                        Interlocked.Increment(ref framesDone);
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


