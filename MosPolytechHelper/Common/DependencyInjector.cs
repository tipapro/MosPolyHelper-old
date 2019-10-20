namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;

    public static class DependencyInjector
    {
        static LoggerFactory loggerFactory;
        static ProtoConverter converter;
        static Mediator<ViewModels, VmMessage> mediator;


        static T GetSingleton<T>(ref T obj) where T : new()
        {
            if (obj == null)
                obj = new T();
            return obj;
        }

        public static ILoggerFactory GetILoggerFactory()
        {
            if (loggerFactory == null)
                loggerFactory = new LoggerFactory();
            return loggerFactory;
        }
    public static ISerializer GetISerializer() => GetSingleton(ref converter);
        public static IDeserializer GetIDeserializer() => GetSingleton(ref converter);
        public static IMediator<ViewModels, VmMessage> GetIMediator() => GetSingleton(ref mediator);
    }
}