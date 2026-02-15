using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WzComparerR2
{
    public class Updater
    {
        private const string checkUpdateURL = "https://api.hikaricalyx.com/WcR2-NTMS/v1/GetLatestUpdate";

        public Updater()
        {
            string appVersion = Program.ApplicationVersion;
            if (TryParseApplicationVersion(appVersion, out var version))
            {
                this.CurrentVersion = version;
                this.CurrentVersionString = $"{version.Build}.{version.Revision}";
            }
            else
            {
                this.CurrentVersion = default;
                this.CurrentVersionString = appVersion;
            }
        }

        public bool LatestReleaseFetched { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public Version CurrentVersion { get; private set; }
        public Version LatestVersion { get; private set; }
        public string CurrentVersionString { get; private set; }
        public string LatestVersionString { get; private set; }

        public HCTAPIResponse Release { get; private set; }

        private readonly TimeSpan defaultRequestTimeout = TimeSpan.FromSeconds(15);

        public async Task QueryUpdateAsync(CancellationToken cancellationToken = default)
        {
            // send request
            using var client = new HttpClient();
            client.Timeout = defaultRequestTimeout;
            using var request = new HttpRequestMessage(HttpMethod.Get, checkUpdateURL);
            request.Headers.Accept.ParseAdd("application/json");
            request.Headers.UserAgent.ParseAdd($"WzComparerR2-NTMS/1.0");
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // parse payload
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var jsonTextReader = new JsonTextReader(new StreamReader(responseStream));
            var serializer = new JsonSerializer();
            var release = serializer.Deserialize<HCTAPIResponse>(jsonTextReader);

            // reset all
            this.LatestReleaseFetched = true;
            this.Release = release;

            // check version
            this.LatestVersionString = $"{release.MajorVersion}.{release.BuildNumber}";
            this.UpdateAvailable = Int64.Parse(release.BuildNumber.Replace("-", "")) > Int64.Parse(BuildInfo.BuildTime.Replace("-", ""));
        }

        public async Task DownloadAssetAsync(string assetUrl, string fileName, OnProgressCallback onProgress = null, CancellationToken cancellationToken = default)
        {
            // send request
            using var client = new HttpClient();
            client.Timeout = defaultRequestTimeout;
            using var request = new HttpRequestMessage(HttpMethod.Get, assetUrl);
            request.Headers.UserAgent.ParseAdd($"WzComparerR2-NTMS/1.0");
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            long fileSize = (long)response.Content.Headers.ContentLength;
            // copy to file
            bool fileCreated = false;
            try
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var fs = File.Create(fileName);
                fileCreated = true;
                byte[] buffer = new byte[16 * 1024];
                long downloadedBytes = 0;
                while (true)
                {
                    int len = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (len <= 0)
                    {
                        break;
                    }
                    fs.Write(buffer, 0, len);
                    downloadedBytes += len;
                    onProgress?.Invoke(downloadedBytes, fileSize);
                }
                await responseStream.CopyToAsync(fs, 16 * 1024, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                if (fileCreated && File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                throw;
            }
        }

        private static bool TryParseApplicationVersion(string appVersion, out Version result)
        {
            return Version.TryParse(appVersion, out result);
        }

        public delegate void OnProgressCallback(long downloadedBytes, long totalBytes);

        public class HCTAPIResponse
        {
            [JsonProperty("MajorVersion")]
            public string MajorVersion { get; set; }
            [JsonProperty("BuildNumber")]
            public string BuildNumber { get; set; }
            [JsonProperty("ChangeTitle")]
            public string ChangeTitle { get; set; }
            [JsonProperty("Changelog")]
            public string Changelog { get; set; }
            [JsonProperty("net48-url")]
            public string Net48Url { get; set; }
            [JsonProperty("net60-url")]
            public string Net60Url { get; set; }
            [JsonProperty("net80-url")]
            public string Net80Url { get; set; }
            [JsonProperty("net100-url")]
            public string Net100Url { get; set; }
        }
    }
}
