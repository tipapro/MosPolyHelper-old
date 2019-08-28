namespace MosPolytechHelper.Common.Interfaces
{
    interface IDeserializer
    {
        T Deserialize<T>(string serializedObj);
    }
}