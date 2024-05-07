using UnityEngine;
using UnityEngine.EventSystems;
using Xuf.UI;

namespace Xuf.Common
{
    class CEventEmitter : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
    {
        public EEventId eventPointerClick;
        public EEventId eventPointerDown;
        public EEventId eventPointerUp;

        public void OnPointerClick(PointerEventData data)
        {
            var eventData = new EventData()
            {
                eventId = eventPointerClick,
                from = transform
            };
            CUIManager.Instance.Broadcast(eventPointerClick, eventData);
        }

        public void OnPointerDown(PointerEventData data)
        {
            var eventData = new EventData()
            {
                eventId = eventPointerDown,
                from = transform
            };
            CUIManager.Instance.Broadcast(eventPointerDown, eventData);
        }
        public void OnPointerUp(PointerEventData data)
        {
            var eventData = new EventData()
            {
                eventId = eventPointerUp,
                from = transform
            };
            CUIManager.Instance.Broadcast(eventPointerUp, eventData);
        }
    }
}
