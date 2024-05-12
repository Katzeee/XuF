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

    public abstract class PanelBase : UIBehaviour
    {
        abstract public void RegisterListeners();

        public void Broadcast(EEventId eventId, CEventData data)
        {
            CUIManager.Instance.Broadcast(eventId, data);
        }

        // use by unity editor button action
        public void Broadcast(string eventName)
        {
            // TODO: select enum from unity inspect window
            var eventId = (EEventId)Enum.Parse(enumType: typeof(EEventId), eventName, true);
            var @event = new CEventData()
            {
                EventId = eventId,
                from = transform
            };
            CUIManager.Instance.Broadcast(eventId, new());
        }
    }
}
