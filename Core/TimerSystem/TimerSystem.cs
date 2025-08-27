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

        // Pool-based data structures
        private List<Timer> m_timerPool; // Dynamic list of all timers
        private Stack<int> m_availableIndices; // Stack of available timer indices
        private int m_poolSize = 1024; // Initial pool size
        private bool m_paused = false;

        // Buffer lists for pending operations to prevent modification during iteration
        private List<PendingTimerData> m_pendingAddTimers;
        private List<PendingLoopTimerData> m_pendingAddLoopTimers;
        private List<int> m_pendingRemoveTimers;

        public TimerSystem()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            m_timerPool = new List<Timer>(m_poolSize);
            m_availableIndices = new Stack<int>(m_poolSize);

            // Initialize buffer lists
            m_pendingAddTimers = new List<PendingTimerData>();
            m_pendingAddLoopTimers = new List<PendingLoopTimerData>();
            m_pendingRemoveTimers = new List<int>();

            // Initialize all timers as inactive
            for (int i = 0; i < m_poolSize; i++)
            {
                m_timerPool.Add(new Timer(0, 1, null, false, null));
                m_timerPool[i].SetActive(false);
                m_availableIndices.Push(i);
            }
        }

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (m_paused)
                return;

            // Set flag to indicate we're in the update loop
            m_isInUpdateLoop = true;

            // Step 1: Process pending add operations first
            ProcessPendingAddOperations();

            var count = m_timerPool.Count;
            // Step 2: Update all active timers in the pool
            for (int i = 0; i < count; i++)
            {
                var timer = m_timerPool[i];
                if (timer != null && timer.Active)
                {
                    timer.Update(deltaTime, unscaledDeltaTime);

                    // If timer should be cleared, deactivate it and return to available stack
                    if (timer.ShouldClear)
                    {
                        timer.SetActive(false);
                        m_availableIndices.Push(i);
                    }
                }
            }

            // Step 3: Process pending remove operations after timer updates
            ProcessPendingRemoveOperations();

            // Clear the flag
            m_isInUpdateLoop = false;
        }

        private void ProcessPendingAddOperations()
        {
            // Process regular timers
            foreach (var pendingTimer in m_pendingAddTimers)
            {
                var timer = m_timerPool[pendingTimer.TimerId];
                timer.Reset(pendingTimer.Interval, 1, pendingTimer.Action, pendingTimer.TimeUnscaled, pendingTimer.Owner);
                if (pendingTimer.PauseOnStart)
                {
                    timer.Pause();
                }
                timer.SetActive(true);
            }
            m_pendingAddTimers.Clear();

            // Process loop timers
            foreach (var pendingLoopTimer in m_pendingAddLoopTimers)
            {
                var timer = m_timerPool[pendingLoopTimer.TimerId];
                timer.Reset(pendingLoopTimer.Interval, pendingLoopTimer.LoopCount, pendingLoopTimer.Action, pendingLoopTimer.TimeUnscaled, pendingLoopTimer.Owner);
                if (pendingLoopTimer.PauseOnStart)
                {
                    timer.Pause();
                }
                timer.SetActive(true);

                // Execute immediately if requested
                if (pendingLoopTimer.ExecuteImmediately)
                {
                    timer.ExecuteImmediately(ExecuteImmediatelyMode.KeepSchedule);
                }
            }
            m_pendingAddLoopTimers.Clear();
        }

        private void ProcessPendingRemoveOperations()
        {
            foreach (int index in m_pendingRemoveTimers)
            {
                if (index >= 0 && index < m_timerPool.Count)
                {
                    var timer = m_timerPool[index];
                    if (timer != null && timer.Active)
                    {
                        timer.SetActive(false);
                        m_availableIndices.Push(index);
                    }
                }
            }
            m_pendingRemoveTimers.Clear();
        }

        public int AddTimer(object owner, float interval, Action action, bool timeUnscaled = false, bool pauseOnStart = false)
        {
            // Get available index first
            int index = GetAvailableIndex();
            if (index == -1)
            {
                LogUtils.Error("TimerSystem: No available timer slots!");
                return 0;
            }

            // Check if we're currently in the Update loop
            if (m_isInUpdateLoop)
            {
                // Add to pending buffer instead of immediate execution
                m_pendingAddTimers.Add(new PendingTimerData(interval, action, timeUnscaled, pauseOnStart, index, owner));
                return index; // Return the real timer ID
            }

            var timer = m_timerPool[index];
            timer.Reset(interval, 1, action, timeUnscaled, owner);
            if (pauseOnStart)
            {
                timer.Pause();
            }
            timer.SetActive(true);

            return index; // Return the index as the timer identifier
        }

        public int AddLoopTimer(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false, bool pauseOnStart = false, bool executeImmediately = false)
        {
            // Get available index first
            int index = GetAvailableIndex();
            if (index == -1)
            {
                LogUtils.Error("TimerSystem: No available timer slots!");
                return 0;
            }

            // Check if we're currently in the Update loop
            if (m_isInUpdateLoop)
            {
                // Add to pending buffer instead of immediate execution
                m_pendingAddLoopTimers.Add(new PendingLoopTimerData(interval, loopCount, action, timeUnscaled, pauseOnStart, executeImmediately, index, owner));
                return index; // Return the real timer ID
            }

            var timer = m_timerPool[index];
            timer.Reset(interval, loopCount, action, timeUnscaled, owner);
            if (pauseOnStart)
            {
                timer.Pause();
            }
            timer.SetActive(true);

            // Execute immediately if requested
            if (executeImmediately)
            {
                timer.ExecuteImmediately(ExecuteImmediatelyMode.KeepSchedule);
            }

            return index; // Return the index as the timer identifier
        }

        public Timer GetTimerWithoutOwner(int index)
        {
            // Index is the position in the pool
            if (index >= 0 && index < m_timerPool.Count)
            {
                var timer = m_timerPool[index];
                if (timer != null && timer.Active)
                {
                    return timer;
                }
            }
            return null;
        }

        public Timer GetTimer(object owner, int index)
        {
            var timer = GetTimerWithoutOwner(index);
            if (timer != null && timer.Owner == owner)
            {
                return timer;
            }
            return null;
        }

        public void RemoveTimerWithoutOwner(int index)
        {
            // Check if we're currently in the Update loop
            if (m_isInUpdateLoop)
            {
                // Add to pending buffer instead of immediate execution
                m_pendingRemoveTimers.Add(index);
                return;
            }

            // Handle pending timer removal - check if this timer is in pending add lists
            m_pendingAddTimers.RemoveAll(item => item.TimerId == index);
            m_pendingAddLoopTimers.RemoveAll(item => item.TimerId == index);

            var timer = GetTimerWithoutOwner(index);
            if (timer != null)
            {
                timer.SetActive(false);
                m_availableIndices.Push(index);
            }
        }

        public void RemoveTimer(object owner, int index)
        {
            var timer = GetTimer(owner, index);
            if (timer == null) // pending or not owned
            {
                m_pendingAddTimers.RemoveAll(item => item.TimerId == index && item.Owner == owner);
                m_pendingAddLoopTimers.RemoveAll(item => item.TimerId == index && item.Owner == owner);
                return;
            }

            if (m_isInUpdateLoop)
            {
                m_pendingRemoveTimers.Add(index);
                return;
            }

            timer.SetActive(false);
            m_availableIndices.Push(index);
        }

        // Helper method to detect if we're currently in the Update loop
        private bool m_isInUpdateLoop = false;

        public void ClearAllTimers()
        {
            // Deactivate all timers and reset the available indices stack
            m_availableIndices.Clear();
            for (int i = 0; i < m_timerPool.Count; i++)
            {
                if (m_timerPool[i] != null)
                {
                    m_timerPool[i].SetActive(false);
                    m_availableIndices.Push(i);
                }
            }

            // Clear pending operations
            m_pendingAddTimers.Clear();
            m_pendingAddLoopTimers.Clear();
            m_pendingRemoveTimers.Clear();
        }

        public int GetActiveTimerCount()
        {
            int count = 0;
            for (int i = 0; i < m_timerPool.Count; i++)
            {
                if (m_timerPool[i] != null && m_timerPool[i].Active)
                {
                    count++;
                }
            }
            return count;
        }

        public void SetPaused(bool paused)
        {
            m_paused = paused;
        }

        // Get an available index from the stack, expand pool if necessary
        private int GetAvailableIndex()
        {
            if (m_availableIndices.Count > 0)
            {
                return m_availableIndices.Pop();
            }

            // Expand pool if no available indices
            ExpandPool();

            if (m_availableIndices.Count > 0)
            {
                return m_availableIndices.Pop();
            }

            return -1; // No available slots
        }

        // Expand the timer pool
        private void ExpandPool()
        {
            int oldSize = m_timerPool.Count;
            int newSize = oldSize * 2; // Double the pool size

            // Add new timers to the pool
            for (int i = oldSize; i < newSize; i++)
            {
                m_timerPool.Add(new Timer(0, 1, null, false, null));
                m_timerPool[i].SetActive(false);
                m_availableIndices.Push(i);
            }

            LogUtils.Trace($"TimerSystem: Expanded pool to {newSize} timers");
        }

        // Reset accumulated errors for all active timers (useful for debugging)
        public void ResetAllTimerErrors()
        {
            for (int i = 0; i < m_timerPool.Count; i++)
            {
                var timer = m_timerPool[i];
                if (timer != null && timer.Active)
                {
                    timer.ResetError();
                }
            }
        }

        // Get total accumulated error across all active timers (for monitoring)
        public float GetTotalAccumulatedError()
        {
            float totalError = 0f;
            for (int i = 0; i < m_timerPool.Count; i++)
            {
                var timer = m_timerPool[i];
                if (timer != null && timer.Active)
                {
                    totalError += Mathf.Abs(timer.AccumulatedError);
                }
            }
            return totalError;
        }

        public bool ExecuteTimerImmediately(int timerId, ExecuteImmediatelyMode mode = ExecuteImmediatelyMode.KeepSchedule)
        {
            var timer = GetTimerWithoutOwner(timerId);
            if (timer == null)
            {
                LogUtils.Warning($"TimerSystem: Timer with ID {timerId} not found or not active");
                return false;
            }

            timer.ExecuteImmediately(mode);
            return true;
        }

        public bool ExecuteTimerImmediately(object owner, int timerId, ExecuteImmediatelyMode mode = ExecuteImmediatelyMode.KeepSchedule)
        {
            var timer = GetTimer(owner, timerId);
            if (timer == null)
            {
                LogUtils.Warning($"TimerSystem: Timer with ID {timerId} not found, not active, or not owned by the specified owner");
                return false;
            }

            timer.ExecuteImmediately(mode);
            return true;
        }
    }
}