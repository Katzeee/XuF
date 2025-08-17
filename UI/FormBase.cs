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

    public abstract class FormBase : UIBehaviour
    {
        //SECTION: Utility Functions
        protected static TComp GetOrAddComponent<TComp>(GameObject gameObject)
            where TComp : Component
        {
            var component = gameObject.GetComponent<TComp>();
            if (component == null)
            {
                component = gameObject.AddComponent<TComp>();
            }
            return component;
        }

        protected static void ClearTemplateChildren(Transform parent, GameObject template)
        {
            template.SetActive(false);
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.gameObject != template)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        protected static void SetActiveAllChildren(Transform parent, bool active, bool recursive = false)
        {
            if (parent == null) return;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.gameObject.SetActive(active);

                if (recursive)
                {
                    SetActiveAllChildren(child, active, true);
                }
            }
        }
    }

    public abstract class FormBase<T> : FormBase
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
