namespace MosPolyHelper.Common
{
    using MosPolyHelper.Common.Interfaces;
    using System;

    class NLogger : ILogger
    {
        NLog.ILogger logger;

        public NLogger(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.logger = NLog.LogManager.GetCurrentClassLogger(type);
        }

        public NLogger(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            this.logger = NLog.LogManager.GetLogger(name);
        }

        public bool IsFatalEnabled => this.logger.IsFatalEnabled;
        public bool IsWarnEnabled => this.logger.IsWarnEnabled;
        public bool IsInfoEnabled => this.logger.IsInfoEnabled;
        public bool IsDebugEnabled => this.logger.IsDebugEnabled;
        public bool IsTraceEnabled => this.logger.IsTraceEnabled;
        public bool IsErrorEnabled => this.logger.IsErrorEnabled;

        public void Debug(string message, params object[] args) =>
            this.logger.Debug(message, args);
        public void Debug(Exception exception, string message = "") =>
            this.logger.Debug(exception, message);
        public void Debug(Exception exception, string message, params object[] args) =>
            this.logger.Debug(exception, message, args);
        public void Error(string message, params object[] args) =>
            this.logger.Error(message, args);
        public void Error(Exception exception, string message = "") =>
            this.logger.Error(exception, message);
        public void Error(Exception exception, string message, params object[] args) =>
            this.logger.Error(exception, message, args);
        public void Fatal(string message, params object[] args) =>
            this.logger.Fatal(message, args);
        public void Fatal(Exception exception, string message = "") =>
            this.logger.Fatal(exception, message);
        public void Fatal(Exception exception, string message, params object[] args) =>
            this.logger.Fatal(exception, message, args);
        public void Info(string message, params object[] args) =>
            this.logger.Info(message, args);
        public void Info(Exception exception, string message = "") =>
            this.logger.Info(exception, message);
        public void Info(Exception exception, string message, params object[] args) =>
            this.logger.Info(exception, message, args);
        public void Trace(string message, params object[] args) =>
            this.logger.Trace(message, args);
        public void Trace(Exception exception, string message = "") =>
            this.logger.Trace(exception, message);
        public void Trace(Exception exception, string message, params object[] args) =>
            this.logger.Trace(exception, message, args);
        public void Warn(string message, params object[] args) =>
            this.logger.Warn(message, args);
        public void Warn(Exception exception, string message = "") =>
            this.logger.Warn(exception, message);
        public void Warn(Exception exception, string message, params object[] args) =>
            this.logger.Warn(exception, message, args);
    }
}