namespace MosPolyHelper.Common.Interfaces
{
    public interface ILoggerFactory
    {
        ILogger Create<T>();
        ILogger Create(string name);
    }
}