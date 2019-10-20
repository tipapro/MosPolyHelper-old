//namespace MosPolytechHelper.Common
//{
//    using System.IO;
//    using System.Threading.Tasks;
//    using MosPolytechHelper.Common.Interfaces;
//    using Newtonsoft.Json;

//    class JsonConverter : ISerializer, IDeserializer
//    {
//        public Task<T> DeserializeAsync<T>(string serializedObj) =>
//            Task.Run(() => JsonConvert.DeserializeObject<T>(serializedObj));
//        public Task<T> DeserializeAsync<T>(Stream serializedStream)
//        {
//            return Task.Run(() =>
//            {
//                T res;
//                var serializer = new JsonSerializer();
//                using (var sr = new StreamReader(serializedStream))
//                using (var jsonTextReader = new JsonTextReader(sr))
//                {
//                    res = serializer.Deserialize<T>(jsonTextReader);
//                }
//                return res;
//            });
//        }
//        public string Serialize(object obj) => 
//            JsonConvert.SerializeObject(obj);
//    }
//}