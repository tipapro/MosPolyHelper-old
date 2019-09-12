namespace MosPolytechHelper.Common.Interfaces
{
    using System;

    public interface IMediator<TKey, TMessage>
    {
        void Subscribe(TKey key, Action<TMessage> action);
        void Unsubscribe(TKey key);
        void Send(TKey key, TMessage message);
    }
}