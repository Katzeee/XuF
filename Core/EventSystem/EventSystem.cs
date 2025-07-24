using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Core
{
    public class CEventSystem : IGameSystem
    {
        private Dictionary<EEventId, Delegate> m_eventHandlers = new();

        public int Priority => 900;

        public void Update(float deltaTime, float unscaledDeltaTime) { }

        // Type-safe subscription
        // https://stackoverflow.com/questions/6229131/why-cant-c-sharp-infer-type-from-this-seemingly-simple-obvious-case
        public void Subscribe<T>(EEventId eventId, Action<T> handler) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var existing))
                m_eventHandlers[eventId] = Delegate.Combine(existing, handler);
            else
                m_eventHandlers[eventId] = handler;
        }

        // Type-safe unsubscription
        public void Unsubscribe<T>(EEventId eventId, Action<T> handler) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var existing))
            {
                var newHandler = Delegate.Remove(existing, handler);
                if (newHandler == null)
                    m_eventHandlers.Remove(eventId);
                else
                    m_eventHandlers[eventId] = newHandler;
            }
        }

        // Type-safe publish
        public void Publish<T>(EEventId eventId, T data) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var handler) && handler is Action<T> action)
                action(data);
        }
    }
}


