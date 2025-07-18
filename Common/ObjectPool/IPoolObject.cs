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
        /// Reset object to initial state, releasing all resources
        /// Object should become an empty shell after reset
        /// </summary>
        void Reset();
    }
} 