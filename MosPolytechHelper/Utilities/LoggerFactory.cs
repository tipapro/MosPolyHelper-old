namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    class LoggerFactory : ILoggerFactory
    {
        const int MaxLogFiles = 10;
        ILogger logger;

        void RemoveOldLogs(string[] logFiles, int maxLogsCount)
        {
            //try
            //{
            //    int logToRemoveCount = logFiles.Length - maxLogsCount;
            //    if (logToRemoveCount <= 0)
            //    {
            //        return;
            //    }
            //    var logToRemoveList = new List<string>(logToRemoveCount);
            //    foreach (string file in logFiles)
            //    {
            //        AddToRemoveList(logToRemoveList, file);
            //    }
            //    foreach (string file in logToRemoveList)
            //    {
            //        File.Delete(file);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    this.logger.Error(ex);
            //}
        }

        void AddToRemoveList(List<string> logToRemoveList, string fileName)
        {
            //if (logToRemoveList.Count < logToRemoveList.Capacity)
            //{
            //    logToRemoveList.Add(fileName);
            //}
            //else
            //{
            //    int newestDatePos = 0;
            //    bool needToReplace = false;
            //    var fileDate = File.GetCreationTime(fileName);
            //    if (fileDate < File.GetCreationTime(logToRemoveList[0]))
            //    {
            //        needToReplace = true;
            //    }
            //    for (int i = 1; i < logToRemoveList.Count; i++)
            //    {
            //        var curFileDate = File.GetCreationTime(logToRemoveList[i]);
            //        if (!needToReplace && fileDate < curFileDate)
            //        {
            //            needToReplace = true;
            //        }
            //        if (curFileDate > File.GetCreationTime(logToRemoveList[newestDatePos]))
            //        {
            //            newestDatePos = i;
            //        }
            //    }
            //    if (needToReplace)
            //    {
            //        logToRemoveList[newestDatePos] = fileName;
            //    }
            //}
        }

        public LoggerFactory(Stream config)
        {
            //if (config != null)
            //{
            //    Config(config);
            //}
        }

        public void Config(Stream config)
        {
            //NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(XmlReader.Create(config));
            //this.logger = Create<LoggerFactory>();
            //if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            //    "logs")))
            //{
            //    string[] logs = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            //    "logs"));
            //    if (logs.Length > MaxLogFiles)
            //    {
            //        RemoveOldLogs(logs, MaxLogFiles);
            //    }
            //}
            //NLog.LogManager.Flush();
        }

        public ILogger Create<T>()
        {
            return new NLogger(typeof(T).FullName);
        }
        public ILogger Create(string name)
        {
            return new NLogger(name);
        }

        public static string ReadAllLogs(int count)
        {
            //NLog.LogManager.Flush();
            //if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            //    "logs")))
            //{
            //    string result = string.Empty;
            //    string[] logs = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(
            //        Environment.SpecialFolder.LocalApplicationData), "logs"));
            //    if (count == -1 || logs.Length <= count)
            //    {
            //        foreach (var file in logs)
            //        {
            //            result += File.ReadAllText(file) + "\n";
            //        }
            //    }
            //    else
            //    {
            //        Array.Sort(logs, Comparer<string>.Create((s1, s2) => 
            //        DateTime.Parse(Path.GetFileNameWithoutExtension(s2))
            //        .CompareTo(DateTime.Parse(Path.GetFileNameWithoutExtension(s1)))));
            //        for (int i = 0; i < count; i++)
            //        {
            //            result += File.ReadAllText(logs[i]) + "\n";
            //        }
            //    }
            //    return result;
            //}
            return null;
        }
    }
}