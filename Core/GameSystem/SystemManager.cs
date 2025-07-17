using System.Collections.Generic;
using System.Linq;
using UnityEngine.Android;
using Xuf.Common;

namespace Xuf.Core
{
  class CSystemManager : CSingleton<CSystemManager>
  {
    private List<IGameSystem> m_systems = new List<IGameSystem>();

    /// <summary>
    /// Register a game system with priority-based ordering
    /// </summary>
    /// <param name="system">The system to register</param>
    public void RegisterGameSystem(IGameSystem system)
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

      m_systems.Remove(system);
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
    /// Update all registered systems
    /// </summary>
    /// <param name="deltaTime">Frame time</param>
    /// <param name="unscaledDeltaTime">Unscaled frame time</param>
    public void Update(float deltaTime, float unscaledDeltaTime)
    {
      foreach (var system in m_systems)
      {
        system.Update(deltaTime, unscaledDeltaTime);
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
    /// Get the count of registered systems
    /// </summary>
    /// <returns>The number of registered systems</returns>
    public int Count => m_systems.Count;

    /// <summary>
    /// Clear all registered systems
    /// </summary>
    public void ClearAllSystems()
    {
      m_systems.Clear();
    }

    /// <summary>
    /// Get debug information about all registered systems
    /// </summary>
    /// <returns>Debug information string</returns>
    public string GetDebugInfo()
    {
      var info = $"SystemManager Debug Info:\nTotal Systems: {m_systems.Count}\n";
      for (int i = 0; i < m_systems.Count; i++)
      {
        var system = m_systems[i];
        info += $"[{i}] {system.GetType().Name} (Priority: {system.Priority})\n";
      }
      return info;
    }
  }
}
