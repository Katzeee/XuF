using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Xuf.Core
{
    public class Timer
    {
        public const uint INFINITE_LOOPCOUNT = uint.MaxValue;
        private const float c_epsilon = 0.0001f; // Precision for float comparison
        private const float c_maxCompensation = 0.1f; // Maximum compensation to prevent excessive correction

        private bool m_active = false; // New property for pool-based approach

        private Action m_action = null;

        // Changed from readonly to allow reset for object pooling
        private float m_interval = 0;
        private uint m_loopCount = 1;

        private float m_elapsedTime = 0;
        private float m_currentCycleElapsedTime = 0;
        private uint m_currentLoopCount = 0;
        private bool m_paused = false;
        private bool m_timeUnscaled = false;
        private bool m_isCompleted = false;

        // Compensation mechanism for timing errors
        private float m_accumulatedError = 0f; // Accumulated timing error

        public Timer(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false)
        {
            if (interval < 0)
            {
                Assert.IsTrue(false, "Timer interval is less than 0");
            }

            m_action = action;
            m_interval = Mathf.Max(interval, 0);
            m_loopCount = Math.Max(loopCount, 1);
            m_timeUnscaled = timeUnscaled;
            m_active = true; // Timer is active when created
        }

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (!m_active || m_paused || m_action == null || m_isCompleted)
            {
                return;
            }

            // Timer is done
            if (m_currentLoopCount >= m_loopCount && m_loopCount != INFINITE_LOOPCOUNT)
            {
                m_isCompleted = true;
                return;
            }

            float timeStep = m_timeUnscaled ? unscaledDeltaTime : deltaTime;
            m_elapsedTime += timeStep;

            if (m_loopCount != INFINITE_LOOPCOUNT)
            {
                m_elapsedTime = Mathf.Min(m_elapsedTime, m_interval * m_loopCount);
            }

            // Calculate current cycle time with error compensation
            float expectedCycleTime = m_currentLoopCount * m_interval;
            float actualCycleTime = m_elapsedTime;
            m_currentCycleElapsedTime = Mathf.Min(m_interval, actualCycleTime - expectedCycleTime + m_accumulatedError);

            // Check if we should trigger the action
            if (m_currentCycleElapsedTime >= m_interval)
            {
                // Calculate timing error for compensation
                float expectedTriggerTime = (m_currentLoopCount + 1) * m_interval;
                float actualTriggerTime = m_elapsedTime;
                float timingError = actualTriggerTime - expectedTriggerTime;

                // Accumulate error for compensation (clamped to prevent excessive correction)
                m_accumulatedError += Mathf.Clamp(timingError, -c_maxCompensation, c_maxCompensation);

                m_currentLoopCount++;
                m_currentCycleElapsedTime = 0f;
                
                try
                {
                    m_action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception in timer callback: {e.Message}\n{e.StackTrace}");
                }

                if (m_currentLoopCount >= m_loopCount && m_loopCount != INFINITE_LOOPCOUNT)
                {
                    m_isCompleted = true;
                }
            }
        }

        // New property for pool-based approach
        public bool Active => m_active;
        
        // Set active state (for pool management)
        public void SetActive(bool active) => m_active = active;

        public bool ShouldClear => !m_active || m_action == null || m_isCompleted;

        public float RemainingTime
        {
            get
            {
                if (m_isCompleted)
                    return 0f;

                if (m_loopCount == INFINITE_LOOPCOUNT)
                {
                    return m_interval <= 0f ? 0f : Mathf.Infinity;
                }

                return Mathf.Max(m_loopCount * m_interval - m_elapsedTime, 0f);
            }
        }

        // Check if timer is completed
        public bool IsCompleted => m_isCompleted;

        // Pause/Resume timer
        public void Pause() => m_paused = true;
        public void Resume() => m_paused = false;
        public bool IsPaused => m_paused;

        // Get accumulated timing error (for debugging)
        public float AccumulatedError => m_accumulatedError;

        // Reset accumulated error (useful for debugging or manual correction)
        public void ResetError() => m_accumulatedError = 0f;

        // Reset timer for object pooling
        public void Reset(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false)
        {
            // Allow null action for object pooling (timer will be inactive)
            if (interval < 0)
            {
                Assert.IsTrue(false, "Timer interval is less than 0");
            }

            m_action = action;
            m_interval = Mathf.Max(interval, 0);
            m_loopCount = Math.Max(loopCount, 1);
            m_timeUnscaled = timeUnscaled;
            m_elapsedTime = 0;
            m_currentCycleElapsedTime = 0;
            m_currentLoopCount = 0;
            m_paused = false;
            m_isCompleted = false;
            m_accumulatedError = 0f;
            m_active = action != null; // Set active based on whether action is provided
        }
    }
}