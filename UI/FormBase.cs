using System;
using UnityEngine;
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

    public abstract class FormBase<T> : UIBehaviour
    {
        public T m_data;

        public abstract void OnActivate();
        public abstract void OnDeActivate();

        public abstract void Refresh(T data);

        protected CEventSystem m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>(throwException: false);

        private Action m_closeCallback;
        internal Action CloseCallback
        {
            get => m_closeCallback;
            set => m_closeCallback = value;
        }

        protected void CloseForm()
        {
            m_closeCallback?.Invoke();
        }
    }
}
