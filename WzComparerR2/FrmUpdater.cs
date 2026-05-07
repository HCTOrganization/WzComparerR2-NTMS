using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using WzComparerR2.Config;
using WzComparerR2.Controls;

namespace WzComparerR2
{
    public partial class FrmUpdater : DevComponents.DotNetBar.Office2007Form
    {
        public FrmUpdater() : this(new Updater())
        {
        }

        public FrmUpdater(Updater updater)
        {
            InitializeComponent();
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif

            this.Updater = updater;
            this.lblCurrentVer.Text = $"{Program.WcR2MajorVersion}{BuildInfo.BuildTime}";
        }

        public Updater Updater { get; set; }

        public bool EnableAutoUpdate
        {
            get { return chkEnableAutoUpdate.Checked; }
            set { chkEnableAutoUpdate.Checked = value; }
        }

        private CancellationTokenSource cts;

        private async void FrmUpdater_Load(object sender, EventArgs e)
        {
            var updater = this.Updater;
            if (!updater.LatestReleaseFetched)
            {
                using var cts = new CancellationTokenSource();
                this.cts = cts;
                try
                {
                    await updater.QueryUpdateAsync(cts.Token);
                    
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_CHECKFAIL;
                    this.AppendText(ex.Message + "\r\n" + ex.StackTrace, Color.Red);
                    return;
                }
                finally
                {
                    this.cts = null;
                }
            }

            if (updater.LatestReleaseFetched)
            {
                this.lblLatestVer.Text = updater.LatestVersionString;
                this.AppendText(updater.Release?.ChangeTitle + Environment.NewLine, Color.Red);
                this.AppendText(updater.Release?.Changelog, Color.Black);
                this.richTextBoxEx1.SelectionStart = 0;
                if (updater.UpdateAvailable)
                {
                    this.buttonX1.Enabled = true;
                    this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_AVAILABLE;
                }
                else
                {
                    this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_ALREADYLATEST;
                }
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            var updater = this.Updater;

            var runtimeVer = Environment.Version.Major;
            var asset = runtimeVer switch
            {
                4 => updater.Release.Net48Url,
                6 => updater.Release.Net60Url,
                8 => updater.Release.Net80Url,
                10 => updater.Release.Net100Url,
                _ => null,
            };

            if (asset == null)
            {
                MessageBoxEx.Show(this, $"未找到 .NET {runtimeVer} 版本。請訪問專案網頁進行下載。", "注意", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.cts != null)
            {
                MessageBoxEx.Show(this, "正在進行別的任務。請稍後重試。", "注意", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var cts = new CancellationTokenSource();
            this.cts = cts;
            this.buttonX1.Enabled = false;
            this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOADING;

            try
            {
                string savePath = Path.Combine(Application.StartupPath, "update.zip");
                var result = ProgressDialog.Show(this, LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOADING, "Updater", true, true, async (ctx, cancellationToken) =>
                {
                    cancellationToken.Register(() => cts.Cancel());
                    
                    try
                    {
                        await updater.DownloadAssetAsync(asset, savePath, (downloaded, total) =>
                        {
                            if (total > 0)
                            {
                                if (ctx.Progress == 0)
                                {
                                    ctx.ProgressMin = 0;
                                    ctx.ProgressMax = (int)total;
                                }
                                ctx.Progress = (int)downloaded;
                                ctx.Message = $"已下載: {(1.0 * downloaded / total):P1}";
                            }
                            else
                            {
                                ctx.Message = $"已下載: {downloaded:N0}";
                            }
                        }, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_CANCELLED;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_DOWNLOAD_FAIL;
                        this.AppendText(ex.Message + "\r\n" + ex.StackTrace, Color.Red);
                        throw;
                    }
                });

                if (result == DialogResult.OK)
                {
                    this.ExecuteUpdater(savePath);
                }
            }
            catch (Exception ex)
            {
                this.lblUpdateContent.Text = LocalizedString_JP.FRMUPDATER_UPDATE_FAIL;
                AppendText(ex.Message + "\r\n" + ex.StackTrace, Color.Red);
            }
            finally
            {
                this.cts = null;
                if (!this.IsDisposed)
                {
                    this.buttonX1.Enabled = true;
                }
            }
        }

        private void ExecuteUpdater(string assetFileName)
        {
            string wcR2Folder = Application.StartupPath;
            ExtractResource("WzComparerR2.Updater.exe", Path.Combine(wcR2Folder, "Updater.exe"));
#if NET6_0_OR_GREATER
            ExtractResource("WzComparerR2.Updater.deps.json", Path.Combine(wcR2Folder, "Updater.deps.json"));
            ExtractResource("WzComparerR2.Updater.dll", Path.Combine(wcR2Folder, "Updater.dll"));
            ExtractResource("WzComparerR2.Updater.dll.config", Path.Combine(wcR2Folder, "Updater.dll.config"));
            ExtractResource("WzComparerR2.Updater.runtimeconfig.json", Path.Combine(wcR2Folder, "Updater.runtimeconfig.json"));
#else
            ExtractResource("WzComparerR2.Updater.exe.config", Path.Combine(wcR2Folder, "Updater.exe.config"));
#endif
            RunProgram("Updater.exe", "\"" + assetFileName + "\"");
        }

        private void RunProgram(string url, string argument)
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

        private void AppendText(string text, Color color)
        {
            this.richTextBoxEx1.SelectionStart = this.richTextBoxEx1.TextLength;
            this.richTextBoxEx1.SelectionLength = 0;

            this.richTextBoxEx1.SelectionColor = color;
            this.richTextBoxEx1.AppendText(text);
            this.richTextBoxEx1.SelectionColor = this.richTextBoxEx1.ForeColor;
        }

        private void ExtractResource(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                throw new InvalidOperationException($"Resource not found: {resourceName}");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
        }

        private void chkEnableAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            var config = WcR2Config.Default;
            config.EnableAutoUpdate = chkEnableAutoUpdate.Checked;
            ConfigManager.Save();
        }

        public void LoadConfig(WcR2Config config)
        {
            this.EnableAutoUpdate = config.EnableAutoUpdate;
        }

        private void FrmUpdater_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (this.cts != null)
            {
                this.cts.Cancel();
            }
        }
    }
}