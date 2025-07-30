using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Xuf.Core
{
    class Timer
    {
        public const uint INFINITE_LOOPCOUNT = uint.MaxValue;
        private const float c_epsilon = 0.0001f; // Precision for float comparison
        private const float c_maxCompensation = 0.1f; // Maximum compensation to prevent excessive correction

        private WeakReference m_owner = null;
        private Action m_action = null;

        private readonly float ro_interval = 0;
        private readonly uint ro_loopCount = 1;

        private float m_elapsedTime = 0;
        private float m_currentCycleElapsedTime = 0;
        private uint m_currentLoopCount = 0;
        private bool m_paused = false;
        private readonly bool m_timeUnscaled = false;
        private bool m_isCompleted = false;
        
        // Compensation mechanism for timing errors
        private float m_accumulatedError = 0f; // Accumulated timing error

        public Timer(object owner, float interval, uint loopCount, Action action, bool timeUnscaled = false)
        {
            if (owner == null || action == null)
            {
                Assert.IsTrue(false, "Timer owner or action is null");
            }
            if (interval < 0)
            {
                Assert.IsTrue(false, "Timer interval is less than 0");
            }

            m_owner = new WeakReference(owner);
            m_action = action;
            ro_interval = Mathf.Max(interval, 0);
            ro_loopCount = Math.Max(loopCount, 1);
            m_timeUnscaled = timeUnscaled;
        }

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            if (m_paused || !IsValid || m_action == null || m_isCompleted)
            {
                return;
            }

            // Timer is done
            if (m_currentLoopCount >= ro_loopCount && ro_loopCount != INFINITE_LOOPCOUNT)
            {
                m_isCompleted = true;
                return;
            }

            float timeStep = m_timeUnscaled ? unscaledDeltaTime : deltaTime;
            m_elapsedTime += timeStep;
            
            if (ro_loopCount != INFINITE_LOOPCOUNT)
            {
                m_elapsedTime = Mathf.Min(m_elapsedTime, ro_interval * ro_loopCount);
            }

            // Calculate current cycle time with error compensation
            float expectedCycleTime = m_currentLoopCount * ro_interval;
            float actualCycleTime = m_elapsedTime;
            m_currentCycleElapsedTime = Mathf.Min(ro_interval, actualCycleTime - expectedCycleTime + m_accumulatedError);

            // Check if we should trigger the action
            if (m_currentCycleElapsedTime >= ro_interval)
            {
                // Calculate timing error for compensation
                float expectedTriggerTime = (m_currentLoopCount + 1) * ro_interval;
                float actualTriggerTime = m_elapsedTime;
                float timingError = actualTriggerTime - expectedTriggerTime;
                
                // Accumulate error for compensation (clamped to prevent excessive correction)
                m_accumulatedError += Mathf.Clamp(timingError, -c_maxCompensation, c_maxCompensation);
                
                m_currentLoopCount++;
                m_currentCycleElapsedTime = 0f;
                m_action?.Invoke();

                if (m_currentLoopCount >= ro_loopCount && ro_loopCount != INFINITE_LOOPCOUNT)
                {
                    m_isCompleted = true;
                }
            }
        }

        public int Id => GetHashCode();

        public bool ShouldClear => m_action == null || m_isCompleted || !IsValid;

        public float RemainingTime
        {
            get
            {
                if (m_isCompleted)
                    return 0f;

                if (ro_loopCount == INFINITE_LOOPCOUNT)
                {
                    return ro_interval <= 0f ? 0f : Mathf.Infinity;
                }

                return Mathf.Max(ro_loopCount * ro_interval - m_elapsedTime, 0f);
            }
        }

        private bool IsValid { get { return m_owner.Target is UnityEngine.Object obj ? obj : m_owner.Target != null; } }

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
    }
}