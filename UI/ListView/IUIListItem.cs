using UnityEngine;
using UnityEngine.UI;

namespace Xuf.UI
{
    public abstract class IUIListItem : MonoBehaviour
    {
        RectTransform rectTransform;

        public abstract void SetData(object data);

        public virtual Vector2 GetItemSize(object data)
        {
            if (null == rectTransform)
            {
                rectTransform = transform as RectTransform;
            }
            return rectTransform.rect.size;
        }
    }
}