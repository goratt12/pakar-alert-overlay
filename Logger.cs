using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pakar_alert_overlay
{
    internal static class Logger
    {
        private static readonly string LogFilePath;

        static Logger()
        {
            // Get the directory of the executable
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            LogFilePath = Path.Combine(exeDirectory, "log.txt");
        }

        public static void Log(LogLevel level, string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            // Log to the console
            Console.WriteLine(logMessage);

            // Log to the file
            using (StreamWriter writer = File.AppendText(LogFilePath))
            {
                writer.WriteLine(logMessage);
            }
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
