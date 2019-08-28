namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using Newtonsoft.Json;

    class JsonConverter : ISerializer, IDeserializer
    {
        public T Deserialize<T>(string serializedObj) =>
            JsonConvert.DeserializeObject<T>(serializedObj);
        public string Serialize(object obj) => 
            JsonConvert.SerializeObject(obj);
    }
}