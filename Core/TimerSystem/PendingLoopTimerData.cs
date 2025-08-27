using System;

namespace Xuf.Core
{
    /// <summary>
    /// Data structure for pending loop timer operations
    /// </summary>
    internal class PendingLoopTimerData
    {
        public float Interval { get; set; }
        public uint LoopCount { get; set; }
        public Action Action { get; set; }
        public bool TimeUnscaled { get; set; }
        public bool PauseOnStart { get; set; }
        public bool ExecuteImmediately { get; set; }
        public int TimerId { get; set; }
        public object Owner { get; set; }

        public PendingLoopTimerData(float interval, uint loopCount, Action action, bool timeUnscaled, bool pauseOnStart, bool executeImmediately, int timerId, object owner)
        {
            Interval = interval;
            LoopCount = loopCount;
            Action = action;
            TimeUnscaled = timeUnscaled;
            PauseOnStart = pauseOnStart;
            ExecuteImmediately = executeImmediately;
            TimerId = timerId;
            Owner = owner;
        }
    }
}
