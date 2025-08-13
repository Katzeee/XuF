using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Core
{
    public enum EventExecutionMode
    {
        Immediate,    // Always execute immediately
        Queued,       // Always add to queue
        Smart         // Use frame budget to decide
    }

    internal struct QueuedEvent
    {
        public EEventId eventId;
        public CEventArgBase eventArg;
        public float timestamp;
    }

    public class CEventSystem : IGameSystem
    {
        public string Name => "EventSystem";
        private Dictionary<EEventId, Delegate> m_eventHandlers = new();
        private Queue<QueuedEvent> m_eventQueue = new();
        private float m_frameStartTime;
        private int m_eventsProcessedThisFrame;

        // Configuration fields
        public float maxProcessingTimePerFrame = 2.0f; // milliseconds
        public int maxEventsPerFrame = 15;

        public int Priority => 900;

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            m_frameStartTime = Time.realtimeSinceStartup * 1000f;
            m_eventsProcessedThisFrame = 0;

            ProcessEventQueue();
        }

        // Type-safe subscription
        // https://stackoverflow.com/questions/6229131/why-cant-c-sharp-infer-type-from-this-seemingly-simple-obvious-case
        public void Subscribe<T>(EEventId eventId, Action<T> handler) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var existing))
            {
                m_eventHandlers[eventId] = Delegate.Combine(existing, handler);
                LogUtils.Trace($"[EventSystem] Added handler for event {eventId} (type: {typeof(T).Name}), total handlers: {m_eventHandlers[eventId].GetInvocationList().Length}");
            }
            else
            {
                m_eventHandlers[eventId] = handler;
                LogUtils.Trace($"[EventSystem] Registered new event {eventId} (type: {typeof(T).Name})");
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
                    LogUtils.Trace($"[EventSystem] Removed last handler for event {eventId} (type: {typeof(T).Name}), event unregistered");
                }
                else
                {
                    m_eventHandlers[eventId] = newHandler;
                    LogUtils.Trace($"[EventSystem] Removed handler for event {eventId} (type: {typeof(T).Name}), remaining handlers: {newHandler.GetInvocationList().Length}");
                }
            }
            else
            {
                LogUtils.Warning($"[EventSystem] Attempted to unsubscribe from non-existent event {eventId} (type: {typeof(T).Name})");
            }
        }

        public void Publish<T>(EEventId eventId, T data, EventExecutionMode mode = EventExecutionMode.Smart) where T : CEventArgBase
        {
            switch (mode)
            {
                case EventExecutionMode.Immediate:
                    ExecuteEventImmediate(eventId, data);
                    break;

                case EventExecutionMode.Queued:
                    AddToQueue(eventId, data);
                    break;

                case EventExecutionMode.Smart:
                    if (HasFrameBudget())
                    {
                        ExecuteEventImmediate(eventId, data);
                    }
                    else
                    {
                        AddToQueue(eventId, data);
                    }
                    break;
            }
        }

        public void PublishNow<T>(EEventId eventId, T data) where T : CEventArgBase
        {
            ExecuteEventImmediate(eventId, data);
        }

        private void ExecuteEventImmediate<T>(EEventId eventId, T data) where T : CEventArgBase
        {
            if (m_eventHandlers.TryGetValue(eventId, out var handler))
            {
                if (handler is Action<T> action)
                {
                    action(data);
                    m_eventsProcessedThisFrame++;
                }
                else
                {
                    var expectedType = handler.Method.GetParameters().Length > 0
                        ? handler.Method.GetParameters()[0].ParameterType.Name
                        : "Unknown";
                    LogUtils.Error($"[EventSystem] Type mismatch when publishing event {eventId}. Expected: {expectedType}, Got: {typeof(T).Name}");
                }
            }
        }

        private void AddToQueue<T>(EEventId eventId, T data) where T : CEventArgBase
        {
            var queuedEvent = new QueuedEvent
            {
                eventId = eventId,
                eventArg = data,
                timestamp = Time.time
            };

            m_eventQueue.Enqueue(queuedEvent);
        }

        private bool HasFrameBudget()
        {
            var currentTime = Time.realtimeSinceStartup * 1000f;
            var timeUsed = currentTime - m_frameStartTime;

            return timeUsed < maxProcessingTimePerFrame &&
                   m_eventsProcessedThisFrame < maxEventsPerFrame;
        }

        private void ProcessEventQueue()
        {
            while (m_eventQueue.Count > 0 && HasFrameBudget())
            {
                var queuedEvent = m_eventQueue.Dequeue();

                if (m_eventHandlers.TryGetValue(queuedEvent.eventId, out var handler))
                {
                    try
                    {
                        handler.DynamicInvoke(queuedEvent.eventArg);
                        m_eventsProcessedThisFrame++;
                    }
                    catch (Exception e)
                    {
                        LogUtils.Error($"[EventSystem] Error executing queued event {queuedEvent.eventId}: {e.Message}");
                    }
                }
            }
        }

        // Debug properties
        public int QueuedEventCount => m_eventQueue.Count;
        public int EventsProcessedThisFrame => m_eventsProcessedThisFrame;
    }
}


