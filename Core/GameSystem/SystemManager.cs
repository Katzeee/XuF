using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Xuf.Common;

namespace Xuf.Core
{
    public class CSystemManager : CSingleton<CSystemManager>
    {
        public Transform Root { get; set; }
        private List<IGameSystem> m_systems = new List<IGameSystem>();
        private HashSet<IGameSystem> m_disabledSystems = new HashSet<IGameSystem>();

        /// <summary>
        /// Register a game system with priority-based ordering
        /// </summary>
        /// <param name="system">The system to register</param>
        /// <param name="enableByDefault">Whether to enable the system immediately after registration (default: false)</param>
        public void RegisterGameSystem(IGameSystem system, bool enableByDefault = true)
        {
            if (system == null)
            {
                throw new System.ArgumentNullException(nameof(system), "System cannot be null");
            }

            // Check if already registered
            if (m_systems.Contains(system))
            {
                throw new System.InvalidOperationException($"System {system.GetType().Name} is already registered");
            }

            // Insert at the correct position to maintain priority order (higher priority first)
            int insertIndex = 0;
            for (int i = 0; i < m_systems.Count; i++)
            {
                if (system.Priority > m_systems[i].Priority)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = i + 1;
            }

            m_systems.Insert(insertIndex, system);

            // If enableByDefault is true, call OnEnable, otherwise add to disabled systems
            if (enableByDefault)
            {
                system.OnEnable();
            }
            else
            {
                m_disabledSystems.Add(system);
            }
        }

        /// <summary>
        /// Unregister a game system
        /// </summary>
        /// <param name="system">The system to unregister</param>
        public void UnregisterGameSystem(IGameSystem system)
        {
            if (system == null)
            {
                throw new System.ArgumentNullException(nameof(system), "System cannot be null");
            }

            if (!m_systems.Contains(system))
            {
                throw new System.InvalidOperationException($"System {system.GetType().Name} is not registered");
            }

            // Call OnDisable before unregistering
            system.OnDisable();

            m_systems.Remove(system);
            m_disabledSystems.Remove(system); // Remove from disabled set if it was there
        }

        /// <summary>
        /// Unregister a game system by type
        /// </summary>
        /// <typeparam name="T">The system type</typeparam>
        public void UnregisterGameSystem<T>() where T : IGameSystem
        {
            var system = m_systems.OfType<T>().FirstOrDefault();
            if (system != null)
            {
                UnregisterGameSystem(system);
            }
        }

        /// <summary>
        /// Enable a system (call OnEnable)
        /// </summary>
        /// <param name="system">The system to enable</param>
        public void EnableSystem(IGameSystem system)
        {
            if (system == null)
            {
                throw new System.ArgumentNullException(nameof(system), "System cannot be null");
            }

            if (!m_systems.Contains(system))
            {
                throw new System.InvalidOperationException($"System {system.GetType().Name} is not registered");
            }

            if (m_disabledSystems.Contains(system))
            {
                m_disabledSystems.Remove(system);
                system.OnEnable();
            }
        }

        /// <summary>
        /// Enable a system by type
        /// </summary>
        /// <typeparam name="T">The system type</typeparam>
        public void EnableSystem<T>() where T : IGameSystem
        {
            var system = m_systems.OfType<T>().FirstOrDefault();
            if (system != null)
            {
                EnableSystem(system);
            }
        }

        /// <summary>
        /// Disable a system (call OnDisable)
        /// </summary>
        /// <param name="system">The system to disable</param>
        public void DisableSystem(IGameSystem system)
        {
            if (system == null)
            {
                throw new System.ArgumentNullException(nameof(system), "System cannot be null");
            }

            if (!m_systems.Contains(system))
            {
                throw new System.InvalidOperationException($"System {system.GetType().Name} is not registered");
            }

            if (!m_disabledSystems.Contains(system))
            {
                m_disabledSystems.Add(system);
                system.OnDisable();
            }
        }

        /// <summary>
        /// Disable a system by type
        /// </summary>
        /// <typeparam name="T">The system type</typeparam>
        public void DisableSystem<T>() where T : IGameSystem
        {
            var system = m_systems.OfType<T>().FirstOrDefault();
            if (system != null)
            {
                DisableSystem(system);
            }
        }

        /// <summary>
        /// Check if a system is enabled
        /// </summary>
        /// <param name="system">The system to check</param>
        /// <returns>True if system is enabled, false otherwise</returns>
        public bool IsSystemEnabled(IGameSystem system)
        {
            if (system == null)
            {
                return false;
            }

            return m_systems.Contains(system) && !m_disabledSystems.Contains(system);
        }

        /// <summary>
        /// Check if a system of specified type is enabled
        /// </summary>
        /// <typeparam name="T">The system type</typeparam>
        /// <returns>True if system is enabled, false otherwise</returns>
        public bool IsSystemEnabled<T>() where T : IGameSystem
        {
            var system = m_systems.OfType<T>().FirstOrDefault();
            return system != null && IsSystemEnabled(system);
        }

        /// <summary>
        /// Update all registered systems
        /// </summary>
        /// <param name="deltaTime">Frame time</param>
        /// <param name="unscaledDeltaTime">Unscaled frame time</param>
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            foreach (var system in m_systems)
            {
                // Only update enabled systems
                if (!m_disabledSystems.Contains(system))
                {
                    system.Update(deltaTime, unscaledDeltaTime);
                }
            }
        }

        public void FixedUpdate(float deltaTime, float unscaledDeltaTime)
        {
            foreach (var system in m_systems)
            {
                // Only update enabled systems
                if (!m_disabledSystems.Contains(system))
                {
                    system.FixedUpdate(deltaTime, unscaledDeltaTime);
                }
            }
        }

        /// <summary>
        /// Get a system of specified type, throws exception if not found
        /// </summary>
        /// <typeparam name="T">The system type</typeparam>
        /// <returns>The found system instance</returns>
        public T GetSystem<T>(bool throwException = true) where T : IGameSystem
        {
            var system = m_systems.OfType<T>().FirstOrDefault();
            if (system == null)
            {
                if (throwException)
                {
                    throw new System.InvalidOperationException($"System of type {typeof(T).Name} is not registered");
                }
                return default;
            }
            return system;
        }

        /// <summary>
        /// Get a system by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public T GetSystemByName<T>(string name, bool throwException = true) where T : IGameSystem{
            var system = m_systems.FirstOrDefault(s => s.Name == name);
            if (system == null)
            {
                if (throwException)
                {
                    throw new System.InvalidOperationException($"System {name} is not registered");
                }
                return default;
            }
            return (T)system;
        }

        /// <summary>
        /// Check if a system of specified type is registered
        /// </summary>
        /// <typeparam name="T">The system type</typeparam>
        /// <returns>True if system is registered, false otherwise</returns>
        public bool HasSystem<T>() where T : IGameSystem
        {
            return m_systems.OfType<T>().Any();
        }

        /// <summary>
        /// Get all registered systems
        /// </summary>
        /// <returns>A copy of the systems list</returns>
        public List<IGameSystem> GetAllSystems()
        {
            return new List<IGameSystem>(m_systems);
        }

        /// <summary>
        /// Get all enabled systems
        /// </summary>
        /// <returns>A list of enabled systems</returns>
        public List<IGameSystem> GetEnabledSystems()
        {
            return m_systems.Where(system => !m_disabledSystems.Contains(system)).ToList();
        }

        /// <summary>
        /// Get all disabled systems
        /// </summary>
        /// <returns>A list of disabled systems</returns>
        public List<IGameSystem> GetDisabledSystems()
        {
            return m_disabledSystems.ToList();
        }

        /// <summary>
        /// Get the count of registered systems
        /// </summary>
        /// <returns>The number of registered systems</returns>
        public int Count => m_systems.Count;

        /// <summary>
        /// Get the count of enabled systems
        /// </summary>
        /// <returns>The number of enabled systems</returns>
        public int EnabledCount => m_systems.Count - m_disabledSystems.Count;

        /// <summary>
        /// Clear all registered systems
        /// </summary>
        public void ClearAllSystems()
        {
            // Call OnDisable for all systems before clearing
            foreach (var system in m_systems)
            {
                DisableSystem(system);
            }

            m_systems.Clear();
            m_disabledSystems.Clear();
        }

        /// <summary>
        /// Get debug information about all registered systems
        /// </summary>
        /// <returns>Debug information string</returns>
        public string GetDebugInfo()
        {
            var info = $"SystemManager Debug Info:\nTotal Systems: {m_systems.Count}\nEnabled Systems: {EnabledCount}\nDisabled Systems: {m_disabledSystems.Count}\n";
            for (int i = 0; i < m_systems.Count; i++)
            {
                var system = m_systems[i];
                var status = m_disabledSystems.Contains(system) ? "[DISABLED]" : "[ENABLED]";
                info += $"[{i}] {system.GetType().Name} (Priority: {system.Priority}) {status}\n";
            }
            return info;
        }
    }
}
