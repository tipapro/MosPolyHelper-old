namespace MosPolytechHelper.Common.Interfaces
{
    interface ILoggerFactory
    {
        ILogger Create<T>();
    }
}