using UnityEngine;

namespace XuF.Common.ObjectPool
{
    /// <summary>
    /// Interface for objects that can be managed by object pool
    /// </summary>
    public interface IPoolObject
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

        /// <summary>
        /// Release all resources held by this object
        /// Called when object is permanently discarded from pool
        /// Should clean up event listeners, references, etc.
        /// </summary>
        void ReleaseResources();
    }
} 