using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common
{
    public class LogHelper
    {
        /// <summary>
        /// 记录日志到本地
        /// </summary>
        /// <param name="content"></param>
        public static void WriteLog(string content)
        {
            var folderPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            var filePath = folderPath + "crash.log";
            File.AppendAllText(filePath, content);
        }
    }
}
