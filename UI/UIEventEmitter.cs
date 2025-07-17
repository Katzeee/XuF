using UnityEngine;
using UnityEngine.EventSystems;
using Xuf.UI;

namespace Xuf.Common
{
    // TODO: let the event with custom data
    class CUIEventEmitter : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
    {
        public EEventId eventPointerClick;
        public ScriptableObject dataPointerClick;
        public EEventId eventPointerDown;
        public EEventId eventPointerUp;

        public void OnPointerClick(PointerEventData data)
        {
            var eventData = new CEventData()
            {
                from = transform,
                CustomData = dataPointerClick,
            };
            CEventSystem.Instance.Broadcast(eventPointerClick, eventData);
        }

        public void OnPointerDown(PointerEventData data)
        {
            var eventData = new CEventData()
            {
                from = transform
            };
            CEventSystem.Instance.Broadcast(eventPointerDown, eventData);
        }
        public void OnPointerUp(PointerEventData data)
        {
            var eventData = new CEventData()
            {
                from = transform
            };
            CEventSystem.Instance.Broadcast(eventPointerUp, eventData);
        }

        // For Animation Broadcast
        public void Broadcast(EEventId @event)
        {
            var eventData = new CEventData()
            {
                from = transform,
            };
            CEventSystem.Instance.Broadcast(@event, eventData);
        }
    }
}
