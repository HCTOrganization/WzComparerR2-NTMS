using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
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
        public FrmUpdater() : this(new Updater(), false)
        {
        }

        public FrmUpdater(Updater updater) : this(updater, false)
        {
        }

        public FrmUpdater(Updater updater, bool isDarkMode)
        {
#if NET6_0_OR_GREATER
            // https://learn.microsoft.com/en-us/dotnet/core/compatibility/fx-core#controldefaultfont-changed-to-segoe-ui-9pt
            this.Font = new Font(new FontFamily("SimSun"), 9f);
#endif
            InitializeComponent();

            this.IsDarkMode = isDarkMode;
            this.Updater = updater;
            this.lblCurrentVer.Text = $"{Program.WcR2MajorVersion}{BuildInfo.BuildTime}";
        }

        public Updater Updater { get; set; }
        public bool IsDarkMode { get; private set; }

        public bool EnableAutoUpdate
        {
            get { return chkEnableAutoUpdate.Checked; }
            set { chkEnableAutoUpdate.Checked = value; }
        }

        private CancellationTokenSource cts;

        // Maps character range (start index) to URL for markdown [text](url) links
        private readonly Dictionary<int, (int Length, string Url)> _linkRanges = new Dictionary<int, (int, string)>();

        private async void FrmUpdater_Load(object sender, EventArgs e)
        {
            if (IsDarkMode)
            {
                this.richTextBoxEx1.BackColorRichTextBox = Color.FromArgb(-13816528);
                this.richTextBoxEx1.ForeColor = Color.LightGray;
            }

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
                this.AppendText(updater.Release?.ChangeTitle + Environment.NewLine, IsDarkMode ? Color.SkyBlue : Color.DarkBlue);
                this.AppendMarkdown(updater.Release?.Changelog);
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

        private void AppendMarkdown(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return;

            var rtb = this.richTextBoxEx1;
            var baseFont = rtb.Font;
            var lines = markdown.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            // Derive accent colors from the RTB background so they work in both light and dark themes.
            bool isDark = rtb.BackColor.GetBrightness() < 0.5f;
            Color headingColor = isDark ? Color.CornflowerBlue : Color.DarkBlue;
            Color rulerColor   = isDark ? Color.Gray        : Color.Gray;
            Color quoteColor   = isDark ? Color.DarkGray       : Color.Gray;
            Color linkColor    = isDark ? Color.SkyBlue         : Color.Blue;
            Color codeColor    = isDark ? Color.LightCoral      : Color.DarkRed;

            foreach (var rawLine in lines)
            {
                string line = rawLine;

                // Horizontal rule
                if (Regex.IsMatch(line, @"^[-*_]{3,}\s*$"))
                {
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;
                    rtb.SelectionColor = rulerColor;
                    rtb.AppendText(new string('─', 40) + "\n");
                    rtb.SelectionColor = rtb.ForeColor;
                    continue;
                }

                // ATX headings: # / ## / ###
                var headingMatch = Regex.Match(line, @"^(#{1,3})\s+(.+)$");
                if (headingMatch.Success)
                {
                    int level = headingMatch.Groups[1].Length;
                    string content = headingMatch.Groups[2].Value;
                    float size = level == 1 ? baseFont.Size + 4 :
                                 level == 2 ? baseFont.Size + 2 :
                                              baseFont.Size + 1;
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;
                    rtb.SelectionFont = new Font(baseFont.FontFamily, size, FontStyle.Bold);
                    rtb.SelectionColor = headingColor;
                    rtb.AppendText(content + "\n");
                    rtb.SelectionFont = baseFont;
                    rtb.SelectionColor = rtb.ForeColor;
                    continue;
                }

                // Blockquote: > text
                if (line.StartsWith("> "))
                {
                    string content = line.Substring(2);
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;
                    rtb.SelectionColor = quoteColor;
                    rtb.SelectionFont = new Font(baseFont, FontStyle.Italic);
                    rtb.AppendText("  │ " + content + "\n");
                    rtb.SelectionFont = baseFont;
                    rtb.SelectionColor = rtb.ForeColor;
                    continue;
                }

                // Bullet list: - or * or + at start
                var bulletMatch = Regex.Match(line, @"^(\s*)[-*+]\s+(.+)$");
                if (bulletMatch.Success)
                {
                    string indent = bulletMatch.Groups[1].Value;
                    string content = bulletMatch.Groups[2].Value;
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;
                    AppendInlineMarkdown(rtb, baseFont, indent + "• " + content + "\n", linkColor, codeColor);
                    continue;
                }

                // Plain paragraph (including empty lines)
                AppendInlineMarkdown(rtb, baseFont, line + "\n", linkColor, codeColor);
            }
        }

        /// <summary>
        /// Appends a single line of text, processing inline markdown:
        /// **bold**, *italic*, `code`, [text](url), and plain text segments.
        /// </summary>
        private void AppendInlineMarkdown(DevComponents.DotNetBar.Controls.RichTextBoxEx rtb, Font baseFont, string line, Color linkColor, Color codeColor)
        {
            // Order matters: images ![...](...) must be stripped before links [...](...),
            // and ** must be tried before * to avoid partial matches.
            var pattern = @"(!\[.*?\]\(.*?\)|\[(.+?)\]\((https?://[^\)]+)\)|\*\*(.+?)\*\*|\*(.+?)\*|`(.+?)`)";
            int lastIndex = 0;
            var matches = Regex.Matches(line, pattern);

            foreach (Match m in matches)
            {
                // Plain text before this match
                if (m.Index > lastIndex)
                {
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.SelectionLength = 0;
                    rtb.SelectionFont = baseFont;
                    rtb.SelectionColor = rtb.ForeColor;
                    rtb.AppendText(line.Substring(lastIndex, m.Index - lastIndex));
                }

                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;

                if (m.Value.StartsWith("!["))
                {
                    // Embedded image — skip entirely (do not render)
                }
                else if (m.Value.StartsWith("["))
                {
                    // Hyperlink: [display text](url)
                    string displayText = m.Groups[2].Value;
                    string url = m.Groups[3].Value;
                    int linkStart = rtb.TextLength;
                    rtb.SelectionFont = new Font(baseFont, FontStyle.Underline);
                    rtb.SelectionColor = linkColor;
                    rtb.AppendText(displayText);
                    _linkRanges[linkStart] = (displayText.Length, url);
                }
                else if (m.Value.StartsWith("**"))
                {
                    // Bold
                    rtb.SelectionFont = new Font(baseFont, FontStyle.Bold);
                    rtb.SelectionColor = rtb.ForeColor;
                    rtb.AppendText(m.Groups[4].Value);
                }
                else if (m.Value.StartsWith("*"))
                {
                    // Italic
                    rtb.SelectionFont = new Font(baseFont, FontStyle.Italic);
                    rtb.SelectionColor = rtb.ForeColor;
                    rtb.AppendText(m.Groups[5].Value);
                }
                else if (m.Value.StartsWith("`"))
                {
                    // Inline code
                    rtb.SelectionFont = new Font(FontFamily.GenericMonospace, baseFont.Size);
                    rtb.SelectionColor = codeColor;
                    rtb.AppendText(m.Groups[6].Value);
                }

                rtb.SelectionFont = baseFont;
                rtb.SelectionColor = rtb.ForeColor;
                lastIndex = m.Index + m.Length;
            }

            // Remaining plain text after last match
            if (lastIndex < line.Length)
            {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionFont = baseFont;
                rtb.SelectionColor = rtb.ForeColor;
                rtb.AppendText(line.Substring(lastIndex));
            }
        }

        private void RichTextBoxEx1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int charIndex = this.richTextBoxEx1.GetCharIndexFromPosition(e.Location);
            foreach (var kvp in _linkRanges)
            {
                if (charIndex >= kvp.Key && charIndex < kvp.Key + kvp.Value.Length)
                {
                    try
                    {
#if NET6_0_OR_GREATER
                        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = kvp.Value.Url });
#else
                        Process.Start(kvp.Value.Url);
#endif
                    }
                    catch { }
                    return;
                }
            }
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