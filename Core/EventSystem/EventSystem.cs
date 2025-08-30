using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Core
{
    public enum EventExecutionMode
    {
        Immediate,    // Always execute immediately
        Queued,       // Always add to queue
        Throttled     // Apply throttling strategy
    }

    public enum ThrottleStrategy
    {
        AlwaysDrop,   // Always drop events when throttled
        DropOldest,   // Drop oldest events when throttled
        AddToQueue    // Add to queue when throttled
    }

    public struct EventSystemConfig
    {
        public float maxProcessingTimePerFrame;
        public int maxConsecutiveEvents;

        public EventSystemConfig(float maxProcessingTimePerFrame = 2.0f, int maxConsecutiveEvents = 3)
        {
            this.maxProcessingTimePerFrame = maxProcessingTimePerFrame;
            this.maxConsecutiveEvents = maxConsecutiveEvents;
        }
    }

    public struct ThrottleConfig
    {
        public int maxEventsPerTimeWindow;
        public float timeWindowSeconds;
        public ThrottleStrategy strategy;
        public int maxQueuedEvents;

        public ThrottleConfig(int maxEventsPerTimeWindow = 10, float timeWindowSeconds = 1.0f,
                             ThrottleStrategy strategy = ThrottleStrategy.AddToQueue, int maxQueuedEvents = 5)
        {
            this.maxEventsPerTimeWindow = maxEventsPerTimeWindow;
            this.timeWindowSeconds = timeWindowSeconds;
            this.strategy = strategy;
            this.maxQueuedEvents = maxQueuedEvents;
        }
    }

    internal class ThrottleInfo
    {
        public ThrottleConfig config;
        public int eventCount;
        public float windowStartTime;

        public ThrottleInfo(ThrottleConfig config)
        {
            this.config = config;
            this.eventCount = 0;
            this.windowStartTime = 0f;
        }


        public void Reset(float currentTime)
        {
            eventCount = 0;
            windowStartTime = currentTime;
        }

        public bool IsThrottled(float currentTime)
        {
            // Check if time window has expired
            if (currentTime - windowStartTime >= config.timeWindowSeconds)
            {
                Reset(currentTime);
                return false; // Window reset, not throttled
            }

            // Check if event count exceeds limit
            if (eventCount >= config.maxEventsPerTimeWindow)
            {
                return true; // Throttled
            }

            // Update event count and allow execution
            eventCount++;
            return false; // Not throttled
        }
    }

    internal struct QueuedEvent
    {
        public EEventId eventId;
        public Action executeAction;
        public float timestamp;
        public ThrottleStrategy? throttleStrategy;  // Throttle strategy for this event
        public int maxQueuedEvents;                 // Max queued events for DropOldest strategy
    }

    public class CEventSystem : IGameSystem
    {
        public string Name => "EventSystem";
        private Dictionary<EEventId, Delegate> m_eventHandlers = new();
        private LinkedList<QueuedEvent> m_eventQueue = new();
        private float m_frameStartTime;
        private int m_eventsProcessedThisFrame;

        // Configuration field - now using the new config struct
        private readonly EventSystemConfig m_config;

        // Throttling configuration and state (merged)
        private Dictionary<EEventId, ThrottleInfo> m_throttleInfos = new();

        // Fair scheduling to prevent starvation
        private Dictionary<EEventId, int> m_consecutiveExecutionCounts = new();

        public int Priority => 900;

        // Constructor with configurable parameters
        public CEventSystem(EventSystemConfig config = default)
        {
            m_config = config;
        }

        // Alternative constructor for backward compatibility
        public CEventSystem(float maxProcessingTimePerFrame, int maxConsecutiveEvents)
            : this(new EventSystemConfig(maxProcessingTimePerFrame, maxConsecutiveEvents))
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

        public void Publish<T>(EEventId eventId, T data, EventExecutionMode mode = EventExecutionMode.Immediate)
            where T : CEventArgBase
        {
            switch (mode)
            {
                case EventExecutionMode.Immediate:
                    ExecuteEventImmediate(eventId, data);
                    break;

                case EventExecutionMode.Queued:
                    AddToQueue(eventId, data);
                    break;

                case EventExecutionMode.Throttled:
                    HandleThrottledEvent(eventId, data);
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

        private void AddToQueue<T>(EEventId eventId, T data, ThrottleStrategy? throttleStrategy = null, int maxQueuedEvents = 0) where T : CEventArgBase
        {
            var queuedEvent = new QueuedEvent
            {
                eventId = eventId,
                executeAction = () => ExecuteEventImmediate(eventId, data),
                timestamp = Time.time,
                throttleStrategy = throttleStrategy,
                maxQueuedEvents = maxQueuedEvents
            };

            m_eventQueue.AddLast(queuedEvent);
        }

        private bool HasFrameBudget()
        {
            var currentTime = Time.realtimeSinceStartup * 1000f;
            var timeUsed = currentTime - m_frameStartTime;

            return timeUsed < m_config.maxProcessingTimePerFrame;
        }

        private void ProcessEventQueue()
        {
            // Reset consecutive execution counts each frame
            m_consecutiveExecutionCounts.Clear();

            // First pass: Apply DropOldest strategy in a single traversal
            ApplyDropOldestStrategiesInQueue();

            // If no events in queue, we're done
            if (m_eventQueue.Count == 0)
            {
                return;
            }

            // Process events until we run out of frame budget
            LinkedListNode<QueuedEvent> currentNode = null;
            while (m_eventQueue.Count > 0 && HasFrameBudget())
            {
                // Find next event to execute, continuing from where we left off
                var nextEventNode = FindNextExecutableEvent(currentNode);

                // If we couldn't find any executable event, we're done
                if (nextEventNode == null)
                    break;

                // Remember the next node for continuing traversal
                currentNode = nextEventNode.Next;

                // If we've wrapped around to the beginning, we've completed a full pass
                if (currentNode == null && m_eventQueue.Count > 1)
                {
                    currentNode = m_eventQueue.First;

                    // Reset consecutive counts after each complete pass through the queue
                    // This ensures fair distribution across multiple passes
                    LogUtils.Trace("[EventSystem] Completed full queue pass with remaining budget. Resetting consecutive counts.");
                    m_consecutiveExecutionCounts.Clear();
                }

                // Execute the event
                var queuedEvent = nextEventNode.Value;
                m_eventQueue.Remove(nextEventNode);
                try
                {
                    queuedEvent.executeAction();
                    m_eventsProcessedThisFrame++;

                    // Update consecutive execution count
                    if (!m_consecutiveExecutionCounts.ContainsKey(queuedEvent.eventId))
                        m_consecutiveExecutionCounts[queuedEvent.eventId] = 0;
                    m_consecutiveExecutionCounts[queuedEvent.eventId]++;
                    LogUtils.Trace($"[EventSystem] Executed event {queuedEvent.eventId}, consecutive: {m_consecutiveExecutionCounts[queuedEvent.eventId]}");
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

        // Throttling and fair scheduling methods
        private void HandleThrottledEvent<T>(EEventId eventId, T data) where T : CEventArgBase
        {
            // Check if throttling is configured for this event
            if (!m_throttleInfos.TryGetValue(eventId, out var throttleInfo))
            {
                LogUtils.Warning($"[EventSystem] Event {eventId} published with Throttled mode but no throttling configured. Use Immediate or Queued mode instead.");
                ExecuteEventImmediate(eventId, data);
                return;
            }

            // Check if event is throttled
            if (throttleInfo.IsThrottled(Time.time))
            {
                HandleThrottledEventStrategy(eventId, data, throttleInfo.config);
                return;
            }

            // Not throttled, execute immediately
            ExecuteEventImmediate(eventId, data);
        }

        private void HandleThrottledEventStrategy<T>(EEventId eventId, T data, ThrottleConfig config) where T : CEventArgBase
        {
            switch (config.strategy)
            {
                case ThrottleStrategy.AlwaysDrop:
                    LogUtils.Trace($"[EventSystem] Event {eventId} dropped due to AlwaysDrop throttling");
                    break;

                case ThrottleStrategy.DropOldest:
                    // Just add to queue with strategy info, cleanup will happen in Update
                    AddToQueue(eventId, data, ThrottleStrategy.DropOldest, config.maxQueuedEvents);
                    LogUtils.Trace($"[EventSystem] Event {eventId} queued with DropOldest strategy");
                    break;

                case ThrottleStrategy.AddToQueue:
                    AddToQueue(eventId, data);
                    LogUtils.Trace($"[EventSystem] Event {eventId} queued due to throttling");
                    break;
            }
        }

        private void ApplyDropOldestStrategiesInQueue()
        {
            // Group events by eventId and track DropOldest events
            var eventGroups = new Dictionary<EEventId, List<LinkedListNode<QueuedEvent>>>();
            var eventsToRemove = new List<LinkedListNode<QueuedEvent>>();

            var current = m_eventQueue.First;
            while (current != null)
            {
                var queuedEvent = current.Value;

                // Only process DropOldest events
                if (queuedEvent.throttleStrategy == ThrottleStrategy.DropOldest)
                {
                    if (!eventGroups.ContainsKey(queuedEvent.eventId))
                    {
                        eventGroups[queuedEvent.eventId] = new List<LinkedListNode<QueuedEvent>>();
                    }
                    eventGroups[queuedEvent.eventId].Add(current);
                }
                current = current.Next;
            }

            // Apply DropOldest strategy for each event type
            foreach (var kvp in eventGroups)
            {
                var eventId = kvp.Key;
                var eventNodes = kvp.Value;

                if (eventNodes.Count > 0)
                {
                    var maxQueuedEvents = eventNodes[0].Value.maxQueuedEvents;

                    if (eventNodes.Count > maxQueuedEvents)
                    {
                        // Sort by timestamp to identify oldest events
                        eventNodes.Sort((a, b) => a.Value.timestamp.CompareTo(b.Value.timestamp));

                        // Mark oldest events for removal
                        int removeCount = eventNodes.Count - maxQueuedEvents;
                        for (int i = 0; i < removeCount; i++)
                        {
                            eventsToRemove.Add(eventNodes[i]);
                        }
                    }
                }
            }

            // Remove all marked events
            foreach (var nodeToRemove in eventsToRemove)
            {
                m_eventQueue.Remove(nodeToRemove);
                LogUtils.Trace($"[EventSystem] Removed oldest event {nodeToRemove.Value.eventId} (timestamp: {nodeToRemove.Value.timestamp:F3})");
            }

            if (eventsToRemove.Count > 0)
            {
                LogUtils.Trace($"[EventSystem] Applied DropOldest strategy, removed {eventsToRemove.Count} events");
            }
        }

        private LinkedListNode<QueuedEvent> FindNextExecutableEvent(LinkedListNode<QueuedEvent> startNode = null)
        {
            // Start from the provided node or from the beginning if null
            var current = startNode ?? m_eventQueue.First;
            var startingPoint = current;

            // If we're at the end of the queue, wrap around to the beginning
            if (current == null && m_eventQueue.Count > 0)
            {
                current = m_eventQueue.First;
                startingPoint = current;
            }

            // No events in queue
            if (current == null)
                return null;

            // Try to find an event that hasn't reached consecutive limit
            do
            {
                var eventId = current.Value.eventId;

                // Check if this event type has reached consecutive execution limit
                if (!m_consecutiveExecutionCounts.TryGetValue(eventId, out int count) ||
                    count < m_config.maxConsecutiveEvents)
                {
                    // This event can be executed
                    return current;
                }

                // Move to next event, wrap around if needed
                current = current.Next ?? m_eventQueue.First;

                // If we've checked all events and come back to the start, break
            } while (current != startingPoint);

            // If all events have reached consecutive limit, return the starting point
            // This ensures progress is always made
            return startingPoint;
        }

        // Configuration methods
        public void ConfigureEventThrottling(EEventId eventId, ThrottleConfig config)
        {
            m_throttleInfos[eventId] = new ThrottleInfo(config);
            LogUtils.Trace($"[EventSystem] Configured throttling for event {eventId}: strategy={config.strategy}, maxEvents={config.maxEventsPerTimeWindow}, window={config.timeWindowSeconds}s");
        }

        public void RemoveEventThrottling(EEventId eventId)
        {
            if (m_throttleInfos.Remove(eventId))
            {
                LogUtils.Trace($"[EventSystem] Removed throttling configuration for event {eventId}");
            }
        }

        public bool TryGetEventThrottling(EEventId eventId, out ThrottleConfig config)
        {
            if (m_throttleInfos.TryGetValue(eventId, out var throttleInfo))
            {
                config = throttleInfo.config;
                return true;
            }
            config = default;
            return false;
        }
    }
}
