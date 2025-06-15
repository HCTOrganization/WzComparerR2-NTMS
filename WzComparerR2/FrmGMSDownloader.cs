using DevComponents.AdvTree;
using DevComponents.DotNetBar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WzComparerR2.Common;
using WzComparerR2.Config;

namespace WzComparerR2
{
    public partial class FrmGMSDownloader : DevComponents.DotNetBar.Office2007Form
    {
        public FrmGMSDownloader()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(FrmGMSDownloader_FormClosing);
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif

            var downloaderSession = new DownloaderSession();
            Task.Run(() => this.ExecuteUpdateAsync(downloaderSession, downloaderSession.CancellationToken));
        }


        private DownloaderSession downloaderSession;
        private string manifestUrl;
        private string manifestBaseUrl = "http://download2.nexon.net/Game/nxl/games/10100/";
        private string applyPath = "";

        private async Task ExecuteUpdateAsync(DownloaderSession session, CancellationToken cancellationToken)
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

                this.lblUpdateDate.Text = releaseDate + " UTC";
                this.lblLatestVer.Text = majorVersion + "." + minorVersion + "." + revision;
                this.richTextBoxEx1.SelectionStart = 0;
                buttonX1.Enabled = true;
                this.lblUpdateContent.Text = "可以下載";
            }
            catch (Exception ex)
            {
                this.lblUpdateContent.Text = "不可下載";
            }
        }

        private async Task DownloadClientAsync(string url, DownloaderSession session, CancellationToken cancellationToken)
        {
            void AppendStateText(string text, Color color)
            {
                this.richTextBoxEx1.SelectionStart = this.richTextBoxEx1.TextLength;
                this.richTextBoxEx1.SelectionLength = 0;
                this.richTextBoxEx1.SelectionColor = color;
                this.Invoke(new Action<string>(t => this.richTextBoxEx1.AppendText(text)), text);
                if (session.LoggingFileName != null)
                {
                    File.AppendAllText(session.LoggingFileName, text, Encoding.UTF8);
                }
                this.richTextBoxEx1.SelectionColor = this.richTextBoxEx1.ForeColor;
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
                AppendStateText("嘗試獲取清單哈希...", Color.Black);
                var manifestHashResponse = (HttpWebResponse)manifestHashRequest.GetResponse();
                var manifestHashString = new StreamReader(manifestHashResponse.GetResponseStream()).ReadToEnd();
                using (StreamWriter outputFile = new StreamWriter(manifestPath, false, Encoding.UTF8))
                {
                    outputFile.Write(manifestHashString);
                }
                AppendStateText("完成\r\n", Color.Green);
                var manifestRequest = (HttpWebRequest)WebRequest.Create("http://download2.nexon.net/Game/nxl/games/10100/" + manifestHashString);
                manifestRequest.UserAgent = "GmsDownloader/1.0";
                manifestRequest.Method = "GET";
                try
                {
                    AppendStateText("正在載入清單...\r\n", Color.Black);
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
                            AppendStateText("清單載入完成\r\n", Color.Green);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                AppendStateText(String.Format("總數量: {0}\r\n", manifest.files.Count), Color.Black);
                AppendStateText(String.Format("總大小: {0}\r\n", GetBothByteAndGBValue(manifest.total_uncompressed_size)), Color.Black);
                if (manifest.total_uncompressed_size > RemainingDiskSpace(applyPath))
                {
                    AppendStateText("磁碟空間不足以完成下載。終止中...\r\n", Color.Red);
                    this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOAD_FAIL;
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
                            AppendStateText(String.Format("正在建立資料夾: {0}\r\n", fullFileName), Color.Black);
                            Directory.CreateDirectory(fullFileName);
                        }
                    }
                    else
                    {
                        if (!File.Exists(fullFileName) || new FileInfo(fullFileName).Length != kv.Value.fsize)
                        {
                            AppendStateText(String.Format("正在下載檔案: {0}...", fullFileName), Color.Black);
                            using (var fs = File.Create(fullFileName))
                            {
                                for (int p = 0; p < kv.Value.objects.Length; p++)
                                {
                                    var objID = kv.Value.objects[p];
                                    string objUrl = String.Format("{0}10100/{1}/{2}", manifestBaseUrl, objID.Substring(0, 2), objID);
                                    // AppendStateText(String.Format("part {0}/{1}: {2}\r\n", p + 1, kv.Value.objects.Length, objUrl), Color.Black);
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
                            AppendStateText("完成\r\n", Color.Green);
                        }
                        else
                        {
                            AppendStateText(String.Format("檔案{0}已存在，跳過\r\n", fullFileName), Color.Black);
                        }
                    }
                }
                this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOAD_FAIL;
            }
            catch (Exception ex)
            {
                AppendText(ex.Message + "\r\n", Color.Red);
                this.lblUpdateContent.Text = "下載完成";
            }
            finally
            {
                AppendStateText("完了\r\n", Color.Green);
                buttonX1.Enabled = true;
            }
        }

        private async Task UpdateClientAsync(string sourceClientHash, string url, DownloaderSession session, CancellationToken cancellationToken)
        {
            void AppendStateText(string text, Color color)
            {
                this.richTextBoxEx1.SelectionStart = this.richTextBoxEx1.TextLength;
                this.richTextBoxEx1.SelectionLength = 0;
                this.richTextBoxEx1.SelectionColor = color;
                this.Invoke(new Action<string>(t => this.richTextBoxEx1.AppendText(text)), text);
                if (session.LoggingFileName != null)
                {
                    File.AppendAllText(session.LoggingFileName, text, Encoding.UTF8);
                }
                this.richTextBoxEx1.SelectionColor = this.richTextBoxEx1.ForeColor;
            }

            AppendStateText("Starting update process...\r\n", Color.Black);
        }

        private void FrmGMSDownloader_FormClosing(object sender, FormClosingEventArgs e)
        {
            downloaderSession?.Cancel();
            while (downloaderSession?.UpdateExecTask != null && !downloaderSession.UpdateExecTask.IsCanceled)
            {
                downloaderSession.WaitForContinueAsync().Wait(1000);
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            downloaderSession = new DownloaderSession();
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "請選擇GMS安裝資料夾。";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                applyPath = dlg.SelectedPath;
                downloaderSession.LoggingFileName = Path.Combine(applyPath, $"gmsdownloader_{DateTime.Now:yyyyMMdd_HHmmssfff}.log");
            }
            else return;
            if (File.Exists(Path.Combine(applyPath, "appdata", "MapleStory.exe")) && File.Exists(Path.Combine(applyPath, "patchdata", "10100.manifest.hash")))
            {
                switch (DevComponents.DotNetBar.MessageBoxEx.Show(this, "檢測到完全安裝的GMS客戶端。\r\n是否要嘗試更新?", "確認", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        // go to update section, not implemented
                        using (StreamReader reader = new StreamReader(Path.Combine(applyPath, "patchdata", "10100.manifest.hash"), Encoding.UTF8))
                        {
                            string manifestHash = reader.ReadToEnd().Trim();
                        }
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        return;
                }
            }
            this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOADING;
            buttonX1.Enabled = false;
            this.richTextBoxEx1.Clear();
            Task.Run(() => this.DownloadClientAsync(manifestUrl, downloaderSession, downloaderSession.CancellationToken));
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
                return $"{size:N0} 位元組";
            }
            else
            {
                return $"{size:N0} 位元組 ({targetbytes:0.##} {sizes[order]})";
            }
        }

        private void AppendText(string text, Color color)
        {
            this.richTextBoxEx1.SelectionStart = this.richTextBoxEx1.TextLength;
            this.richTextBoxEx1.SelectionLength = 0;

            this.richTextBoxEx1.SelectionColor = color;
            this.richTextBoxEx1.AppendText(text);
            this.richTextBoxEx1.SelectionColor = this.richTextBoxEx1.ForeColor;
        }

        class DownloaderSession
        {
            public DownloaderSession()
            {
                this.cancellationTokenSource = new CancellationTokenSource();
            }
            public Task UpdateExecTask;

            public CancellationToken CancellationToken => this.cancellationTokenSource.Token;
            private CancellationTokenSource cancellationTokenSource;
            private TaskCompletionSource<bool> tcsWaiting;
            public string LoggingFileName;

            public void Cancel()
            {
                this.cancellationTokenSource.Cancel();
            }

            public async Task WaitForContinueAsync()
            {
                var tcs = new TaskCompletionSource<bool>();
                this.tcsWaiting = tcs;
                this.cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled());
                await tcs.Task;
            }

            public void Continue()
            {
                if (this.tcsWaiting != null)
                {
                    this.tcsWaiting.SetResult(true);
                }
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