namespace MosPolyHelper.Common
{
    using MosPolyHelper.Common.Interfaces;
    using System;
    using System.Collections.Generic;

    public class Mediator<TKey, TMessage> : IMediator<TKey, TMessage>
    {
        Dictionary<TKey, Action<TMessage>> subscribers;

        public Mediator()
        {
            this.subscribers = new Dictionary<TKey, Action<TMessage>>();
        }

        public void Send(TKey key, TMessage value)
        {
            this.subscribers[key](value);
        }

        public void Subscribe(TKey key, Action<TMessage> action)
        {
            this.subscribers[key] = action;
        }

        public void Unsubscribe(TKey key)
        {
            this.subscribers.Remove(key);
        }
    }
}