using System;
using System.Linq;

namespace Xuf.Core
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

        /// <summary>
        /// The handler of the event.
        /// <see langword="return"/> true to intercept the event, false to pass the event to the parent.
        /// </summary>
        private Func<CEventData, bool> m_handler;

        public void AddEventListener(Func<CEventData, bool> handler)
        {
            m_handler += handler;
        }

        public void RemoveEventListener(Func<CEventData, bool> handler)
        {
            m_handler -= handler;
        }

        public void Broadcast(in CEventData @event)
        {
            if (IsDisabled())
            {
                return;
            }
            var intercepted = false;
            if (m_handler != null)
            {
                foreach (Func<CEventData, bool> singleHandler in m_handler.GetInvocationList().Cast<Func<CEventData, bool>>())
                {
                    if (singleHandler(@event))
                    {
                        intercepted = true;
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

