using System;
using System.Collections.Generic;
using UnityEngine;

namespace XuF.Common.ObjectPool
{
    /// <summary>
    /// Generic object pool class
    /// Supports pooling of any type that implements IPoolObject interface
    /// </summary>
    /// <typeparam name="T">Type of pooled object, must implement IPoolObject interface</typeparam>
    public class ObjectPool<T> where T : class, IPoolObject, new()
    {
        private readonly Stack<T> pool;
        private readonly int maxSize;
        private readonly string poolName;

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
        public int ActiveCount => TotalCount - AvailableCount;

        /// <summary>
        /// Constructor for object pool
        /// </summary>
        /// <param name="initialSize">Initial pool size</param>
        /// <param name="maxSize">Maximum pool size, 0 means unlimited</param>
        /// <param name="poolName">Pool name for debugging</param>
        public ObjectPool(
            int initialSize = 0,
            int maxSize = 0,
            string poolName = null)
        {
            this.maxSize = maxSize;
            this.poolName = poolName ?? typeof(T).Name;

            pool = new Stack<T>();

            // Pre-create initial objects
            for (int i = 0; i < initialSize; i++)
            {
                var obj = new T();
                obj.Reset();
                pool.Push(obj);
                TotalCount++;
            }
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
            }
            else
            {
                // Pool is empty, create new object
                obj = new T();
                TotalCount++;
            }

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

            // Check pool size limit
            if (maxSize > 0 && pool.Count >= maxSize)
            {
                // Pool is full, just return without adding to pool
                TotalCount--;
                return;
            }

            // Call object's Reset method
            obj.Reset();

            // Add to pool
            pool.Push(obj);
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

                var obj = new T();
                obj.Reset();
                pool.Push(obj);
                TotalCount++;
            }
        }

        /// <summary>
        /// Clear all objects in pool
        /// </summary>
        public void Clear()
        {
            pool.Clear();
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
    }
}