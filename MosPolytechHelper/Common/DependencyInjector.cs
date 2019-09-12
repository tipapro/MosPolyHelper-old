namespace MosPolytechHelper.Common
{
    using MosPolytechHelper.Common.Interfaces;
    using MosPolytechHelper.Features.Common;

    public class DependencyInjector
    {
        JsonConverter jsonConverter;
        Mediator<ViewModels, VmMessage> mediator;

        DependencyInjector()
        { }

        T GetSingleton<T>(ref T obj) where T : new()
        {
            if (obj == null)
                obj = new T();
            return obj;
        }

        public ILoggerFactory GetILoggerFactory() => new LoggerFactory();
        public ISerializer GetISerializer() => GetSingleton(ref this.jsonConverter);
        public IDeserializer GetIDeserializer() => GetSingleton(ref this.jsonConverter);
        public IMediator<ViewModels, VmMessage> GetIMediator() => GetSingleton(ref this.mediator);

        public static void SetDiInstance(IMain mainClass)
        {
            mainClass.DependencyInjector = new DependencyInjector();
        }
    }

    public interface IMain
    {
        DependencyInjector DependencyInjector { get; set; }
    }
}