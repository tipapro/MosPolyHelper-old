namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;

    public static class DependencyInjector
    {
        static LoggerFactory loggerFactory;
        static JsonConverter jsonConverter;
        static Mediator<ViewModels, VmMessage> mediator;


        static T GetSingleton<T>(ref T obj) where T : new()
        {
            if (obj == null)
                obj = new T();
            return obj;
        }

        public static ILoggerFactory GetILoggerFactory() => GetSingleton(ref loggerFactory);
        public static ISerializer GetISerializer() => GetSingleton(ref jsonConverter);
        public static IDeserializer GetIDeserializer() => GetSingleton(ref jsonConverter);
        public static IMediator<ViewModels, VmMessage> GetIMediator() => GetSingleton(ref mediator);
    }
}