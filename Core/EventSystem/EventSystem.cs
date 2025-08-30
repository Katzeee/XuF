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

    public struct EventSystemConfig
    {
        public float maxProcessingTimePerFrame;
        public int maxEventsPerFrame;

        public EventSystemConfig(float maxProcessingTimePerFrame = 2.0f, int maxEventsPerFrame = 300)
        {
            this.maxProcessingTimePerFrame = maxProcessingTimePerFrame;
            this.maxEventsPerFrame = maxEventsPerFrame;
        }
    }

    internal struct QueuedEvent
    {
        public EEventId eventId;
        public Action executeAction;
        public float timestamp;
    }

    public class CEventSystem : IGameSystem
    {
        public string Name => "EventSystem";
        private Dictionary<EEventId, Delegate> m_eventHandlers = new();
        private Queue<QueuedEvent> m_eventQueue = new();
        private float m_frameStartTime;
        private int m_eventsProcessedThisFrame;

        // Configuration field - now using the new config struct
        private readonly EventSystemConfig m_config;

        public int Priority => 900;

        // Constructor with configurable parameters
        public CEventSystem(EventSystemConfig config = default)
        {
            m_config = config;
        }

        // Alternative constructor for backward compatibility
        public CEventSystem(float maxProcessingTimePerFrame, int maxEventsPerFrame)
            : this(new EventSystemConfig(maxProcessingTimePerFrame, maxEventsPerFrame))
        {
        }

        // Public properties to access current configuration
        public EventSystemConfig Config => m_config;

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
                // Check for duplicate registration
                var invocationList = existing.GetInvocationList();
                foreach (var registeredHandler in invocationList)
                {
                    if (AreHandlersEqual(registeredHandler, handler))
                    {
                        LogUtils.Warning($"[EventSystem] Attempted to register the same handler twice for event {eventId} (type: {typeof(T).Name}). This may indicate a programming error.");
                        return;
                    }
                }

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
                // Check if this specific handler is actually registered using GetInvocationList
                var invocationList = existing.GetInvocationList();
                bool handlerFound = false;

                foreach (var registeredHandler in invocationList)
                {
                    if (AreHandlersEqual(registeredHandler, handler))
                    {
                        handlerFound = true;
                        break;
                    }
                }

                if (!handlerFound)
                {
                    LogUtils.Warning($"[EventSystem] Attempted to unsubscribe handler that was never registered for event {eventId} (type: {typeof(T).Name}). This usually indicates a lambda expression mismatch issue.");
                    return;
                }

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
                executeAction = () => ExecuteEventImmediate(eventId, data),
                timestamp = Time.time
            };

            m_eventQueue.Enqueue(queuedEvent);
        }

        private bool HasFrameBudget()
        {
            var currentTime = Time.realtimeSinceStartup * 1000f;
            var timeUsed = currentTime - m_frameStartTime;

            return timeUsed < m_config.maxProcessingTimePerFrame &&
                   m_eventsProcessedThisFrame < m_config.maxEventsPerFrame;
        }

        private void ProcessEventQueue()
        {
            while (m_eventQueue.Count > 0 && HasFrameBudget())
            {
                var queuedEvent = m_eventQueue.Dequeue();

                try
                {
                    queuedEvent.executeAction();
                    m_eventsProcessedThisFrame++;
                }
                catch (Exception e)
                {
                    LogUtils.Error($"[EventSystem] Error executing queued event {queuedEvent.eventId}: {e.Message}");
                }
            }
        }

        // Debug properties
        public int QueuedEventCount => m_eventQueue.Count;
        public int EventsProcessedThisFrame => m_eventsProcessedThisFrame;

        // Compares two delegate handlers for functional equality beyond reference equality
        private bool AreHandlersEqual(Delegate handler1, Delegate handler2)
        {
            // Quick reference check first (fastest path)
            if (ReferenceEquals(handler1, handler2))
            {
                return true;
            }

            // Check if method signatures match
            if (handler1.Method != handler2.Method)
            {
                return false;
            }

            // For static methods, target is null, so they're equal if Methods match
            if (handler1.Target == null && handler2.Target == null)
            {
                return true;
            }

            // For instance methods, both Method and Target must match
            return ReferenceEquals(handler1.Target, handler2.Target);
        }
    }
}


