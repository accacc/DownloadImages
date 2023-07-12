using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace DownloadImages
{
    class Program
    {
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        static ImageDownloader imageDownloader = new ImageDownloader("Input.json");


        static async Task Main()
        {
           

            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

            imageDownloader.ProgressUpdated += ProgressUpdated;
            imageDownloader.ProgressStarted += ProgressStarted;
            imageDownloader.ProgressCompleted += ProgressCompleted;
            imageDownloader.ProgressFailed += ProgressFailed;

            await imageDownloader.Start();

            imageDownloader.ProgressUpdated -= ProgressUpdated;
            imageDownloader.ProgressStarted -= ProgressStarted;
            imageDownloader.ProgressCompleted -= ProgressCompleted;
            imageDownloader.ProgressFailed -= ProgressFailed;

        }



        static void ProgressUpdated(int currentCount,int totalCount)
        {
            //Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"\rProgress: {currentCount}/{totalCount}");

        }

        static void ProgressStarted(int totalCount, int ParalelJobCount)
        {
            Console.WriteLine($"\nDownloading {totalCount} images ({ParalelJobCount} parallel downloads at most)\n");

        }

        static void ProgressCompleted()
        {
            Console.WriteLine("\nDownload completed!");
            //ExploreFile(Environment.CurrentDirectory);
        }


        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            imageDownloader.DeleteDownloadedImages();
            Environment.Exit(0);
            return true;
        }

        private static void ProgressFailed(string message)
        {
            Console.WriteLine($"{message}");
            Console.ReadKey();
        }

        private static bool ExploreFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }

            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }
    }
}
