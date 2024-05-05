using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf
{
    namespace Common
    {

        public interface IEventData { }

        public class CEventSystem<TEventType, TEventData>
            where TEventType : struct, Enum
            where TEventData : struct, IEventData
        {
            private Dictionary<TEventType, Action<TEventData>> _events = new();
            public void AddEventListener(TEventType @event, Action<TEventData> action)
            {
                if (_events.ContainsKey(@event))
                {
                    _events[@event] += action;
                }
                else
                {
                    _events.Add(@event, action);
                }

            }

            public void RemoveEvenetListener(TEventType @event, Action<TEventData> action)
            {
                if (!_events.ContainsKey(@event))
                {
                    Debug.LogWarning($"Event {@event} does not exist.");
                    return;
                }
                _events[@event] -= action;
            }

            public void Broadcast(TEventType @event, in TEventData data)
            {
                if (!_events.ContainsKey(@event))
                {
                    Debug.LogWarning($"Event {@event} does not exist.");
                    return;
                }
                _events[@event](data);
            }
        }

    }

}
