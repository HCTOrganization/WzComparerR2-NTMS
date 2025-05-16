using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DevComponents.AdvTree;
using Newtonsoft.Json;
using WzComparerR2.Common;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Security.Policy;
using WzComparerR2.Config;

namespace WzComparerR2
{
    public partial class FrmUpdater : DevComponents.DotNetBar.Office2007Form
    {
        public FrmUpdater()
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif

            // this.lblClrVer.Text = string.Format("{0} ({1})", Environment.Version, Program.GetArchitecture());
            this.lblCurrentVer.Text = Program.WcR2MajorVersion + BuildInfo.BuildTime;
            // this.lblLatestVer.Text = GetFileVersion().ToString();
            var updateSession = new UpdaterSession();
            // this.lblUpdateContent.Text = GetAsmCopyright().ToString();
            Task.Run(() => this.ExecuteUpdateAsync(updateSession, updateSession.CancellationToken));
            // GetPluginInfo();
        }

        public bool EnableAutoUpdate
        {
            get { return chkEnableAutoUpdate.Checked; }
            set { chkEnableAutoUpdate.Checked = value; }
        }

        private UpdaterSession updateSession;
        private string net48url;
        private string net60url;
        private string net80url;

        public static async Task<bool> QueryUpdate()
        {
            var request = (HttpWebRequest)WebRequest.Create(Program.CheckUpdateURL);
            request.Accept = "application/json";
            request.UserAgent = "WzComparerR2/1.0";
            try
            {
                using (WebResponse response = await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string responseString = reader.ReadToEnd();
                    JObject UpdateContent = JObject.Parse(responseString);
                    string BuildNumber = UpdateContent.SelectToken("BuildNumber").ToString();
                    return Int64.Parse(BuildNumber.Replace("-", "")) > Int64.Parse(BuildInfo.BuildTime.Replace("-", ""));
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task ExecuteUpdateAsync(UpdaterSession session, CancellationToken cancellationToken)
        {
            var request = (HttpWebRequest)WebRequest.Create(Program.CheckUpdateURL);
            request.Accept = "application/json";
            request.UserAgent = "WzComparerR2/1.0";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                JObject UpdateContent = JObject.Parse(responseString);
                string MajorVersion = UpdateContent.SelectToken("MajorVersion").ToString();
                string BuildNumber = UpdateContent.SelectToken("BuildNumber").ToString();
                string ChangeTitle = UpdateContent.SelectToken("ChangeTitle").ToString();
                string Changelog = UpdateContent.SelectToken("Changelog").ToString();
                net48url = UpdateContent.SelectToken("net48-url").ToString();
                net60url = UpdateContent.SelectToken("net60-url").ToString();
                net80url = UpdateContent.SelectToken("net80-url").ToString();

                this.lblLatestVer.Text = MajorVersion + "." + BuildNumber;
                AppendText(ChangeTitle + "\r\n", Color.Red);
                AppendText(Changelog, Color.Black);
                this.richTextBoxEx1.SelectionStart = 0;

                if (Int64.Parse(BuildNumber.Replace("-", "")) > Int64.Parse(BuildInfo.BuildTime.Replace("-", "")))
                {
                    buttonX1.Enabled = true;
                    this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_AVAILABLE;
                }
                else
                {
                    this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_ALREADYLATEST;
                }
                
            }
            catch (Exception ex)
            {
                this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_CHECKFAIL;
            }
        }

        private async Task DownloadUpdateAsync(string url, UpdaterSession session, CancellationToken cancellationToken)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string savePath = Path.Combine(currentDirectory, "update.zip");
            try
            {
                buttonX1.Enabled = false;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                            {
                                responseStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
                if (!File.Exists(Path.Combine(currentDirectory, "Updater.exe")))
                {
                    object UpdaterFile = Properties.Resources.ResourceManager.GetObject("Updater");
                    if (UpdaterFile is byte[] fileData)
                    {
                        File.WriteAllBytes(Path.Combine(currentDirectory, "Updater.exe"), fileData);
                    }
                }
                RunProgram("Updater.exe", "\"" + savePath + "\"");
            }
            catch (Exception ex)
            {
                this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOAD_FAIL;
            }
            finally
            {
                buttonX1.Text = LocalizedString_JP.FRMUPDATER_DESIGNER_BTN_UPDATE;
                buttonX1.Enabled = true;
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOADING;
            buttonX1.Enabled = false;
            string selectedURL = "";
            updateSession = new UpdaterSession();
            switch (Environment.Version.Major)
            {
                default:
                case 4:
                    selectedURL = net48url;
                    break;
                case 6:
                    selectedURL = net60url;
                    break;
                case 8:
                    selectedURL = net80url;
                    break;
            }
            Task.Run(() => this.DownloadUpdateAsync(selectedURL, updateSession, updateSession.CancellationToken));
        }

        private void RunProgram(string url, string argument="")
        {
#if NET6_0_OR_GREATER
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = url,
                Arguments = argument,
            });
#else
            Process.Start(url, argument);
#endif
        }

        private void chkEnableAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            var config = WcR2Config.Default;
            config.EnableAutoUpdate = chkEnableAutoUpdate.Checked;
            ConfigManager.Save();
        }

        private void AppendText(string text, Color color)
        {
            this.richTextBoxEx1.SelectionStart = this.richTextBoxEx1.TextLength;
            this.richTextBoxEx1.SelectionLength = 0;

            this.richTextBoxEx1.SelectionColor = color;
            this.richTextBoxEx1.AppendText(text);
            this.richTextBoxEx1.SelectionColor = this.richTextBoxEx1.ForeColor;
        }

        class UpdaterSession
        {
            public UpdaterSession()
            {
                this.cancellationTokenSource = new CancellationTokenSource();
            }
            public Task UpdateExecTask;

            public CancellationToken CancellationToken => this.cancellationTokenSource.Token;
            private CancellationTokenSource cancellationTokenSource;
            private TaskCompletionSource<bool> tcsWaiting;

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

        public void Load(WcR2Config config)
        {
            this.EnableAutoUpdate = config.EnableAutoUpdate;
        }
    }
}