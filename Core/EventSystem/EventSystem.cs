using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Core
{
    public class CEventData
    {
        public EEventId EventId { get; internal set; }
        public Transform from;
        private object customData;
        public object CustomData
        {
            get
            {
                if (customData == null)
                {
                    Debug.LogError("Custom data is null");
                }
                return customData;
            }
            set => customData = value;
        }
    }

    public class CEventSystem : IGameSystem
    {
        // private Dictionary<EEventId, Action<EventData>> m_events = new();
        private Dictionary<EEventId, EventGroup> m_eventGroups = new();

        public int Priority => 900;

        public void Update(float deltaTime, float unscaledDeltaTime) { }
        public CEventSystem()
        {
            var eventGroup = new EventGroup()
            {
                m_isGroup = true,
            };
            m_eventGroups.Add(EEventId.GRoot, eventGroup);
            // Automatically register all events and their hierarchy
            AutoRegisterAllEvents();
        }

        public void AddEventListener(EEventId eventId, Func<CEventData, bool> handler)
        {
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogError($"Eventgroup {eventId} doesn't exsit!");
                return;
            }
            m_eventGroups[eventId].AddEventListener(handler);
        }

        public void RemoveEventListener(EEventId eventId, Func<CEventData, bool> handler)
        {
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogError($"Eventgroup {eventId} doesn't exsit!");
                return;
            }
            m_eventGroups[eventId].RemoveEventListener(handler);
        }

        public EventGroup TryAddGroupUnderGroup(EEventId childGroupId, EEventId parentGroupId = EEventId.None)
        {
            var eventGroup = new EventGroup()
            {
                m_isGroup = true,
                m_parent = m_eventGroups[EEventId.GRoot],
                m_eventOrGroup = childGroupId,
            };
            // Add under root
            if (parentGroupId == EEventId.None)
            {
                // already has this group
                if (!m_eventGroups.TryAdd(childGroupId, eventGroup))
                {
                    Debug.LogWarning($"EventGroup {childGroupId} already exists!");
                    return null;
                }
                Debug.Log($"EventGroup {childGroupId} registered under {EEventId.GRoot}!");
                return eventGroup;
            }
            // can't find parent
            if (!m_eventGroups.ContainsKey(parentGroupId))
            {
                Debug.LogError($"EventGroup {parentGroupId} doesn't exsit!");
                return null;
            }
            Debug.Log($"EventGroup {childGroupId} registered under {parentGroupId}!");
            eventGroup.m_parent = m_eventGroups[parentGroupId];
            return eventGroup;

        }

        public EventGroup TryAddEventUnderGroup(EEventId eventId, EEventId groupId = EEventId.GRoot)
        {
            if (!m_eventGroups.ContainsKey(groupId))
            {
                return null;
            }
            var eventGroup = new EventGroup()
            {
                m_parent = m_eventGroups[groupId],
                m_eventOrGroup = eventId,
            };
            if (!m_eventGroups.TryAdd(eventId, eventGroup))
            {
                Debug.LogWarning($"EventGroup {eventId} already exists!");
                return null;
            }
            Debug.Log($"EventGroup {eventId} registered under {groupId}!");
            return eventGroup;
        }

        public void Broadcast(EEventId eventId, in CEventData @event)
        {
            if (eventId == EEventId.None)
            {
                return;
            }
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogWarning($"Event {eventId} does not exist.");
                return;
            }
            @event.EventId = eventId;
            m_eventGroups[eventId].Broadcast(@event);
        }

        public void Broadcast(string eventName, in CEventData @event)
        {
            if (!Enum.IsDefined(typeof(EEventId), eventName))
            {
                Debug.LogWarning($"No event named {eventName}");
                return;
            }
            var eventId = (EEventId)Enum.Parse(enumType: typeof(EEventId), @eventName, true);
            Broadcast(eventId, @event);
        }

        public void Disable(EEventId eventId)
        {
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogWarning($"Event {eventId} does not exist.");
                return;
            }
            m_eventGroups[eventId].m_isEnable = false;
        }

        public void Enable(EEventId eventId)
        {
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogWarning($"Event {eventId} does not exist.");
                return;
            }
            m_eventGroups[eventId].m_isEnable = true;
        }

        // Automatically register all events and their hierarchy
        private void AutoRegisterAllEvents()
        {
            var eventType = typeof(EEventId);
            var names = Enum.GetNames(eventType);
            foreach (var name in names)
            {
                if (name == nameof(EEventId.None) || name == nameof(EEventId.GRoot))
                    continue;
                // Split by '_', build hierarchy for any event with underscores
                var parts = name.Split('_');
                if (parts.Length < 2)
                {
                    // No hierarchy, register directly under GRoot as event
                    var eventId = (EEventId)Enum.Parse(eventType, name);
                    if (!m_eventGroups.ContainsKey(eventId))
                    {
                        TryAddEventUnderGroup(eventId, EEventId.GRoot);
                    }
                    continue;
                }
                string groupPrefix = parts[0];
                EEventId parentGroupId = EEventId.GRoot;
                // Register first group if not exists
                if (Enum.IsDefined(eventType, groupPrefix))
                {
                    var groupId = (EEventId)Enum.Parse(eventType, groupPrefix);
                    if (!m_eventGroups.ContainsKey(groupId))
                    {
                        TryAddGroupUnderGroup(groupId, EEventId.GRoot);
                    }
                    parentGroupId = groupId;
                }
                // Register subgroups if any
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    groupPrefix += "_" + parts[i];
                    if (!Enum.IsDefined(eventType, groupPrefix))
                        continue;
                    var groupId = (EEventId)Enum.Parse(eventType, groupPrefix);
                    if (!m_eventGroups.ContainsKey(groupId))
                    {
                        TryAddGroupUnderGroup(groupId, parentGroupId);
                    }
                    parentGroupId = groupId;
                }
                // Register the event under the last group
                var eventIdFinal = (EEventId)Enum.Parse(eventType, name);
                if (!m_eventGroups.ContainsKey(eventIdFinal))
                {
                    TryAddEventUnderGroup(eventIdFinal, parentGroupId);
                }
            }
        }
    }

}


