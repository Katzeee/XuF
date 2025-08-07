using System;
using System.Collections.Generic;
using UnityEngine;

namespace XuF.Common
{
    /// <summary>
    /// Generic object pool class
    /// Supports pooling of any type that implements IPoolObject interface
    /// </summary>
    /// <typeparam name="T">Type of pooled object, must implement IPoolObject interface</typeparam>
    public class ObjectPool<T> where T : class, IPoolObject
    {
        private readonly Stack<T> pool;
        private readonly HashSet<T> activeObjects;
        private readonly int maxSize;
        private readonly string poolName;
        private readonly Func<T> factory;

        /// <summary>
        /// Total number of objects in pool (including active and available)
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Number of available objects in pool
        /// </summary>
        public int AvailableCount => pool.Count;

        /// <summary>
        /// Number of active objects
        /// </summary>
        public int ActiveCount => activeObjects.Count;

        /// <summary>
        /// Constructor for object pool with factory method
        /// </summary>
        /// <param name="factory">Factory method to create new objects</param>
        /// <param name="initialSize">Initial pool size</param>
        /// <param name="maxSize">Maximum pool size, 0 means unlimited</param>
        /// <param name="poolName">Pool name for debugging</param>
        public ObjectPool(
            Func<T> factory,
            int initialSize = 0,
            int maxSize = 0,
            string poolName = null)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.maxSize = maxSize;
            this.poolName = poolName ?? typeof(T).Name;

            pool = new Stack<T>();
            activeObjects = new HashSet<T>();
            Prewarm(initialSize);
        }

        /// <summary>
        /// Get object from pool
        /// </summary>
        /// <returns>Pooled object</returns>
        public T Get()
        {
            T obj;

            if (pool.Count > 0)
            {
                // Get object from pool
                obj = pool.Pop();
                
                // Safety check: ensure object is not already active
                if (activeObjects.Contains(obj))
                {
                    Debug.LogError($"[{poolName}] Object retrieved from pool is already active: {obj}");
                    // Try to get another object
                    return Get();
                }
            }
            else
            {
                // Pool is empty, create new object using factory
                obj = factory();
                if (obj == null)
                {
                    Debug.LogError($"[{poolName}] Factory returned null object");
                    return null;
                }
                TotalCount++;
            }

            // Add to active objects tracking
            activeObjects.Add(obj);

            // Reset object state
            obj.Reset();

            // Call object's OnSpawn method
            obj.OnSpawn();

            return obj;
        }

        /// <summary>
        /// Release object back to pool
        /// </summary>
        /// <param name="obj">Object to release</param>
        public void Release(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"[{poolName}] Attempting to release null object");
                return;
            }

            if (pool.Contains(obj))
            {
                Debug.LogWarning($"[{poolName}: Double Free] Attempting to release object that is already in pool: {obj}");
                return;
            }

            // Check if object is actually active (was obtained from this pool)
            if (!activeObjects.Contains(obj))
            {
                Debug.LogWarning($"[{poolName}] Attempting to release object that is not active in this pool");
                return;
            }

            // Remove from active objects tracking
            activeObjects.Remove(obj);

            // Check pool size limit
            if (maxSize > 0 && pool.Count >= maxSize)
            {
                // Pool is full, release object resources and discard
                obj.OnDestroy();
                TotalCount--;
                Debug.LogWarning($"[{poolName}] Pool is full, object discarded. {GetStats()}");
                return;
            }

            // Reset object state for reuse and add to pool
            obj.Reset();
            pool.Push(obj);
        }

        /// <summary>
        /// Force release all active objects back to pool
        /// </summary>
        public void ReleaseAll()
        {
            var activeObjectsCopy = new List<T>(activeObjects);
            foreach (var obj in activeObjectsCopy)
            {
                if (obj != null)
                {
                    Release(obj);
                }
            }
        }

        /// <summary>
        /// Pre-create objects in pool
        /// </summary>
        /// <param name="count">Number of objects to create</param>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (maxSize > 0 && TotalCount >= maxSize)
                {
                    Debug.LogWarning($"[{poolName}] Cannot prewarm more objects, pool is at max size");
                    break;
                }

                var obj = factory();
                if (obj == null)
                {
                    Debug.LogError($"[{poolName}] Factory returned null object during prewarm");
                    continue;
                }
                obj.Reset();
                pool.Push(obj);
                TotalCount++;
            }
        }

        /// <summary>
        /// Clear all objects in pool and destroy all active objects
        /// </summary>
        public void Clear()
        {
            // Release resources for all objects in pool
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                obj.OnDestroy();
            }

            // Destroy all active objects
            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    obj.OnDestroy();
                }
            }
            activeObjects.Clear();

            TotalCount = 0;
        }

        /// <summary>
        /// Get pool statistics
        /// </summary>
        /// <returns>Statistics string</returns>
        public string GetStats()
        {
            return $"[{poolName}] Total: {TotalCount}, Active: {ActiveCount}, Available: {AvailableCount}";
        }

        /// <summary>
        /// Check if pool is empty
        /// </summary>
        /// <returns>Whether pool is empty</returns>
        public bool IsEmpty()
        {
            return pool.Count == 0;
        }

        /// <summary>
        /// Check if pool is full
        /// </summary>
        /// <returns>Whether pool is full</returns>
        public bool IsFull()
        {
            return maxSize > 0 && pool.Count >= maxSize;
        }

        /// <summary>
        /// Check if object is active in this pool
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>Whether object is active in this pool</returns>
        public bool IsActive(T obj)
        {
            return activeObjects.Contains(obj);
        }

        /// <summary>
        /// Get all active objects (read-only)
        /// </summary>
        /// <returns>Read-only collection of active objects</returns>
        public IReadOnlyCollection<T> GetActiveObjects()
        {
            return activeObjects;
        }
    }
}