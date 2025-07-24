using System;
using UnityEngine.EventSystems;
using Xuf.Core;
using System.Reflection;

namespace Xuf.UI
{
    /// <summary>
    /// Attribute for marking FormBase subclasses with prefab path and open/close event ids.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor |
        AttributeTargets.Field | AttributeTargets.Method |
        AttributeTargets.Property)]
    public class UIPrefab : Attribute
    {
        public string path;
        public EEventId OpenEventId { get; }
        public EEventId CloseEventId { get; }
        public UIPrefab(string path, EEventId openEventId, EEventId closeEventId)
        {
            this.path = path;
            OpenEventId = openEventId;
            CloseEventId = closeEventId;
        }
    }

    public abstract class FormBase : UIBehaviour
    {
        abstract public void OnActivate();
        abstract public void OnDeActivate();

        protected CEventSystem m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>(throwException: false);
    }
}
