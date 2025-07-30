using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Xuf.Core
{
    class TimerSystem : IGameSystem
    {

        public string Name => "TimerSystem";

        public int Priority => 100;

        private Dictionary<int, Timer> m_timers = new();
        private bool m_paused = false;
        private List<int> m_cachedKeys = new(100);


        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (m_paused)
                return;

            int count = -1;
            foreach (int id in m_timers.Keys)
            {
                if (++count < m_cachedKeys.Count)
                    m_cachedKeys[count] = id;
                else
                    m_cachedKeys.Add(id);
            }

            for (int i = 0; i <= count; ++i)
            {
                var id = m_cachedKeys[i];
                if (m_timers.TryGetValue(id, out var timer))
                {
                    timer.Update(deltaTime, unscaledDeltaTime);
                    if (timer.ShouldClear)
                        m_timers.Remove(id);
                }
            }
        }

        public int AddTimer(Timer timer)
        {
            if (timer == null)
            {
                Assert.IsTrue(false, "Timer is null");
                return 0;
            }
            m_timers.Remove(timer.Id);
            m_timers.Add(timer.Id, timer);
            return timer.Id;
        }

        public int AddTimer(object owner, float interval, Action action, bool timeUnscaled = false)
        {
            var timer = new Timer(owner, interval, 1, action, timeUnscaled);
            AddTimer(timer);
            return timer.Id;
        }

        public int AddLoopTimer(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false)
        {
            var timer = new Timer(owner, interval, loopCount, action, timeUnscaled);
            AddTimer(timer);
            return timer.Id;
        }

        public Timer GetTimer(int id)
        {
            if (m_timers.TryGetValue(id, out var timer))
                return timer;
            return null;
        }

        public void RemoveTimer(Timer timer)
        {
            if (timer != null)
            {
                m_timers.Remove(timer.Id);
            }
        }

        public void RemoveTimer(int id)
        {
            m_timers.Remove(id);
        }

        public void ClearAllTimers()
        {
            m_timers.Clear();
        }

        public int GetActiveTimerCount()
        {
            return m_timers.Count;
        }

        public void SetPaused(bool paused)
        {
            m_paused = paused;
        }

        // Reset accumulated errors for all timers (useful for debugging)
        public void ResetAllTimerErrors()
        {
            foreach (var timer in m_timers.Values)
            {
                if (timer != null)
                {
                    timer.ResetError();
                }
            }
        }

        // Get total accumulated error across all timers (for monitoring)
        public float GetTotalAccumulatedError()
        {
            float totalError = 0f;
            foreach (var timer in m_timers.Values)
            {
                if (timer != null)
                {
                    totalError += Mathf.Abs(timer.AccumulatedError);
                }
            }
            return totalError;
        }

        // Get timer statistics for debugging
        public (int activeCount, int completedCount, float totalError) GetTimerStatistics()
        {
            int activeCount = 0;
            int completedCount = 0;
            float totalError = 0f;

            foreach (var timer in m_timers.Values)
            {
                if (timer != null)
                {
                    if (timer.IsCompleted)
                        completedCount++;
                    else
                        activeCount++;

                    totalError += Mathf.Abs(timer.AccumulatedError);
                }
            }

            return (activeCount, completedCount, totalError);
        }
    }
}