using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace WzComparerR2.CLI
{
    internal class CliGMSDownloader
    {
        public string manifestUrl;
        private string manifestBaseUrl = "http://download2.nexon.net/Game/nxl/games/10100/";
        public string applyPath = "";

        public void CheckUpdate()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.hikaricalyx.com/WcR2-JMS/v1/GMSClient/Latest");
            request.Accept = "application/json";
            request.UserAgent = "WzComparerR2/1.0";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                JObject UpdateContent = JObject.Parse(responseString);
                string majorVersion = UpdateContent.SelectToken("majorVersion").ToString();
                string minorVersion = UpdateContent.SelectToken("minorVersion").ToString();
                string revision = UpdateContent.SelectToken("revision").ToString();
                string releaseDate = UpdateContent.SelectToken("releaseDate").ToString();
                manifestUrl = UpdateContent.SelectToken("manifestUrl").ToString();

                Console.WriteLine("Release Date: " + releaseDate + " UTC");
                Console.WriteLine("Version: " + majorVersion + "." + minorVersion + "." + revision);
            }
            catch (Exception)
            {
                Console.WriteLine("Download API unavailable");
            }
        }

        public void DownloadClient(string url)
        {
            void AppendStateText(string text, ConsoleColor color = ConsoleColor.White)
            {
                var currentColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(text);
                File.AppendAllText(Path.Combine(applyPath, $"gmsdownloader_{DateTime.Now:yyyyMMdd_HHmmssfff}.log"), text, Encoding.UTF8);
                Console.ForegroundColor = currentColor;
            }

            GMSManifest manifest = new GMSManifest();
            if (!Directory.Exists(Path.Combine(applyPath, "appdata"))) Directory.CreateDirectory(Path.Combine(applyPath, "appdata"));
            if (!Directory.Exists(Path.Combine(applyPath, "patchdata"))) Directory.CreateDirectory(Path.Combine(applyPath, "patchdata"));

            string manifestPath = Path.Combine(applyPath, "patchdata", "10100.manifest.hash");
            var manifestHashRequest = (HttpWebRequest)WebRequest.Create(url);
            manifestHashRequest.UserAgent = "GmsDownloader/1.0";
            manifestHashRequest.Method = "GET";
            try
            {
                AppendStateText("Trying to receive manifest hash...");
                var manifestHashResponse = (HttpWebResponse)manifestHashRequest.GetResponse();
                var manifestHashString = new StreamReader(manifestHashResponse.GetResponseStream()).ReadToEnd();
                using (StreamWriter outputFile = new StreamWriter(manifestPath, false, Encoding.UTF8))
                {
                    outputFile.Write(manifestHashString);
                }
                AppendStateText("Done\r\n", ConsoleColor.Green);
                var manifestRequest = (HttpWebRequest)WebRequest.Create("http://download2.nexon.net/Game/nxl/games/10100/" + manifestHashString);
                manifestRequest.UserAgent = "GmsDownloader/1.0";
                manifestRequest.Method = "GET";
                try
                {
                    AppendStateText("Downloading manifest...\r\n");
                    using (HttpWebResponse response = (HttpWebResponse)manifestRequest.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        byte[] compressedData = memoryStream.ToArray();

                        using (MemoryStream decompressedStream = new MemoryStream())
                        using (DeflateStream deflateStream = new DeflateStream(new MemoryStream(compressedData, 2, compressedData.Length - 2), CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(decompressedStream);
                            string manifestContent = Encoding.UTF8.GetString(decompressedStream.ToArray());

                            manifest = JsonConvert.DeserializeObject<GMSManifest>(manifestContent);
                            AppendStateText("Manifest loaded successfully.\r\n", ConsoleColor.Green);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                AppendStateText(String.Format("Total files: {0}\r\n", manifest.files.Count));
                AppendStateText(String.Format("Total size: {0}\r\n", GetBothByteAndGBValue(manifest.total_uncompressed_size)));
                if (manifest.total_uncompressed_size > RemainingDiskSpace(applyPath))
                {
                    AppendStateText("Insufficient disk space for the client.\r\n", ConsoleColor.Red);
                    return;
                }
                Encoding fileNameEnc = manifest.filepath_encoding == "utf16" ? Encoding.Unicode : Encoding.UTF8;

                foreach (var kv in manifest.files)
                {
                    string fileName = new StreamReader(new MemoryStream(Convert.FromBase64String(kv.Key))).ReadToEnd();
                    string fullFileName = Path.Combine(applyPath, "appdata", fileName);
                    if (kv.Value.objects[0] == "__DIR__")
                    {
                        if (!Directory.Exists(fullFileName))
                        {
                            AppendStateText(String.Format("Create dir: {0}\r\n", fullFileName));
                            Directory.CreateDirectory(fullFileName);
                        }
                    }
                    else
                    {
                        if (!File.Exists(fullFileName) || new FileInfo(fullFileName).Length != kv.Value.fsize)
                        {
                            AppendStateText(String.Format("Downloading file: {0}...", fullFileName));
                            using (var fs = File.Create(fullFileName))
                            {
                                for (int p = 0; p < kv.Value.objects.Length; p++)
                                {
                                    var objID = kv.Value.objects[p];
                                    string objUrl = String.Format("{0}10100/{1}/{2}", manifestBaseUrl, objID.Substring(0, 2), objID);
                                    // AppendStateText(String.Format("part {0}/{1}: {2}\r\n", p + 1, kv.Value.objects.Length, objUrl));
                                    var objRequest = (HttpWebRequest)WebRequest.Create(objUrl);
                                    objRequest.UserAgent = "GmsDownloader/1.0";
                                    objRequest.Method = "GET";
                                    using (HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse())
                                    using (Stream objResponseStream = objResponse.GetResponseStream())
                                    using (MemoryStream objMemoryStream = new MemoryStream())
                                    {
                                        objResponseStream.CopyTo(objMemoryStream);
                                        byte[] compressedData = objMemoryStream.ToArray();

                                        using (DeflateStream deflateStream = new DeflateStream(new MemoryStream(compressedData, 2, compressedData.Length - 2), CompressionMode.Decompress))
                                        {
                                            deflateStream.CopyTo(fs);
                                        }
                                        fs.Flush();
                                    }
                                }
                            }
                            AppendStateText("Done\r\n", ConsoleColor.Green);
                        }
                        else
                        {
                            AppendStateText(String.Format("File {0} already exists, skipping\r\n", fullFileName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendStateText(ex.Message + "\r\n", ConsoleColor.Red);
            }
            finally
            {
                AppendStateText("Completed\r\n", ConsoleColor.Green);
            }
        }


        private long RemainingDiskSpace(string path)
        {
            string diskDrive = path.Substring(0, 2);
            try
            {
                DriveInfo dinfo = new DriveInfo(diskDrive);
                return dinfo.AvailableFreeSpace;
            }
            catch
            {
                return 0;
            }
        }

        private string GetBothByteAndGBValue(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double targetbytes = size;
            int order = 0;

            while (targetbytes >= 1024 && order < sizes.Length)
            {
                order++;
                targetbytes /= 1024;
            }

            if (size <= 1024)
            {
                return $"{size:N0} bytes";
            }
            else
            {
                return $"{size:N0} bytes ({targetbytes:0.##} {sizes[order]})";
            }
        }

        class GMSManifest
        {
            public double buildtime;
            public string filepath_encoding;
            public Dictionary<string, GMSFileInfo> files;
            public string platform;
            public string product;
            public long total_compressed_size;
            public int total_objects;
            public long total_uncompressed_size;
            public string version;
        }

        class GMSFileInfo
        {
            public long fsize;
            public double mtime;
            public string[] objects;
            public int[] objects_fsize;
        }
    }
}
