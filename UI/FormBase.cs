using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Xuf.Core;
using System.Reflection;
using System.Collections.Generic;

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
        public static TComp GetOrAddComponent<TComp>(GameObject gameObject)
            where TComp : Component
        {
            var component = gameObject.GetComponent<TComp>();
            if (component == null)
            {
                component = gameObject.AddComponent<TComp>();
            }
            return component;
        }

        protected static void ClearChildrenTemplate(Transform parent, GameObject template)
        {
            ClearChildrenTemplate(parent, new List<GameObject> { template });
        }

        protected static void ClearChildrenTemplate(Transform parent, List<GameObject> templates)
        {
            if (templates == null)
            {
                return;
            }

            foreach (var template in templates)
            {
                template.SetActive(false);
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (!templates.Contains(child.gameObject))
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


        public virtual void OnActivate() { }
        public virtual void OnDeActivate() { }

        /// <summary>
        /// Activates the form, making it ready for interaction.
        /// Base implementation simply calls OnActivate.
        /// </summary>
        public virtual void Activate()
        {
            OnActivate();
        }


        /// <summary>
        /// Deactivates the form, cleaning up any resources.
        /// Base implementation simply calls OnDeActivate.
        /// </summary>
        public virtual void Deactivate()
        {
            OnDeActivate();
        }
    }

    /// <summary>
    /// Form base class for a specific data type and model type.
    /// This class represents the View component in the MVC pattern.
    /// Views are responsible for:
    /// 1. Displaying data from the Model
    /// 2. Handling user input
    /// 3. Updating the Model in response to user actions
    /// </summary>
    public abstract class FormBase<TData, TModel> : FormBase
        where TModel : ModelBase<TData>, new()
    {
        protected TModel Model { get; private set; }

        /// <summary>
        /// Initializes the form with data, creating the appropriate model
        /// The model is automatically created based on the TModel type parameter
        /// </summary>
        internal void Initialize(TData data)
        {
            Model = new TModel();
            Model.InitializeData(data);
            // Notify model that it has been created and bound
            Model.OnModelCreated();
        }

        /// <summary>
        /// Activates the form, subscribes to model updates, and performs initial refresh.
        /// Sealed to prevent further overriding - customize behavior in OnActivate instead.
        /// </summary>
        public override sealed void Activate()
        {
            // Call virtual activation method for derived classes
            base.Activate();

            // Subscribe to model updates
            Model.OnDataChanged += Refresh;

            // Initial refresh with current data
            Refresh(Model.Data);
        }


        /// <summary>
        /// Deactivates the form and cleans up subscriptions.
        /// Sealed to prevent further overriding - customize behavior in OnDeActivate instead.
        /// </summary>
        public override sealed void Deactivate()
        {
            // Unsubscribe from model
            if (Model != null)
            {
                Model.OnDataChanged -= Refresh;
                // Notify model that it is about to be destroyed
                Model.OnModelDestroyed();
            }

            // Clear references before calling base deactivate
            Model = null;

            // Call base deactivation method (which calls OnDeActivate)
            base.Deactivate();
        }

        /// <summary>
        /// Refreshes the UI with the provided data.
        /// </summary>
        public abstract void Refresh(TData data);
    }
}
