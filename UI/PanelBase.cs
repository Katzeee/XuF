using UnityEngine;

namespace Xuf
{
    namespace UI
    {
        public class PanelBase : MonoBehaviour
        {
            // protected EventDispatcher dispatcher;
            public void Broadcast(string name)
            {
                UIManager.Instance.Broadcast(name);
            }
        }
    }
}
