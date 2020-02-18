namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Utilities.Interfaces;
    using Newtonsoft.Json;
    using System.IO;
    using System.Threading.Tasks;

    class JsonConverter : ISerializer, IDeserializer
    {
        public Task<T> DeserializeAsync<T>(string serializedObj)
        {
            return Task.Run(() => JsonConvert.DeserializeObject<T>(serializedObj));
        }

        public Task<T> DeserializeAsync<T>(Stream serializedStream)
        {
            return Task.Run(() =>
            {
                using (serializedStream)
                {
                    var serializer = new JsonSerializer();
                    using var sr = new StreamReader(serializedStream);
                    using var jr = new JsonTextReader(sr);
                    return serializer.Deserialize<T>(jr);
                }
            });
        }

        public Task<string> SerializeAsync<T>(T obj)
        {
            return Task.Run(() => JsonConvert.SerializeObject(obj));
        }

        public Task SerializeAndSaveAsync<T>(string filePath, T obj)
        {
            return Task.Run(() =>
            {
                var serializer = new JsonSerializer();
                using var fs = File.Create(filePath);
                using var sw = new StreamWriter(fs);
                using var jw = new JsonTextWriter(sw);
                serializer.Serialize(jw, obj);
            });
        }
    }
}