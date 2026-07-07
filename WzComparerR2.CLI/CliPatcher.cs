using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WzComparerR2.CLI.Patcher;

namespace WzComparerR2.CLI
{
    internal class CliPatcher
    {
        public string GameRegion { get; set; }
        public int OldVersion { get; set; } = 0;
        public int NewVersion { get; set; } = 0;
        public int BaseVersion { get; set; } = 0;
        public bool OverrideMode { get; set; }
        private long availableDiskSpace;
        private bool isUpdating;
        private PatcherSession patcherSession;

        SortedDictionary<string, long> patchedFileSizes = new SortedDictionary<string, long>();
        List<string> patchedFileIndex = new List<string>();
        Dictionary<string, string> finishedFileIndex = new Dictionary<string, string>();
        public Encoding PatcherNoticeEncoding { get; set; }

        private static Dictionary<string, string> RegionUrlTemplate = new Dictionary<string, string>()
        {
            {"KMST", "http://maplestory.dn.nexoncdn.co.kr/PatchT/{1:d5}/{0:d5}to{1:d5}.patch"},
            {"KMST-MINOR", "http://maplestory.dn.nexoncdn.co.kr/PatchT/{0:d5}/Minor/{1:d2}to{2:d2}.patch"},
            {"KMS", "http://maplestory.dn.nexoncdn.co.kr/Patch/{1:d5}/{0:d5}to{1:d5}.patch"},
            {"KMS-MINOR", "http://maplestory.dn.nexoncdn.co.kr/Patch/{0:d5}/Minor/{1:d2}to{2:d2}.patch"},
            {"CMS", "http://mxd.clientdown.sdo.com/maplestory/patch/patchdir/{1:d5}/{0:d5}to{1:d5}.patch" },
            {"MSEA", "http://patch.maplesea.com/sea/patch/patchdir/{1:d5}/{0:d5}to{1:d5}.patch" },
            {"MSEA-ZIP", "http://download-maple.playpark.net/manual/MaplePatch{0:d3}to{1:d3}.zip" },
            {"TMS", "http://tw.cdnpatch.maplestory.beanfun.com/maplestory/patch/patchdir/{1:d5}/{0:d5}to{1:d5}.patch" }
        };

        public void TryGetPatch()
        {
            string url = "";
            if (string.IsNullOrEmpty(GameRegion) || !RegionUrlTemplate.TryGetValue(GameRegion, out string template))
            {
                throw new ArgumentException("Invalid game region.");
            }
            if (OldVersion < 0 || NewVersion < 0)
            {
                throw new ArgumentOutOfRangeException("Version numbers must be non-negative.");
            }
            if (GameRegion.Contains("MINOR"))
            {
                url = string.Format(template, BaseVersion, OldVersion, NewVersion);
            }
            else
            {
                url = string.Format(template, OldVersion, NewVersion);
            }
            DownloadingItem item = new DownloadingItem(url, null);
            try
            {
                Console.WriteLine("Finding patch file, please wait...");
                item.GetFileLength();
                if (item.FileLength > 0)
                {
                    Console.WriteLine($"Patch URL: {item.Url}");
                    Console.WriteLine($"File Length: {item.FileLength} bytes");
                    Console.WriteLine($"Modification Date: {item.LastModified}");
                    Console.WriteLine("Do you want to download this patch?");
                    Console.WriteLine("Press Y - Download");
                    Console.WriteLine("Press C - Copy Download URL to Clipboard");
                    Console.WriteLine("Press N - Do not download");
                    ConsoleKeyInfo cki = Console.ReadKey();
                    int responseType = ReadYNC(cki);
                    switch (responseType)
                    {
                        case 0:
#if NET6_0_OR_GREATER
                            Process.Start(new ProcessStartInfo
                            {
                                UseShellExecute = true,
                                FileName = url,
                            });
#else
                            Process.Start(url);
#endif
                            return;
                        case 1:
                            Clipboard.SetText(url);
                            return;
                        case 2:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving patch information: {ex.Message}");
            }
        }

        public void ApplyPatch(string patchFile, string gameDirectory, bool immediatePatch, bool verbose, bool keepOldWz)
        {
            if (!this.ValidateKeepOldWzBeforePatch(gameDirectory, keepOldWz))
            {
                Console.WriteLine("Patch cancelled.");
                return;
            }

            var session = new PatcherSession()
            {
                PatchFile = patchFile,
                MSFolder = gameDirectory,
                PrePatch = immediatePatch,
                DeadPatch = immediatePatch && !keepOldWz,
                KeepOldWz = keepOldWz,
            };
            session.LoggingFileName = Path.Combine(session.MSFolder, $"wcpatcher_{DateTime.Now:yyyyMMdd_HHmmssfff}.log");
            session.PatchExecTask = Task.Run(() => this.ExecutePatchAsync(session, session.CancellationToken, verbose));
            this.patcherSession = session;
            Task.WaitAny(session.PatchExecTask);
        }

        private bool ValidateKeepOldWzBeforePatch(string gameDirectory, bool keepOldWz)
        {
            if (!keepOldWz)
            {
                return true;
            }

            string backupDir = Path.Combine(gameDirectory, "DataBk");
            if (!Directory.Exists(backupDir))
            {
                return true;
            }

            try
            {
                RemoveReadOnlyAttributesRecursively(backupDir);
                Directory.Delete(backupDir, true);
                Console.WriteLine("Existing DataBk directory has been deleted.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Failed to delete existing DataBk directory. {ex.Message}");
                Console.WriteLine("Please close processes using files under DataBk and try again.");
                return false;
            }
        }

        private static int ReadYNC(ConsoleKeyInfo cki)
        {
            switch (cki.Key)
            {
                case ConsoleKey.Y:
                    return 0;
                case ConsoleKey.C:
                    return 1;
                case ConsoleKey.N:
                    return 2;
                default:
                    Console.WriteLine("Invalid Input, please try again.");
                    ConsoleKeyInfo retryCki = Console.ReadKey();
                    return ReadYNC(retryCki);
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

        private async Task ExecutePatchAsync(PatcherSession session, CancellationToken cancellationToken, bool verbose)
        {
            void AppendStateText(string text)
            {
                Console.Write(text);
                if (session.LoggingFileName != null)
                {
                    File.AppendAllText(session.LoggingFileName, text, Encoding.UTF8);
                }
            }
            WzPatcher patcher = null;
            session.State = PatcherTaskState.Prepatch;

            try
            {
                patcher = new WzPatcher(session.PatchFile);
                patcher.NoticeEncoding = this.PatcherNoticeEncoding ?? Encoding.Default;
                patcher.PatchingStateChanged += (o, e) => this.patcher_PatchingStateChanged(o, e, session, AppendStateText);
                AppendStateText($"更新檔案名稱: {session.PatchFile}\r\n");
                AppendStateText("更新檔案分析中...");
                long patchedAllFileSize = 0;
                long decompressedSize = patcher.PrePatch(cancellationToken);
                availableDiskSpace = RemainingDiskSpace(session.MSFolder);
                if (verbose)
                {
                    Console.WriteLine(patcher.NoticeText);
                }
                patcher.OpenDecompress(cancellationToken);
                StringBuilder diskSpaceMessage = new StringBuilder();
                patchedFileSizes.Add("ZOther", 0);
                foreach (PatchPartContext part in patcher.PatchParts)
                {
                    switch (part.Type)
                    {
                        case 0:
                        case 1:
                            patchedAllFileSize += part.NewFileLength;
                            break;
                        case 2:
                            patchedAllFileSize -= part.NewFileLength;
                            break;
                    }
                    if (patcher.IsKMST1125Format.Value)
                    {
                        string[] patchedFileDirectory = part.FileName.Split('\\');
                        if (part.Type == 1 && (patchedFileDirectory[0] == "Data" || patchedFileDirectory[0] == "NxOverlay")) patchedFileIndex.Add(part.FileName);
                        if (patchedFileDirectory[0] == "Data")
                        {
                            if (!patchedFileSizes.ContainsKey(patchedFileDirectory[1])) patchedFileSizes.Add(patchedFileDirectory[1], 0);
                            switch (part.Type)
                            {
                                case 0:
                                case 1:
                                    patchedFileSizes[patchedFileDirectory[1]] += part.NewFileLength;
                                    break;
                                case 2:
                                    patchedFileSizes[patchedFileDirectory[1]] -= part.NewFileLength;
                                    break;
                            }
                        }
                        else
                        {
                            switch (part.Type)
                            {
                                case 0:
                                case 1:
                                    patchedFileSizes["ZOther"] += part.NewFileLength;
                                    break;
                                case 2:
                                    patchedFileSizes["ZOther"] -= part.NewFileLength;
                                    break;
                            }
                        }
                    }
                }
                foreach (string key in patchedFileSizes.Keys)
                {
                    switch (key)
                    {
                        case "ZOther":
                            diskSpaceMessage.AppendLine(string.Format("其他檔案所需的空間: {0}", GetBothByteAndGBValue(patchedFileSizes[key])));
                            break;
                        default:
                            diskSpaceMessage.AppendLine(string.Format("「{0}」所需空間: {1}", key, GetBothByteAndGBValue(patchedFileSizes[key])));
                            break;
                    }
                }
                patchedFileIndex.Sort();
                diskSpaceMessage.AppendLine(string.Format("所需總空間: {0}", GetBothByteAndGBValue(patchedAllFileSize)));
                diskSpaceMessage.AppendLine(string.Format("可用磁碟空間: {0}", GetBothByteAndGBValue(availableDiskSpace)));
                AppendStateText(diskSpaceMessage.ToString());
                AppendStateText("結束\r\n");
                if ((patchedAllFileSize > availableDiskSpace) && !this.OverrideMode)
                {
                    Console.WriteLine("可能沒有足夠的磁碟空間來完成更新。");
                    Console.WriteLine("您還想繼續嗎？");
                    Console.WriteLine("如繼續請按一下Y。按下其它鍵將會取消操作。");
                    ConsoleKeyInfo cki = Console.ReadKey();
                    if (cki.Key != ConsoleKey.Y)
                    {
                        AppendStateText("更新已中止\r\n");
                        throw new OperationCanceledException("更新已中止。");
                    }
                }
                AppendStateText("更新準備中...\r\n");
                if (patcher.IsKMST1125Format.Value)
                {
                    AppendStateText("更新類別: KMST1125\r\n");
                    if (patcher.OldFileHash != null)
                    {
                        AppendStateText($"更新前需要檢查校驗和的檔案數量: {patcher.OldFileHash.Count}\r\n");
                    }
                }
                AppendStateText(string.Format("更新大小: {0:N0} 位元組\r\n", decompressedSize));
                AppendStateText(string.Format("需要更新的文件數量: {0}\r\n", patcher.PatchParts.Count));
                if (patcher.IsKMST1125Format.Value && session.DeadPatch)
                {
                    AppendStateText("立即補丁執行計劃\r\n");
                    session.deadPatchExecutionPlan = new();
                    session.deadPatchExecutionPlan.Build(patcher.PatchParts);
                    foreach (var part in patcher.PatchParts)
                    {
                        if (session.deadPatchExecutionPlan.Check(part.FileName, out var filesCanInstantUpdate))
                        {
                            AppendStateText($"+ 檔案[{part.FileName}]執行\r\n");
                            foreach (var fileName in filesCanInstantUpdate)
                            {
                                AppendStateText($"  - 檔案[{fileName}]應用\r\n");
                            }
                        }
                        else
                        {
                            AppendStateText($"- 檔案[{part.FileName}]執行，但推遲應用\r\n");
                        }
                    }
                    // disable force validation
                    patcher.ThrowOnValidationFailed = false;
                }
                AppendStateText("應用更新\r\n");
                var sw = Stopwatch.StartNew();
                patcher.Patch(session.MSFolder, cancellationToken);
                this.RestoreMissingDataFilesFromBackup(session, patcher.PatchParts, AppendStateText);
                sw.Stop();
                AppendStateText("結束\r\n");
                session.State = PatcherTaskState.Complete;
                Console.WriteLine("更新結束。經過時間: " + sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("更新已停止。");
            }
            catch (UnauthorizedAccessException ex)
            {
                // File IO permission error
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                AppendStateText(ex.ToString());
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                session.State = PatcherTaskState.Complete;
                if (patcher != null)
                {
                    patcher.Close();
                    patcher = null;
                }
                GC.Collect();
            }
        }

        class PatcherSession
        {
            public PatcherSession()
            {
                this.cancellationTokenSource = new CancellationTokenSource();
            }

            public string PatchFile;
            public string MSFolder;
            public string CompareFolder;
            public bool PrePatch;
            public bool DeadPatch;
            public bool KeepOldWz;
            public bool KeepOldWzPrepared;

            public Task PatchExecTask;
            public string LoggingFileName;
            public PatcherTaskState State;

            public DeadPatchExecutionPlan deadPatchExecutionPlan;
            public Dictionary<string, string> TemporaryFileMapping = new();

            public CancellationToken CancellationToken => this.cancellationTokenSource.Token;
            private CancellationTokenSource cancellationTokenSource;
            private TaskCompletionSource<bool> tcsWaiting;

            public bool IsCompleted => this.PatchExecTask?.IsCompleted ?? true;

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

        enum PatcherTaskState
        {
            NotStarted = 0,
            Prepatch = 1,
            WaitForContinue = 2,
            Patching = 3,
            Complete = 4,
        }

        private void patcher_PatchingStateChanged(object sender, PatchingEventArgs e, PatcherSession session, Action<string> logFunc)
        {
            switch (e.State)
            {
                case PatchingState.PatchStart:
                    logFunc("[" + e.Part.FileName + "] 應用更新中 \r\n");
                    break;
                case PatchingState.VerifyOldChecksumBegin:
                    logFunc("  檢查補丁前的校驗和...");
                    break;
                case PatchingState.VerifyOldChecksumEnd:
                    logFunc("  結束\r\n");
                    break;
                case PatchingState.VerifyNewChecksumBegin:
                    logFunc("  檢查補丁後的校驗和...");
                    break;
                case PatchingState.VerifyNewChecksumEnd:
                    logFunc("  結束\r\n");
                    break;
                case PatchingState.TempFileCreated:
                    logFunc("  建立臨時檔案...\r\n");
                    //progressBarX1.Maximum = e.Part.NewFileLength;
                    session.TemporaryFileMapping.Add(e.Part.FileName, e.Part.TempFilePath);
                    break;
                case PatchingState.TempFileBuildProcessChanged:
                    //progressBarX1.Value = (int)e.CurrentFileLength;
                    //progressBarX1.Text = string.Format("{0:N0}/{1:N0}", e.CurrentFileLength, e.Part.NewFileLength);
                    break;
                case PatchingState.TempFileClosed:
                    logFunc("  已建立臨時檔案。\r\n");
                    //progressBarX1.Value = 0;
                    //progressBarX1.Maximum = 0;
                    //progressBarX1.Text = string.Empty;

                    if (session.DeadPatch && e.Part.Type == 1 && sender is WzPatcher patcher)
                    {
                        if (patcher.IsKMST1125Format.Value)
                        {
                            if (session.deadPatchExecutionPlan?.Check(e.Part.FileName, out var filesCanInstantUpdate) ?? false)
                            {
                                long currentUsedDiskSpace = availableDiskSpace - RemainingDiskSpace(session.MSFolder);
                                logFunc(string.Format("  (即時更新)已占用磁碟空間: {0}\r\n", GetBothByteAndGBValue(currentUsedDiskSpace)));
                                foreach (string fileName in filesCanInstantUpdate)
                                {
                                    if (session.TemporaryFileMapping.TryGetValue(fileName, out var temporaryFileName))
                                    {
                                        logFunc($"  (即時更新)檔案[{fileName}]應用中...\r\n");
                                        patcher.SafeMove(temporaryFileName, Path.Combine(session.MSFolder, fileName));
                                    }
                                }
                            }
                            else
                            {
                                logFunc("  (即時更新)檔案推遲應用...\r\n");
                            }
                        }
                        else
                        {
                            logFunc("  (即時更新)檔案應用中...\r\n");
                            patcher.SafeMove(e.Part.TempFilePath, e.Part.OldFilePath);
                        }
                    }
                    break;
                case PatchingState.PrepareVerifyOldChecksumBegin:
                    logFunc($"檢查補丁前的校驗和: {e.Part.FileName}");
                    break;
                case PatchingState.PrepareVerifyOldChecksumEnd:
                    if (e.Part.OldChecksum != e.Part.OldChecksumActual)
                    {
                        logFunc(" 不一致\r\n");
                    }
                    else
                    {
                        logFunc(" 結束\r\n");
                    }
                    break;
                case PatchingState.ApplyFile:
                    if (session.KeepOldWz && !session.KeepOldWzPrepared)
                    {
                        this.PrepareKeepOldWz(session, logFunc);
                        session.KeepOldWzPrepared = true;
                    }
                    logFunc($"檔案應用: {e.Part.FileName}\r\n");
                    break;
                case PatchingState.FileSkipped:
                    logFunc("  已跳過檔案: " + e.Part.FileName + "\r\n");
                    break;
            }
        }

        private string GetBothByteAndGBValue(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double targetbytes = size;
            int order = 0;

            while (targetbytes >= 1024 && order + 1 < sizes.Length)
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

        private void PrepareKeepOldWz(PatcherSession session, Action<string> logFunc)
        {
            if (!session.KeepOldWz)
            {
                return;
            }

            string dataDir = Path.Combine(session.MSFolder, "Data");
            string backupDir = Path.Combine(session.MSFolder, "DataBk");

            if (!Directory.Exists(dataDir))
            {
                logFunc("KeepOldWz: Data 資料夾不存在，跳過。\r\n");
                return;
            }

            if (Directory.Exists(backupDir))
            {
                logFunc("KeepOldWz: 既存的 DataBk 資料夾正在刪除中...\r\n");
                RemoveReadOnlyAttributesRecursively(backupDir);
                Directory.Delete(backupDir, true);
            }

            logFunc("KeepOldWz: Data 資料夾正在重新命名為 DataBk...\r\n");
            Directory.Move(dataDir, backupDir);
            logFunc("KeepOldWz: 完了\r\n");
        }

        private void RestoreMissingDataFilesFromBackup(PatcherSession session, IEnumerable<PatchPartContext> patchParts, Action<string> logFunc)
        {
            if (!session.KeepOldWz)
            {
                return;
            }

            string dataDir = Path.Combine(session.MSFolder, "Data");
            string backupDir = Path.Combine(session.MSFolder, "DataBk");
            if (!Directory.Exists(backupDir))
            {
                logFunc("KeepOldWz: DataBk 資料夾不存在，跳過補齊缺失的檔案。\r\n");
                return;
            }

            Directory.CreateDirectory(dataDir);
            var deletedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var deletedDirs = new List<string>();
            foreach (var part in patchParts.Where(p => p.Type == 2))
            {
                var fileName = (part.FileName ?? string.Empty).Replace('/', '\\');
                if (!fileName.StartsWith("Data\\", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var relativePath = fileName.Substring("Data\\".Length).TrimEnd('\\');
                if (string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }

                if (fileName.EndsWith("\\", StringComparison.Ordinal))
                {
                    deletedDirs.Add(relativePath);
                }
                else
                {
                    deletedFiles.Add(relativePath);
                }
            }

            int copiedCount = 0;
            foreach (string backupFile in Directory.EnumerateFiles(backupDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = backupFile.Substring(backupDir.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('/', '\\');
                if (deletedFiles.Contains(relativePath))
                {
                    continue;
                }
                if (deletedDirs.Any(dir => relativePath.Equals(dir, StringComparison.OrdinalIgnoreCase)
                    || relativePath.StartsWith(dir + "\\", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                string targetFile = Path.Combine(dataDir, relativePath);
                if (File.Exists(targetFile))
                {
                    continue;
                }

                string targetDir = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                File.Copy(backupFile, targetFile, false);
                copiedCount++;
            }

            logFunc($"KeepOldWz: DataBk 資料夾中已複製 {copiedCount} 個缺失的檔案。\r\n");
        }

        private static void RemoveReadOnlyAttributesRecursively(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                var attr = File.GetAttributes(filePath);
                if ((attr & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(filePath, attr & ~FileAttributes.ReadOnly);
                }
            }

            foreach (string dirPath in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories))
            {
                var attr = File.GetAttributes(dirPath);
                if ((attr & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(dirPath, attr & ~FileAttributes.ReadOnly);
                }
            }

            var rootAttr = File.GetAttributes(directoryPath);
            if ((rootAttr & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(directoryPath, rootAttr & ~FileAttributes.ReadOnly);
            }
        }

        class DeadPatchExecutionPlan
        {
            public DeadPatchExecutionPlan()
            {
                this.FileUpdateDependencies = new Dictionary<string, List<string>>();
            }

            public Dictionary<string, List<string>> FileUpdateDependencies { get; private set; }

            public void Build(IEnumerable<PatchPartContext> orderedParts)
            {
                /*
                 *  for examle:
                 *    fileName   | type | dependencies               
                 *    -----------|------|---------------     
                 *    Mob_000.wz | 1    | Mob_000.wz   (self update)
                 *    Mob_001.wz | 1    | Mob_001.wz, Mob_002.wz  (merge data)
                 *    Mob_002.wz | 1    | Mob_001.wz, Mob_002.wz  (merge data)
                 *    Mob_003.wz | 1    | Mob_001.wz, Mob_002.wz  (balance size from other file)
                 *                                                 
                 *  fileLastDependecy:                             
                 *    key        | value                           
                 *    -----------|----------------                 
                 *    Mob_000.wz | Mob_000.wz
                 *    Mob_001.wz | Mob_003.wz
                 *    Mob_002.wz | Mob_003.wz
                 *    Mob_003.wz | Mob_003.wz
                 *    
                 *  FileUpdateDependencies:
                 *    key        | value
                 *    -----------|----------------
                 *    Mob_000.wz | Mob000.wz
                 *    Mob_003.wz | Mob001.wz, Mob002.wz, Mob003.wz
                 */

                // find the last dependency
                Dictionary<string, string> fileLastDependecy = new();
                foreach (var part in orderedParts)
                {
                    if (part.Type == 0)
                    {
                        fileLastDependecy[part.FileName] = part.FileName;
                    }
                    else if (part.Type == 1)
                    {
                        fileLastDependecy[part.FileName] = part.FileName;
                        foreach (var dep in part.DependencyFiles)
                        {
                            fileLastDependecy[dep] = part.FileName;
                        }
                    }
                }

                // reverse key and value
                this.FileUpdateDependencies.Clear();
                foreach (var grp in fileLastDependecy.GroupBy(kv => kv.Value, kv => kv.Key))
                {
                    this.FileUpdateDependencies.Add(grp.Key, grp.ToList());
                }
            }

            public bool Check(string fileName, out IReadOnlyList<string> filesCanInstantUpdate)
            {
                if (this.FileUpdateDependencies.TryGetValue(fileName, out var value) && value != null && value.Count > 0)
                {
                    filesCanInstantUpdate = value;
                    return true;
                }

                filesCanInstantUpdate = null;
                return false;
            }
        }
    }
}
