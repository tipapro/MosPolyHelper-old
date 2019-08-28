namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;

    static class DependencyInjector
    {
        static JsonConverter jsonConverter;

        static T GetSingleton<T>(ref T obj) where T : new()
        {
            if (obj == null)
                obj = new T();
            return obj;
        }

        public static ILoggerFactory GetILoggerFactory() => new LoggerFactory();
        public static ISerializer GetISerializer() => GetSingleton(ref jsonConverter);
        public static IDeserializer GetIDeserializer() => GetSingleton(ref jsonConverter);
    }
}