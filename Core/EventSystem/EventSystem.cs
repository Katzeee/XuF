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
            {
                m_eventHandlers[eventId] = Delegate.Combine(existing, handler);
                Debug.Log($"[EventSystem] Added handler for event {eventId} (type: {typeof(T).Name}), total handlers: {m_eventHandlers[eventId].GetInvocationList().Length}");
            }
            else
            {
                m_eventHandlers[eventId] = handler;
                Debug.Log($"[EventSystem] Registered new event {eventId} (type: {typeof(T).Name})");
            }
        }

        // Type-safe unsubscription
        public void Unsubscribe<T>(EEventId eventId, Action<T> handler) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var existing))
            {
                var newHandler = Delegate.Remove(existing, handler);
                if (newHandler == null)
                {
                    m_eventHandlers.Remove(eventId);
                    Debug.Log($"[EventSystem] Removed last handler for event {eventId} (type: {typeof(T).Name}), event unregistered");
                }
                else
                {
                    m_eventHandlers[eventId] = newHandler;
                    Debug.Log($"[EventSystem] Removed handler for event {eventId} (type: {typeof(T).Name}), remaining handlers: {newHandler.GetInvocationList().Length}");
                }
            }
            else
            {
                Debug.LogWarning($"[EventSystem] Attempted to unsubscribe from non-existent event {eventId} (type: {typeof(T).Name})");
            }
        }

        // Type-safe publish
        public void Publish<T>(EEventId eventId, T data) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var handler))
            {
                if (handler is Action<T> action)
                {
                    action(data);
                }
                else
                {
                    // Get the expected type from the delegate
                    var expectedType = handler.Method.GetParameters().Length > 0 
                        ? handler.Method.GetParameters()[0].ParameterType.Name 
                        : "Unknown";
                    Debug.LogError($"[EventSystem] Type mismatch when publishing event {eventId}. Expected: {expectedType}, Got: {typeof(T).Name}");
                }
            }
        }
    }
}


