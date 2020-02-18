using System.IO;
using System.Threading.Tasks;

namespace MosPolyHelper.Utilities.Interfaces
{
    public interface IDeserializer
    {
        Task<T> DeserializeAsync<T>(string serializedObj);
        Task<T> DeserializeAsync<T>(Stream serializedStream);
    }
}