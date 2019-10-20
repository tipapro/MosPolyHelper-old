namespace MosPolytechHelper.Common
{
    using System.Buffers.Text;
    using System.IO;
    using System.Threading.Tasks;
    using MosPolytechHelper.Common.Interfaces;
    using ProtoBuf;

    class ProtoConverter : ISerializer, IDeserializer
    {
        MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(s);
            streamWriter.Flush();
            stream.Position = 0;
            return stream;
        }

        public Task<T> DeserializeAsync<T>(string serializedObj)
        {
            return Task.Run(() =>
            {
                var stream = GenerateStreamFromString(serializedObj);
                T res;
                    res = Serializer.Deserialize<T>(stream);
                
                return res;
            });
        }

        public Task<T> DeserializeAsync<T>(Stream serializedStream)
        {
            return Task.Run(() =>
            {
                T res;
                using (serializedStream)
                {
                    res = Serializer.Deserialize<T>(serializedStream);
                }
                return res;
            });
        }

        public string Serialize<T>(T obj)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            stream.Position = 0;
            string res;
            using (var sr = new StreamReader(stream))
            {
                res = sr.ReadToEnd();
            }
            return res;
        }

        public void Serialize<T>(string filePath, T obj)
        {
            using (var file = File.Create(filePath))
            {
                Serializer.Serialize(file, obj);
            }
        }
    }
}