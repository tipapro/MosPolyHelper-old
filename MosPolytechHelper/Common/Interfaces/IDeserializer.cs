namespace MosPolytechHelper.Common.Interfaces
{
    public interface IDeserializer
    {
        T Deserialize<T>(string serializedObj);
    }
}