using System;
using Xuf.Common;
using UnityEngine.EventSystems;

namespace Xuf.UI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor |
        AttributeTargets.Field | AttributeTargets.Method |
        AttributeTargets.Property)]
    public class UIPrefab : Attribute
    {
        public string path;
        public UIPrefab(string name, string path)
        {
            // this.name = name;
            this.path = path;
        }
    }

    public abstract class PanelBase<TEventType> : UIBehaviour where TEventType : struct, Enum
    {
        static protected CUIManager<TEventType> s_UIManager = CUIManager<TEventType>.Instance;

        abstract public void RegisterListeners();

        public void Broadcast(TEventType @event, in CEventSystem<TEventType>.EventData data)
        {
            s_UIManager.Broadcast(@event, data);
        }

        // use by unity editor button action
        public void Broadcast(string eventName)
        {
            // TODO: select enum from unity inspect window
            var eventId = (TEventType)Enum.Parse(enumType: typeof(TEventType), eventName, true);
            CEventSystem<TEventType>.EventData @event = new()
            {
                eventId = eventId,
                from = transform
            };
            s_UIManager.Broadcast(eventId, new());
        }
    }
}
