namespace MosPolyHelper.Common.Interfaces
{
    using System;

    public interface ILogger
    {
        bool IsFatalEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsTraceEnabled { get; }
        bool IsErrorEnabled { get; }

        void Debug(string message, params object[] args);
        void Debug(Exception exception, string message = "");
        void Debug(Exception exception, string message, params object[] args);
        void Error(string message, params object[] args);
        void Error(Exception exception, string message = "");
        void Error(Exception exception, string message, params object[] args);
        void Fatal(string message, params object[] args);
        void Fatal(Exception exception, string message = "");
        void Fatal(Exception exception, string message, params object[] args);
        void Info(string message, params object[] args);
        void Info(Exception exception, string message = "");
        void Info(Exception exception, string message, params object[] args);
        void Trace(string message, params object[] args);
        void Trace(Exception exception, string message = "");
        void Trace(Exception exception, string message, params object[] args);
        void Warn(string message, params object[] args);
        void Warn(Exception exception, string message = "");
        void Warn(Exception exception, string message, params object[] args);
    }
}