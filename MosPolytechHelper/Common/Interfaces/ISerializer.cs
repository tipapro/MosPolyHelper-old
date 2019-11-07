namespace MosPolyHelper.Common.Interfaces
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        void Serialize<T>(string filePath, T obj);
    }
}