namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Utilities.Interfaces;
    using ProtoBuf;
    using System.IO;
    using System.Threading.Tasks;

    class ProtofubConverter : ISerializer, IDeserializer
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
            return Task.Run(() => Serializer.Deserialize<T>(GenerateStreamFromString(serializedObj)));
        }

        public Task<T> DeserializeAsync<T>(Stream serializedStream)
        {
            return Task.Run(() =>
            {
                using (serializedStream)
                {
                    return Serializer.Deserialize<T>(serializedStream);
                }
            });
        }

        public Task<string> SerializeAsync<T>(T obj)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            stream.Position = 0;
            using var sr = new StreamReader(stream);
            return sr.ReadToEndAsync();
        }

        public Task SerializeAndSaveAsync<T>(string filePath, T obj)
        {
            return Task.Run(() =>
            {
                using var file = File.Create(filePath);
                Serializer.Serialize(file, obj);
            });
        }
    }
}