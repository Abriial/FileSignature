using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSignature
{
    class Logger
    {
        //Лог файл
        private string logName;
        private string logPath = @"Logs\";

        public Logger()
        {
            logPath += DateTime.Now.ToShortDateString();
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            logName = logPath + "\\" + DateTime.Now.ToShortTimeString().Replace(":", "-") + "_log.csv";
        }

        public void WriteToLog(string error, string source)
        {
            File.AppendAllTextAsync(logName, DateTime.Now + ";\nError:" +  error + ";\nSource:" + source + "\n\n");
        }
    }
}
