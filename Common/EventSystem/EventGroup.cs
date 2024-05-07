using System;
using System.Collections.Generic;

namespace Xuf.Common
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
        private Action<EventData> action;

        public void AddEventListener(Action<EventData> action)
        {
            this.action += action;
        }

        public void RemoveEventListener(Action<EventData> action)
        {
            this.action -= action;
        }

        public void Broadcast(in EventData @event)
        {
            if (!IsDisabled())
            {
                if (action != null)
                {
                    action(@event);
                }
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
