using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.Common
{


    public class CEventSystem<TEventType> where TEventType : struct, Enum
    {
        public class EventData
        {
            public TEventType eventId;
            public Transform from;
            public object customData;
        }

        private Dictionary<TEventType, Action<EventData>> _events = new();
        public void AddEventListener(TEventType eventId, Action<EventData> action)
        {
            if (_events.ContainsKey(eventId))
            {
                _events[eventId] += action;
            }
            else
            {
                _events.Add(eventId, action);
            }

        }

        public void RemoveEvenetListener(TEventType eventId, Action<EventData> action)
        {
            if (!_events.ContainsKey(eventId))
            {
                Debug.LogWarning($"Event {eventId} does not exist.");
                return;
            }
            _events[eventId] -= action;
        }

        public void Broadcast(TEventType eventId, in EventData @event)
        {
            if (!_events.ContainsKey(eventId))
            {
                Debug.LogWarning($"Event {eventId} does not exist.");
                return;
            }
            _events[eventId](@event);
        }

        public void Broadcast(string eventName, in EventData @event)
        {
            if (!Enum.IsDefined(typeof(TEventType), eventName))
            {
                Debug.LogWarning($"No event named {eventName}");
                return;
            }
            var eventId = (TEventType)Enum.Parse(enumType: typeof(TEventType), @eventName, true);
            Broadcast(eventId, @event);
        }
    }

}

