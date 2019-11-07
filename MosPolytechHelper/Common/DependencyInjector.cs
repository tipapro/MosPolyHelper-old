namespace MosPolyHelper.Common
{
    using MosPolyHelper.Common.Interfaces;
    using MosPolyHelper.Features.Common;
    using System.IO;

    public static class DependencyInjector
    {
        static object key = new object();
        static LoggerFactory loggerFactory;
        static ProtofubConverter converter;
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
        public static ISerializer GetISerializer() => GetSingleton(ref converter);
        public static IDeserializer GetIDeserializer() => GetSingleton(ref converter);
        public static IMediator<ViewModels, VmMessage> GetIMediator() => GetSingleton(ref mediator);
    }
}