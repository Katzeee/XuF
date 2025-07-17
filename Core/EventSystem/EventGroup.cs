using System;

namespace Xuf.Core.EventSystem
{
    /// <summary>
    /// A EventGroup could be a group contians a lot of children groups,
    /// also can be just a wrapper of an event.
    /// </summary>
    public class EventGroup
    {
        public bool m_isGroup = false;
        public bool m_isEnable = true;
        public EEventId m_eventOrGroup = EEventId.None;
        public EventGroup m_parent;
        private Func<CEventData, bool> handler;

        public void AddEventListener(Func<CEventData, bool> handler)
        {
            this.handler += handler;
        }

        public void RemoveEventListener(Func<CEventData, bool> handler)
        {
            this.handler -= handler;
        }

        public void Broadcast(in CEventData @event)
        {
            if (IsDisabled())
            {
                return;
            }
            bool intercepted = false;
            if (handler != null)
            {
                foreach (Func<CEventData, bool> singleHandler in handler.GetInvocationList())
                {
                    if (singleHandler(@event))
                    {
                        intercepted = true;
                        break;
                    }
                }
            }
            if (!intercepted)
            {
                m_parent?.Broadcast(@event);
            }
        }

        public bool IsDisabled()
        {
            EventGroup parent = this;
            while (parent.m_isEnable)
            {
                parent = parent.m_parent;
                if (parent == null)
                {
                    return false;
                }
            }
            return true;
        }
    }

}

