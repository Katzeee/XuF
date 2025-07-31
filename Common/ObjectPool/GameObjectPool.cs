using System.Collections.Generic;
using UnityEngine;

namespace XuF.Common.ObjectPool
{
    /// <summary>
    /// GameObject object pool class
    /// Supports pooling GameObjects from prefabs with proper lifecycle management
    /// </summary>
    public class GameObjectPool
    {
        private readonly Stack<GameObject> pool;
        private readonly GameObject prefab;
        private readonly Transform parentTransform;
        private readonly int maxSize;
        private readonly string poolName;

        /// <summary>
        /// Total number of GameObjects in pool (including active and available)
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Number of available GameObjects in pool
        /// </summary>
        public int AvailableCount => pool.Count;

        /// <summary>
        /// Number of active GameObjects
        /// </summary>
        public int ActiveCount => TotalCount - AvailableCount;

        /// <summary>
        /// Constructor for GameObject pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate from</param>
        /// <param name="initialSize">Initial pool size</param>
        /// <param name="maxSize">Maximum pool size, 0 means unlimited</param>
        /// <param name="parentTransform">Parent transform for pooled objects</param>
        /// <param name="poolName">Pool name for debugging</param>
        public GameObjectPool(
            GameObject prefab,
            int initialSize = 0,
            int maxSize = 0,
            Transform parentTransform = null,
            string poolName = null)
        {
            if (prefab == null)
            {
                throw new System.ArgumentNullException(nameof(prefab), "Prefab cannot be null");
            }

            this.prefab = prefab;
            this.parentTransform = parentTransform;
            this.maxSize = maxSize;
            this.poolName = poolName ?? prefab.name + "_Pool";

            pool = new Stack<GameObject>();

            // Pre-create initial objects
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        /// <summary>
        /// Get GameObject from pool
        /// </summary>
        /// <returns>Pooled GameObject</returns>
        public GameObject Get()
        {
            GameObject obj;

            if (pool.Count > 0)
            {
                // Get object from pool
                obj = pool.Pop();
            }
            else
            {
                // Pool is empty, create new object
                obj = CreateNewObject();
            }

            // Activate object and reset its state
            obj.SetActive(true);
            ResetObject(obj);

            // Call OnSpawn on all IPoolObject components
            var poolObjects = obj.GetComponentsInChildren<IPoolObject>();
            foreach (var poolObj in poolObjects)
            {
                poolObj.OnSpawn();
            }
            
            // Call OnSpawn on all IPoolGameObject components
            var poolGameObjects = obj.GetComponentsInChildren<IPoolGameObject>();
            foreach (var poolGameObj in poolGameObjects)
            {
                poolGameObj.OnSpawn();
            }

            return obj;
        }

        /// <summary>
        /// Get GameObject from pool at specific position and rotation
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>Pooled GameObject</returns>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        /// <summary>
        /// Get GameObject from pool with component of type T
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Component of type T, or null if not found</returns>
        public T Get<T>() where T : Component
        {
            var obj = Get();
            return obj.GetComponent<T>();
        }

        /// <summary>
        /// Get GameObject from pool with component of type T at specific position and rotation
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>Component of type T, or null if not found</returns>
        public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Get(position, rotation);
            return obj.GetComponent<T>();
        }

        /// <summary>
        /// Release GameObject back to pool
        /// </summary>
        /// <param name="obj">GameObject to release</param>
        public void Release(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"[{poolName}] Attempting to release null GameObject");
                return;
            }

            // Check if this object belongs to this pool (more flexible check)
            if (!obj.name.Contains(prefab.name))
            {
                Debug.LogWarning($"[{poolName}] Attempting to release object that doesn't belong to this pool: {obj.name}");
                return;
            }

            // Check pool size limit
            if (maxSize > 0 && pool.Count >= maxSize)
            {
                // Pool is full, destroy object
                DestroyObject(obj);
                Debug.LogWarning($"[{poolName}] Pool is full, object destroyed. {GetStats()}");
                return;
            }

            // Deactivate object and reset its state
            obj.SetActive(false);
            ResetObject(obj);

            // Set parent transform if specified
            if (parentTransform != null)
            {
                obj.transform.SetParent(parentTransform);
            }

            // Call Reset on all IPoolObject components
            var poolObjects = obj.GetComponentsInChildren<IPoolObject>();
            foreach (var poolObj in poolObjects)
            {
                poolObj.Reset();
            }
            
            // Call Reset on all IPoolGameObject components
            var poolGameObjects = obj.GetComponentsInChildren<IPoolGameObject>();
            foreach (var poolGameObj in poolGameObjects)
            {
                poolGameObj.Reset();
            }

            pool.Push(obj);
        }

        /// <summary>
        /// Release component's GameObject back to pool
        /// </summary>
        /// <param name="component">Component whose GameObject to release</param>
        public void Release(Component component)
        {
            if (component != null)
            {
                Release(component.gameObject);
            }
        }

        /// <summary>
        /// Pre-create GameObjects in pool
        /// </summary>
        /// <param name="count">Number of GameObjects to create</param>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (maxSize > 0 && TotalCount >= maxSize)
                {
                    Debug.LogWarning($"[{poolName}] Cannot prewarm more objects, pool is at max size");
                    break;
                }

                CreateNewObject();
            }
        }

        /// <summary>
        /// Clear all GameObjects in pool
        /// </summary>
        public void Clear()
        {
            // Destroy all objects in pool
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                DestroyObject(obj);
            }

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
        /// Create new GameObject from prefab
        /// </summary>
        /// <returns>Created GameObject</returns>
        private GameObject CreateNewObject()
        {
            var obj = Object.Instantiate(prefab, parentTransform);
            obj.name = prefab.name + "_Pooled_" + TotalCount;
            obj.SetActive(false);
            
            // Call Reset on all IPoolObject components
            var poolObjects = obj.GetComponentsInChildren<IPoolObject>();
            foreach (var poolObj in poolObjects)
            {
                poolObj.Reset();
            }
            
            // Call Reset on all IPoolGameObject components
            var poolGameObjects = obj.GetComponentsInChildren<IPoolGameObject>();
            foreach (var poolGameObj in poolGameObjects)
            {
                poolGameObj.Reset();
            }

            pool.Push(obj);
            TotalCount++;

            return obj;
        }

        /// <summary>
        /// Reset GameObject to initial state
        /// </summary>
        /// <param name="obj">GameObject to reset</param>
        private void ResetObject(GameObject obj)
        {
            // Reset transform to prefab state
            obj.transform.position = prefab.transform.position;
            obj.transform.rotation = prefab.transform.rotation;
            obj.transform.localScale = prefab.transform.localScale;

            // Reset Rigidbody if present
            var rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            var rigidbody2D = obj.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.angularVelocity = 0f;
            }
        }

        /// <summary>
        /// Destroy GameObject and release resources
        /// </summary>
        /// <param name="obj">GameObject to destroy</param>
        private void DestroyObject(GameObject obj)
        {
            if (obj == null) return;

            Object.Destroy(obj);
            TotalCount--;
        }
    }
} 