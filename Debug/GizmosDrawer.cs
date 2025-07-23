using System;
using UnityEngine;

namespace XuF.Dbg
{
    public class GizmosDrawer : MonoBehaviour
    {
        public Action OnDrawGizmosCallback { get; set; }

        private void OnDrawGizmos()
        {
            OnDrawGizmosCallback?.Invoke();
        }
    }
}