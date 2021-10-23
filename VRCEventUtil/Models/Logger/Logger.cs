using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VRCEventUtil.Models
{
    static class Logger
    {
        private const string LOG_DIR = "log";

        public static void Log(string msg, DateTime? dateTime = null)
        {
            dateTime ??= DateTime.Now;
            var contents = msg.Split(Environment.NewLine).Select(line => $"{dateTime},{line}");
            try
            {
                Directory.CreateDirectory(LOG_DIR);
                File.AppendAllLines(CreateLogFileName(dateTime), contents, Encoding.UTF8);
            }
            catch (Exception)
            {

            }
        }

        public static void Log(Exception? exception, DateTime? dateTime = null)
        {
            if (exception is null)
            {
                Log("不明なエラーが発生しました．");
                return;
            }

            dateTime ??= DateTime.Now;
            var contents = exception.ToString().Split(Environment.NewLine).Select(line => $"{dateTime},{line}");
            try
            {
                Directory.CreateDirectory(LOG_DIR);
                File.AppendAllLines(CreateLogFileName(dateTime), contents, Encoding.UTF8);
            }
            catch (Exception)
            {

            }
        }

        private static string CreateLogFileName(DateTime? time) => Path.Combine(LOG_DIR, $"{time:yyyyMMdd}.csv");
    }
}
