using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Common
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

    public class CEventSystem : Singleton<CEventSystem>
    {
        // private Dictionary<EEventId, Action<EventData>> m_events = new();
        private Dictionary<EEventId, EventGroup> m_eventGroups = new();

        public CEventSystem()
        {
            var eventGroup = new EventGroup()
            {
                m_isGroup = true,
            };
            m_eventGroups.Add(EEventId.GRoot, eventGroup);
        }

        public void AddEventListener(EEventId eventId, Action<CEventData> action)
        {
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogError($"Eventgroup {eventId} doesn't exsit!");
                return;
            }
            m_eventGroups[eventId].AddEventListener(action);
        }

        public void RemoveEventListener(EEventId eventId, Action<CEventData> action)
        {
            if (!m_eventGroups.ContainsKey(eventId))
            {
                Debug.LogError($"Eventgroup {eventId} doesn't exsit!");
                return;
            }
            m_eventGroups[eventId].RemoveEventListener(action);
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
    }

}

