namespace MosPolyHelper.Utilities.Interfaces
{
    public interface ILoggerFactory
    {
        ILogger Create<T>();
        ILogger Create(string name);
    }
}