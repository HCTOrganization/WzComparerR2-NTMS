using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

#if NET6_0_OR_GREATER
using System.Runtime.Loader;
using System.Text;
#endif

namespace WzComparerR2.CLI
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            else
            {
                switch (args[0])
                {
                    case "patch":
                        CliPatcherSession(args);
                        return;
                    default:
                        PrintUsage();
                        break;
                }
                PrintUsage();
            }
        }

        private static void CliPatcherSession(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage("patch");
                return;
            }
            CliPatcher patcher = new CliPatcher();
            switch (args[1])
            {
                case "find":
                    if (args.Length < 4)
                    {
                        PrintUsage("find");
                        return;
                    }
                    if (args[2] == "--help")
                    {
                        PrintUsage("find");
                        return;
                    }
                    else
                    {
                        if (args[2].ToUpper().Contains("MINOR"))
                        {
                            if (args.Length < 5)
                            {
                                PrintUsage("find");
                                return;
                            }
                            patcher.GameRegion = args[2].ToUpper();
                            patcher.BaseVersion = int.Parse(args[3]);
                            patcher.OldVersion = int.Parse(args[4]);
                            patcher.NewVersion = int.Parse(args[5]);
                        }
                        else
                        {
                            patcher.GameRegion = args[2].ToUpper();
                            patcher.OldVersion = int.Parse(args[3]);
                            patcher.NewVersion = int.Parse(args[4]);
                        }
                        try
                        {
                            patcher.TryGetPatch();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            return;
                        }
                        return;
                    }
                case "apply":
                    if (args.Length < 4)
                    {
                        PrintUsage("apply");
                        return;
                    }
                    if (args[2] == "--help")
                    {
                        PrintUsage("apply");
                        return;
                    }
                    else
                    {
                        bool immediatePatch = args.Contains("--immediate");
                        bool verbose = args.Contains("--verbose");
                        bool overrideMode = args.Contains("--override");
                        string patchFile = args[2];
                        string gameDirectory = args[3];
                        if (gameDirectory.Contains("\""))
                        {
                            string[] gameDirParse = gameDirectory.Split('"');
                            gameDirectory = gameDirParse[0];
                            immediatePatch = gameDirParse.Contains(" --immediate") || args.Contains("--immediate");
                            verbose = gameDirParse.Contains(" --verbose") || args.Contains("--verbose");
                            overrideMode = gameDirParse.Contains(" --override") || args.Contains("--override");
                        }
                        if (!File.Exists(patchFile))
                        {
                            Console.WriteLine("Error: Patch file or game directory does not exist, or the path is invalid.");
                            return;
                        }
                        if (!overrideMode && !File.Exists(Path.Combine(gameDirectory, "MapleStory.exe")) && !File.Exists(Path.Combine(gameDirectory, "MapleStoryT.exe")))
                        {
                            Console.WriteLine("Warning: The specified game directory seems not a valid MapleStory directory.");
                            Console.WriteLine("If you'd like to proceed anyway, press Y.");
                            Console.WriteLine("Pressing any other keys will cancel operation.");
                            ConsoleKeyInfo cki = Console.ReadKey();
                            if (cki.Key != ConsoleKey.Y) return;
                            Console.WriteLine("");
                        }
                        if (!HasWritePermission(gameDirectory))
                        {
                            Console.WriteLine("Error: You do not have write permission to the specified game directory.");
                            Console.WriteLine("To proceed, please run WCR2CLI with Administrator privilege.");
                            return;
                        }
                        try
                        {
                            patcher.OverrideMode = overrideMode;
                            patcher.ApplyPatch(patchFile, gameDirectory, immediatePatch, verbose);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            return;
                        }
                    }
                    break;
                default:
                    PrintUsage("patch");
                    return;
            }
        }

        static bool HasWritePermission(string directoryPath)
        {
            try
            {
                string testFilePath = Path.Combine(directoryPath, "test.tmp");
                using (FileStream fs = File.Create(testFilePath, 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void PrintUsage(string subarg="")
        {
            Console.WriteLine("Usage:");
            switch (subarg)
            {
                default:
                    Console.WriteLine("    wcr2cli [mode]");
                    Console.WriteLine("");
                    Console.WriteLine("Valid modes: ");
                    Console.WriteLine("    patch - Run the mini game patcher");
                    break;
                case "patch":
                    Console.WriteLine("    wcr2cli patch [option] [arguments]");
                    Console.WriteLine("");
                    Console.WriteLine("Valid options: ");
                    Console.WriteLine("    find - Locate a patch file");
                    Console.WriteLine("    apply - Apply a patch file");
                    Console.WriteLine("");
                    Console.WriteLine("For usable arguments of every options, execute: ");
                    Console.WriteLine("    wcr2cli patch find --help");
                    Console.WriteLine("    wcr2cli patch apply --help");
                    break;
                case "find":
                    Console.WriteLine("    wcr2cli patch find [game_region] [version_parameters]");
                    Console.WriteLine("");
                    Console.WriteLine("Valid game regions: KMST, KMST-Minor, KMS, KMS-Minor, CMS, MSEA, TMS");
                    Console.WriteLine("");
                    Console.WriteLine("Version Parameters instruction:");
                    Console.WriteLine("");
                    Console.WriteLine("If you're looking for MAJOR update, you'll need old version and new version number.");
                    Console.WriteLine("If you're looking for MINOR update, you'll need base version, old minor version and new minor version number.");
                    Console.WriteLine("");
                    Console.WriteLine("Example:");
                    Console.WriteLine("");
                    Console.WriteLine("Checking KMST 1.2.1186 to KMST 1.2.1187 update:");
                    Console.WriteLine("    wcr2cli patch find KMST 1186 1187");
                    Console.WriteLine("");
                    Console.WriteLine("Checking KMS 1.2.403 minor 1 to 4 update:");
                    Console.WriteLine("    wcr2cli patch find KMS-Minor 403 1 4");
                    break;
                case "apply":
                    Console.WriteLine("    wcr2cli patch apply [patch_file_path] [game_installation_path] [--immediate] [--verbose]");
                    Console.WriteLine("");
                    Console.WriteLine("Supplying \"--immediate\" switch will enable Immediate Patch.");
                    Console.WriteLine("Supplying \"--verbose\" switch will enable verbose output (not implemented yet).");
                    Console.WriteLine("");
                    Console.WriteLine("Both patch file path and game installation path must be absolute path.");
                    Console.WriteLine("If the path contains spaces, the path must be wrapped by a pair of quotes.");
                    Console.WriteLine("");
                    Console.WriteLine("Example:");
                    Console.WriteLine("");
                    Console.WriteLine("    wcr2cli patch apply E:\\Downloads\\00269to00270.patch \"N:\\Games\\MapleStory TW\" --immediate");
                    break;
            }
        }
    }
}