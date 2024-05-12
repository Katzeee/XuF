using UnityEngine;
namespace Xuf.Dbg
{
    class CSceneDebugUtils
    {
        public static void DrawRect(Vector3 topLeft, Vector3 bottomRight)
        {
            Gizmos.DrawLine(topLeft, new Vector3(bottomRight.x, topLeft.y, bottomRight.z)); // top
            Gizmos.DrawLine(topLeft, new Vector3(topLeft.x, bottomRight.y, topLeft.z)); // left
            Gizmos.DrawLine(bottomRight, new Vector3(bottomRight.x, topLeft.y, bottomRight.z)); // right
            Gizmos.DrawLine(bottomRight, new Vector3(topLeft.x, bottomRight.y, topLeft.z)); // bottom
        }
    }
}
