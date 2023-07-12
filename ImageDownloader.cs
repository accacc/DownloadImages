using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace DownloadImages
{
    public  class ImageDownloader
    {

        private  int totalCount;
        private  int ParalelJobCount;
        public  string savePath;
        private  int downloadedCount;
        private  List<string> downloadedImages;
        private HttpClient client = new HttpClient();

        public delegate void ProgressUpdatedEventHandler(int currentCount,int totalCount);
        public event ProgressUpdatedEventHandler ProgressUpdated;

        public delegate void ProgressStartedEventHandler(int totalCount, int paralelJobCount);
        public event ProgressStartedEventHandler ProgressStarted;

        public delegate void ProgressCompletedEventHandler();
        public event ProgressCompletedEventHandler ProgressCompleted;

        public delegate void ProgressFailedEventHandler(string message);
        public event ProgressFailedEventHandler ProgressFailed;

        private void SetInitials(DownloadImageJobRequest request)
        {
            totalCount = request.Count;
            ParalelJobCount = request.ParalelJobCount;
            savePath = string.IsNullOrEmpty(request.SavePath) ? "./outputs" : request.SavePath;
        }

        public ImageDownloader(string jobFilePath)
        {
            string inputJson = File.ReadAllText(jobFilePath);
            var request = JsonConvert.DeserializeObject<DownloadImageJobRequest>(inputJson);

            totalCount = request.Count;
            ParalelJobCount = request.ParalelJobCount;
            savePath = string.IsNullOrEmpty(request.SavePath) ? "./outputs" : request.SavePath;

        }

        public ImageDownloader(DownloadImageJobRequest request)
        {
            SetInitials(request);
        }

        public async Task Start()
        {

            try
            {
                if (!Directory.Exists(savePath)) { Directory.CreateDirectory("./outputs"); }

                downloadedCount = 0;
                downloadedImages = new List<string>();

                var tasks = new List<Task>();

                ProgressStarted?.Invoke(totalCount, ParalelJobCount);                

                var indices = Enumerable.Range(0, totalCount);

                await indices.ForEachAsync(f => DownloadImageAsync(f), ParalelJobCount);

                ProgressCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                ProgressFailed?.Invoke(ex.GetBaseException().Message);
                //Log
            }

        }


        private async Task DownloadFileTaskAsync(string url, string localPath = null, int timeOut = 3000)
        {

            if (url == null)
            {
                throw new ArgumentNullException("remotePath");
            }

            if (localPath == null)
            {
                localPath = Path.GetTempFileName();
            }

            using (var client = new WebClient())
            {
                TimerCallback timerCallback = c =>
                {
                    var webClient = (WebClient)c;
                    if (!webClient.IsBusy) return;
                    webClient.CancelAsync();

                };

                using (var timer = new Timer(timerCallback, client, timeOut, Timeout.Infinite))
                {
                    await client.DownloadFileTaskAsync(url, localPath);
                }
            }
        }


        private async Task DownloadImageAsync(int index, CancellationToken cancellationToken = default)
        {
            try
            {

                var url = $"https://picsum.photos/200/300?random={Guid.NewGuid()}";

                var filePath = Path.Combine(savePath, $"{index}.png");

                await DownloadFileTaskAsync(url, filePath);                

                //await DownloadFileHttpClientAsync(url, filePath, cancellationToken);

                Interlocked.Increment(ref downloadedCount);

                downloadedImages.Add(filePath);

                ProgressUpdated?.Invoke(downloadedCount, totalCount);
            }
            catch (OperationCanceledException ex)
            {
                ProgressFailed?.Invoke(ex.GetBaseException().Message);
                //Log
            }
        }

        private async Task DownloadFileHttpClientAsync(string url, string filePath, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(url, cancellationToken);

            var contentStream = await response.Content.ReadAsStreamAsync();

            using (var fileStream = File.Create(filePath))
            {
                await contentStream.CopyToAsync(fileStream);
            }
        }

        public void DeleteDownloadedImages()
        {
            foreach (var filePath in downloadedImages)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }


    }
}
