using UnityEngine;

namespace XuF.Common.ObjectPool
{
    /// <summary>
    /// Interface for objects that can be managed by object pool
    /// </summary>
    public interface IPoolGameObject
    {
        /// <summary>
        /// Called when object is retrieved from pool
        /// </summary>
        void OnSpawn();
        
        /// <summary>
        /// Reset object to initial state for reuse
        /// Should restore the object to a clean, reusable state
        /// </summary>
        void Reset();
    }
} 