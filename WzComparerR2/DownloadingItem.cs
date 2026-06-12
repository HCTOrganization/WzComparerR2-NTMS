using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;

namespace WzComparerR2
{
    public class DownloadingItem
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(15000)
        };

        static DownloadingItem()
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WzComparerR2-NTMS/1.0");
        }

        public DownloadingItem(string url, string path)
        {
            this.url = url;
            this.path = path;
        }

        string url;
        string path;
        DateTime lastModified;
        long fileLength;
        Thread thread;
        HttpResponseMessage response;
        Stream responseStream;
        CancellationTokenSource cancellationTokenSource;

        public string Url
        {
            get { return url; }
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public DateTime LastModified
        {
            get { return lastModified; }
        }

        public long FileLength
        {
            get { return fileLength; }
        }

        public void GetFileLength()
        {
            var uri = new Uri(this.url);
            switch (uri.Scheme.ToLower())
            {
                case "http":
                case "https":
                    GetFileLengthHttp();
                    break;

                case "ftp":
                    GetFileLengthFtp();
                    break;
            }
        }

        private void GetFileLengthHttp()
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    var resp = httpClient.SendAsync(request).GetAwaiter().GetResult();
                    resp.EnsureSuccessStatusCode();
                    
                    if (resp.Content.Headers.LastModified.HasValue)
                    {
                        this.lastModified = resp.Content.Headers.LastModified.Value.DateTime;
                    }
                    this.fileLength = resp.Content.Headers.ContentLength ?? 0;
                    resp.Dispose();
                }
            }
            catch (Exception)
            {
                this.fileLength = 0;
                throw;
            }
        }

        private void GetFileLengthFtp()
        {
            try
            {
                var req = WebRequest.Create(url) as FtpWebRequest;
                req.Method = WebRequestMethods.Ftp.GetFileSize;
                req.Timeout = 15000;
                using (var resp = req.GetResponse() as FtpWebResponse)
                {
                    this.fileLength = resp.ContentLength;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {
                var req = WebRequest.Create(url) as FtpWebRequest;
                req.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                req.Timeout = 15000;
                using (var resp = req.GetResponse() as FtpWebResponse)
                {
                    this.lastModified = resp.LastModified;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void StartDownload()
        {
            if (thread == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
                thread = new Thread(tryStartDownload);
                thread.Start();
            }
        }

        private void tryStartDownload()
        {
            try
            {
                var uri = new Uri(url);
                if (uri.Scheme.ToLower() == "http" || uri.Scheme.ToLower() == "https")
                {
                    response = httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    responseStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                }
                else
                {
                    // Fallback to WebRequest for non-HTTP protocols (e.g., FTP)
                    WebRequest request = WebRequest.Create(url);
                    request.Timeout = 15000;
                    var webResponse = request.GetResponse();
                    responseStream = webResponse.GetResponseStream();
                    webResponse.Close();
                }
            }
            catch (Exception) { }
            finally { }
        }

        public void StopDownload()
        {
            if (response != null || responseStream != null)
            {
                try
                {
                    cancellationTokenSource?.Cancel();
                    responseStream?.Close();
                    responseStream?.Dispose();
                    response?.Dispose();
                    response = null;
                    responseStream = null;
                    
                    // Note: Thread.Abort is deprecated and dangerous. Using CancellationToken instead.
                    // The thread should complete naturally when the stream is closed.
                    thread = null;
                }
                catch (Exception) { }
                finally { }
            }
        }
    }
}
