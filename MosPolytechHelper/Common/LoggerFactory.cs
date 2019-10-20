namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using System;
    using System.IO;

    class LoggerFactory : ILoggerFactory
    {
        public LoggerFactory()
        {
            //NLog.LogManager.ThrowExceptions = true;
            var config = new NLog.Config.LoggingConfiguration();


            var logMemory = new NLog.Targets.MemoryTarget("logmemory")
            {
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}"
            };
            config.AddRuleForAllLevels(logMemory, "logToMemory");
            NLog.LogManager.Configuration = config;
        }

        public void CanWriteToFileChanged(bool state, string path)
        {
            if (state)
            {
                path = Path.Combine(path, $"log{DateTime.Now}.txt");
                string log = string.Concat(NLog.LogManager.Configuration.FindTargetByName<NLog.Targets.MemoryTarget>("logmemory").Logs);
                if (!string.IsNullOrEmpty(log))
                {
                    File.WriteAllText(path, log);
                }
                var logFile = new NLog.Targets.FileTarget("logfile")
                {
                    FileName = path,
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
                    MaxArchiveFiles = 10
                };
                NLog.LogManager.Configuration.AddRuleForAllLevels(logFile, "logToFile");
            }
            else
            {
                NLog.LogManager.Configuration.RemoveRuleByName("logToFile");
            }
        }

        public ILogger Create<T>()
        {
            return new NLogger(typeof(T));
        }
    }
}