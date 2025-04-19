using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WzComparerR2.PluginBase;

#if NET6_0_OR_GREATER
using System.Runtime.Loader;
using System.Text;
#endif

namespace WzComparerR2
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Program.SetDllDirectory();
#if NET6_0_OR_GREATER
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Dotnet6Patch.Patch();
#endif
            Program.StartMainForm();
        }

        public static string LibPath { get; private set; }
        private static List<Assembly> loadedPluginAssemblies = new List<Assembly>();
        public static string NxAPIBaseURL = "https://open.api.nexon.com";
        public static string WcR2MajorVersion = "v5.7.0.";
        public static string CheckUpdateURL = "https://api.hikaricalyx.com/WcR2-NTMS/v1/GetLatestUpdate";
        private

        /// <summary>
        /// 这是程序入口无雾。
        /// </summary>
        static void StartMainForm()
        {
            //创建主窗体
            var frm = new MainForm();
            //加载插件
            LoadPlugins(frm);
            //加载配置文件并初始化插件
            var cng = Config.ConfigManager.ConfigFile;
            frm.PluginOnLoad();
            PluginManager.PluginOnLoad();
            //走你
            Application.Run(frm);
        }

        static void LoadPlugins(PluginContextProvider provider)
        {
            var asmList = PluginManager.GetPluginFiles().Select(asmFile =>
            {
                try
                {
#if NET6_0_OR_GREATER
                    var ctx = new PluginLoadContext(GetUnmanagedDllDirectory(), asmFile);
                    return ctx.LoadFromAssemblyPath(asmFile);
#else
                    var asmName = AssemblyName.GetAssemblyName(asmFile);
                    return Assembly.Load(asmName);
#endif
                }
                catch (Exception ex)
                {
                    return null;
                }
            }).OfType<Assembly>().ToList();
            loadedPluginAssemblies.AddRange(asmList);

            var context = new PluginContext(provider);
            foreach (var asm in asmList)
            {
                PluginManager.LoadPlugin(asm, context);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                string logFile = Path.Combine(Application.StartupPath, "error.log");
                try
                {
                    string content = DateTime.Now.ToString() + "\r\n" + ex.ToString() + "\r\n";
                    File.AppendAllText(logFile, content);
                }
                catch
                {
                }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (var asm in loadedPluginAssemblies)
            {
                if (asm.FullName == args.Name)
                {
                    return asm;
                }
            }

#if NET6_0_OR_GREATER
            try
            {
                var assemblyName = new AssemblyName(args.Name);
                string assemblyPath = Path.Combine(GetManagedDllDirectory(), assemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                }
            }
            catch
            {
                return null;
            }
#endif
            return null;
        }

        static void SetDllDirectory()
        {
            LibPath = GetUnmanagedDllDirectory();
            SetDllDirectory(LibPath);

            foreach (var dllName in Directory.GetFiles(LibPath, "*.dll"))
            {
                var handle = LoadLibrary(dllName);
            }
        }

        static string GetManagedDllDirectory() => Path.Combine(Application.StartupPath, "Lib");
        static string GetUnmanagedDllDirectory() => Path.Combine(Application.StartupPath, "Lib", GetArchitecture());

        public static string GetArchitecture()
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    return "x86";
                case Architecture.X64:
                    return "x64";
                case Architecture.Arm64:
                    return "ARM64";
                default:
                    return "unknown";

            }
        }

        [DllImport("kernel32.dll")]
        static extern bool SetDllDirectory(string path);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string path);
    }
}
