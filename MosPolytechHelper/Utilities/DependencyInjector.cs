namespace MosPolyHelper.Utilities
{
    using MosPolyHelper.Features.Common;
    using MosPolyHelper.Utilities.Interfaces;
    using System.IO;

    public static class DependencyInjector
    {
        static object key = new object();
        static LoggerFactory loggerFactory;
        static ProtofubConverter protofubConverter;
        static JsonConverter jsonConverter;
        static Mediator<ViewModels, VmMessage> mediator;


        static T GetSingleton<T>(ref T obj) where T : new()
        {
            if (obj == null)
            {
                obj = new T();
            }
            return obj;
        }

        public static ILoggerFactory GetILoggerFactory(Stream config = null)
        {
            if (loggerFactory == null)
            {
                lock (key)
                {
                    loggerFactory = new LoggerFactory(config);
                }
                return loggerFactory;
            }
            if (config != null)
            {
                loggerFactory.Config(config);
            }
            return loggerFactory;
        }
        public static ISerializer GetProtofubISerializer() => GetSingleton(ref protofubConverter);
        public static IDeserializer GetProtofubIDeserializer() => GetSingleton(ref protofubConverter);
        public static ISerializer GetJsonISerializer() => GetSingleton(ref jsonConverter);
        public static IDeserializer GetJsonIDeserializer() => GetSingleton(ref jsonConverter);
        public static IMediator<ViewModels, VmMessage> GetIMediator() => GetSingleton(ref mediator);
    }
}