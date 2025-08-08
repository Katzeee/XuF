using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Xuf.Core
{
    public class TimerSystem : IGameSystem
    {
        public string Name => "TimerSystem";
        public int Priority => 100;

        // Optimized data structures
        private List<Timer> m_activeTimers = new(128);
        private List<Timer> m_timerPool = new(64);
        private HashSet<int> m_activeTimerIds = new(128);
        private bool m_paused = false;
        
        // Batch operation buffers
        private List<Timer> m_timersToRemove = new(32);
        private List<Timer> m_timersToAdd = new(32);

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (m_paused)
                return;

            // Process batch additions
            if (m_timersToAdd.Count > 0)
            {
                foreach (var timer in m_timersToAdd)
                {
                    if (timer != null && !timer.ShouldClear && !m_activeTimerIds.Contains(timer.Id))
                    {
                        m_activeTimers.Add(timer);
                        m_activeTimerIds.Add(timer.Id);
                    }
                }
                m_timersToAdd.Clear();
            }

            // Update active timers and collect completed ones
            for (int i = m_activeTimers.Count - 1; i >= 0; --i)
            {
                var timer = m_activeTimers[i];
                if (timer == null) continue;

                timer.Update(deltaTime, unscaledDeltaTime);
                
                if (timer.ShouldClear)
                {
                    m_timersToRemove.Add(timer);
                    m_activeTimers.RemoveAt(i);
                }
            }

            // Process batch removals
            if (m_timersToRemove.Count > 0)
            {
                foreach (var timer in m_timersToRemove)
                {
                    if (timer != null)
                    {
                        m_activeTimerIds.Remove(timer.Id);
                        ReturnTimerToPool(timer);
                    }
                }
                m_timersToRemove.Clear();
            }
        }

        public int AddTimer(Timer timer)
        {
            if (timer == null)
            {
                Assert.IsTrue(false, "Timer is null");
                return 0;
            }
            
            // Don't add timers with null actions (inactive timers)
            if (timer.ShouldClear)
            {
                return 0;
            }
            
            // Remove existing timer with same ID if exists
            RemoveTimer(timer.Id);
            
            // Add to batch buffer for next Update
            m_timersToAdd.Add(timer);
            return timer.Id;
        }

        public int AddTimer(object owner, float interval, Action action, bool timeUnscaled = false)
        {
            var timer = GetTimerFromPool(owner, interval, 1, action, timeUnscaled);
            AddTimer(timer);
            return timer.Id;
        }

        public int AddLoopTimer(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false)
        {
            var timer = GetTimerFromPool(owner, interval, loopCount, action, timeUnscaled);
            AddTimer(timer);
            return timer.Id;
        }

        public Timer GetTimer(int id)
        {
            if (!m_activeTimerIds.Contains(id))
                return null;
                
            // Linear search is acceptable for small numbers of timers
            // For larger numbers, consider using a separate Dictionary for lookups
            for (int i = 0; i < m_activeTimers.Count; ++i)
            {
                if (m_activeTimers[i].Id == id)
                    return m_activeTimers[i];
            }
            return null;
        }

        public void RemoveTimer(Timer timer)
        {
            if (timer != null)
            {
                RemoveTimer(timer.Id);
            }
        }

        public void RemoveTimer(int id)
        {
            if (!m_activeTimerIds.Contains(id))
                return;
                
            for (int i = 0; i < m_activeTimers.Count; ++i)
            {
                if (m_activeTimers[i].Id == id)
                {
                    var timer = m_activeTimers[i];
                    m_activeTimers.RemoveAt(i);
                    m_activeTimerIds.Remove(id);
                    ReturnTimerToPool(timer);
                    break;
                }
            }
        }

        public void ClearAllTimers()
        {
            // Return all timers to pool
            foreach (var timer in m_activeTimers)
            {
                if (timer != null)
                {
                    ReturnTimerToPool(timer);
                }
            }
            
            m_activeTimers.Clear();
            m_activeTimerIds.Clear();
            m_timersToAdd.Clear();
            m_timersToRemove.Clear();
        }

        public int GetActiveTimerCount()
        {
            return m_activeTimers.Count;
        }

        public void SetPaused(bool paused)
        {
            m_paused = paused;
        }

        // Object pooling methods
        private Timer GetTimerFromPool(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false)
        {
            Timer timer;
            if (m_timerPool.Count > 0)
            {
                timer = m_timerPool[m_timerPool.Count - 1];
                m_timerPool.RemoveAt(m_timerPool.Count - 1);
                timer.Reset(owner, interval, loopCount, action, timeUnscaled);
            }
            else
            {
                timer = new Timer(owner, interval, loopCount, action, timeUnscaled);
            }
            return timer;
        }

        private void ReturnTimerToPool(Timer timer)
        {
            if (timer != null && m_timerPool.Count < 256) // Limit pool size
            {
                // Reset with null action to mark as inactive
                timer.Reset(null, 0, 1, null, false);
                m_timerPool.Add(timer);
            }
        }

        // Reset accumulated errors for all timers (useful for debugging)
        public void ResetAllTimerErrors()
        {
            foreach (var timer in m_activeTimers)
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
            foreach (var timer in m_activeTimers)
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

            foreach (var timer in m_activeTimers)
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