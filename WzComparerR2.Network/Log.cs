using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzComparerR2.Network
{
    public class Log
    {
        public static IList<ILogger> Loggers { get; private set; } = new List<ILogger>();

        public static void Debug(string format, params object[] args)
        {
            Log.Write(LogLevel.Debug, format, args);
        }

        public static void Info(string format, params object[] args)
        {
            Log.Write(LogLevel.Info, format, args);
        }

        public static void Warn(string format, params object[] args)
        {
            Log.Write(LogLevel.Warn, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Log.Write(LogLevel.Error, format, args);
        }

        public static void Think(string format, params object[] args)
        {
            Log.Write(LogLevel.Think, format, args);
        }

        public static void WriteTimeStamp()
        {
            Log.WriteTimeStamp(LogLevel.Info);
        }

        public static void WriteAI(string format, bool isThinking, params object[] args)
        {
            Log.WriteAI(isThinking ? LogLevel.Think : LogLevel.Info, format, args);
        }

        public static void Write(LogLevel logLevel, string format, params object[] args)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    lock (logger)
                    {
                        logger.Write(logLevel, format, args);
                    }
                }
                catch
                {
                }
            }
        }

        public static void WriteTimeStamp(LogLevel logLevel)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    lock (logger)
                    {
                        logger.WriteTimeStamp(logLevel);
                    }
                }
                catch
                {
                }
            }
        }

        public static void WriteAI(LogLevel logLevel, string format, params object[] args)
        {
            foreach (var logger in Loggers)
            {
                try
                {
                    lock (logger)
                    {
                        logger.WriteAI(logLevel, format, args);
                    }
                }
                catch
                {
                }
            }
        }
    }

    public interface ILogger
    {
        void Write(LogLevel logLevel, string format, params object[] args);
        void WriteTimeStamp(LogLevel logLevel);
        void WriteAI(LogLevel logLevel, string format, params object[] args);
    }

    public enum LogLevel
    {
        All = 0,
        Debug,
        Info,
        Warn,
        Error,
        Think,
        None,
    }
}