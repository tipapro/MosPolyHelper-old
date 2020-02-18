namespace MosPolyHelper.Utilities.Interfaces
{
    using System.Threading.Tasks;

    public interface ISerializer
    {
        Task<string> SerializeAsync<T>(T obj);
        Task SerializeAndSaveAsync<T>(string filePath, T obj);
    }
}