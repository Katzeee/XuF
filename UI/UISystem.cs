using System;
using System.Collections.Generic;
using UnityEngine;
using Xuf.Common;
using Xuf.Core;

namespace Xuf.UI
{
    /// <summary>
    /// UI Manager system, should be registered and managed by GameManager.
    /// </summary>
    public class CUISystem : IGameSystem
    {
        public CUISystem(Transform gameEntry)
        {
            _UIRoot = gameEntry.Find("UIRoot");
            if (_UIRoot == null)
            {
                LogUtils.Error("Fatal: not set UIRoot");
                throw new Exception("");
            }
            _UIRoot.SetParent(gameEntry);
        }

        public string Name => "UISystem";

        Dictionary<Type, GameObject> UIForm = new();
        private Transform _UIRoot;
        private CEventSystem m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>();

        /// <summary>
        /// Priority of the UI system. Lower than event system, higher than most logic systems.
        /// </summary>
        public int Priority => 500;

        /// <summary>
        /// Update method for the UI system. (Empty for now)
        /// </summary>
        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            // UI system update logic (if needed)
        }

        public Transform UIRoot
        {
            get
            {
                if (_UIRoot == null)
                {
                    LogUtils.Error("Fatal: not set UIRoot");
                    throw new Exception("");
                }
                return _UIRoot;
            }
            set => _UIRoot = value;
        }

        private GameObject CreateUIForm<TForm>()

        {
            Type type = typeof(TForm);
            var attrs = (UIPrefab) Attribute.GetCustomAttribute(type, typeof(UIPrefab));
            if (attrs == null)
            {
                LogUtils.Error($"Can't get attribute \"UIPrefab\" from {type}");
                return null;
            }
            GameObject prefab = Resources.Load<GameObject>(attrs.path);
            if (prefab == null)
            {
                LogUtils.Error($"Can't load ui prefab at {attrs.path}");
                return null;
            }
            GameObject ui = GameObject.Instantiate(prefab, UIRoot);
            var Form = ui.GetComponent<TForm>();
            if (Form == null)
            {
                LogUtils.Error($"UIForm {type} has no FormBase attached.");
                GameObject.Destroy(ui);
                return null;
            }
            return ui;
        }

        /// <summary>
        /// Opens a form with initial data
        /// The form will create and manage its own model internally
        /// </summary>
        public void OpenForm<TForm, TData, TModel>(TData data) 
            where TForm : FormBase<TData, TModel>
            where TModel : ModelBase<TData>, new()
        {
            Type type = typeof(TForm);

            // Close existing form of same type if open
            if (UIForm.ContainsKey(type))
            {
                CloseForm<TForm>();
            }

            // Create new form instance
            GameObject ui = CreateUIForm<TForm>();
            if (ui == null) return;

            UIForm[type] = ui;

            var form = ui.GetComponent<TForm>();

            // Set close callback for the form
            form.CloseCallback = () => CloseForm<TForm>();
            
            // Initialize form with data and activate it
            form.Initialize(data);
            form.Activate();
            ui.SetActive(true);

            // Publish open event using the UIPrefab attribute
            var attr = (UIPrefab) Attribute.GetCustomAttribute(type, typeof(UIPrefab));
            if (attr == null)
                throw new Exception($"UIPrefab attribute not found on {type.Name}");
            m_eventSystem.Publish(attr.OpenEventId, new CTransformEventArg { transform = ui.transform });
        }



        /// <summary>
        /// Closes a form by type, properly cleaning up model bindings
        /// </summary>
        public void CloseForm<TForm>() where TForm : FormBase
        {
            Type type = typeof(TForm);
            if (!UIForm.ContainsKey(type))
            {
                LogUtils.Warning($"No UIForm named {type}");
                return;
            }

            GameObject ui = UIForm[type];
            var form = ui.GetComponent<TForm>();

            form.Deactivate();

            // Publish close event using the UIPrefab attribute
            var attr = (UIPrefab) Attribute.GetCustomAttribute(type, typeof(UIPrefab));
            if (attr == null)
                throw new Exception($"UIPrefab attribute not found on {type.Name}");
            m_eventSystem.Publish(attr.CloseEventId, new CTransformEventArg { transform = ui.transform });

            UIForm.Remove(type);
            GameObject.Destroy(ui);
        }

        public void ToggleForm<TForm>() where TForm : FormBase
        {
            Type type = typeof(TForm);
            if (!UIForm.ContainsKey(type))
            {
                LogUtils.Error($"No UIForm named {type}");
                return;
            }
            UIForm[type].SetActive(!UIForm[type].activeSelf);
        }
    }
}
