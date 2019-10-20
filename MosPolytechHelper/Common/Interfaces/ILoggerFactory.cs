namespace MosPolytechHelper.Common.Interfaces
{
    public interface ILoggerFactory
    {
        void CanWriteToFileChanged(bool state, string path);
        ILogger Create<T>();
    }
}