using System;

namespace Xuf.Core
{
    /// <summary>
    /// Data structure for pending timer operations
    /// </summary>
    internal class PendingTimerData
    {
        public float Interval { get; set; }
        public Action Action { get; set; }
        public bool TimeUnscaled { get; set; }
        public bool PauseOnStart { get; set; }
        public int TimerId { get; set; }
        public object Owner { get; set; }

        public PendingTimerData(float interval, Action action, bool timeUnscaled, bool pauseOnStart, int timerId, object owner)
        {
            Interval = interval;
            Action = action;
            TimeUnscaled = timeUnscaled;
            PauseOnStart = pauseOnStart;
            TimerId = timerId;
            Owner = owner;
        }
    }
}
