namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using System;
    class LoggerFactory : ILoggerFactory
    {
        public LoggerFactory()
        {
            //var config = new NLog.Config.LoggingConfiguration();

            //// Targets where to log to: File and Console
            //var logFile = new NLog.Targets.FileTarget("logfile")
            //{
            //    // TODO: Place path to the dedicated class
            //    FileName = $"storage\\emulated\\0\\Download\\log2233.txt",
            //    Layout = "${longdate} ${level} ${message}  ${exception}"
            //};

            //// Rules for mapping loggers to targets            
            //config.AddRuleForAllLevels(logFile);

            //// Apply config           
            //NLog.LogManager.Configuration = config;
        }

        public ILogger Create<T>()
        {
            return new NLogger(typeof(T));
        }
    }
}