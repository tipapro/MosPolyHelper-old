namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Utilities.Interfaces;
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    class ProtofubConverter : ISerializer, IDeserializer
    {
        MemoryStream GenerateStreamFromString(string s)
        {
            byte[] byteAfter64 = Convert.FromBase64String(s);
            return new MemoryStream(byteAfter64);
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
            return Task.Run(() =>
            {
                var stream = new MemoryStream();
                Serializer.Serialize(stream, obj);
                var str = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
                return str;
            });
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