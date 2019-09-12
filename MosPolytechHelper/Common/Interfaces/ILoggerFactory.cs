namespace MosPolytechHelper.Common.Interfaces
{
    public interface ILoggerFactory
    {
        ILogger Create<T>();
    }
}