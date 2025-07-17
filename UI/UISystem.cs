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
                Debug.LogError("Fatal: not set UIRoot");
                throw new Exception("");
            }
            _UIRoot.SetParent(gameEntry);
        }

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
                    Debug.LogError("Fatal: not set UIRoot");
                    throw new Exception("");
                }
                return _UIRoot;
            }
            set => _UIRoot = value;
        }

        private void RegisterUIForm<TForm>() where TForm : FormBase
        {
            Type type = typeof(TForm);
            var attrs = (UIPrefab) Attribute.GetCustomAttribute(type, typeof(UIPrefab));
            if (attrs == null)
            {
                Debug.LogError($"Can't get attribute \"UIPrefab\" from {type}");
                return;
            }
            GameObject prefab = Resources.Load<GameObject>(attrs.path);
            if (prefab == null)
            {
                Debug.LogError($"Can't load ui prefab at {attrs.path}");
                return;
            }
            GameObject ui = GameObject.Instantiate(prefab, UIRoot);
            if (!UIForm.TryAdd(type, ui))
            {
                Debug.LogWarning($"UIForm {type} has already be registered");
                return;
            }
            var Form = ui.GetComponent<TForm>();
            if (Form == null)
            {
                Debug.LogWarning($"UIForm {type} has no FormBase attached.");
                return;
            }
            ui.SetActive(false);
        }

        public void RemoveUIForm<TForm>()
        {
            Type type = typeof(UIPrefab);
            if (!UIForm.Remove(type))
            {
                Debug.LogWarning($"No registered UIForm named {type}");
            }
        }

        public void OpenForm<TForm>() where TForm : FormBase
        {
            Type type = typeof(TForm);
            if (!UIForm.ContainsKey(type))
            {
                RegisterUIForm<TForm>();
            }
            UIForm[type].SetActive(true);
            UIForm[type].GetComponent<TForm>().OnActivate();

            var eventData = new CEventData();
            m_eventSystem.Broadcast($"{type.Name}_FormOpen", eventData);
        }

        public void CloseForm<TForm>() where TForm : FormBase
        {
            Type type = typeof(TForm);
            if (!UIForm.ContainsKey(type))
            {
                Debug.LogError($"No UIForm named {type}");
                return;
            }
            UIForm[type].GetComponent<TForm>().OnDeActivate();
            UIForm[type].SetActive(false);

            var eventData = new CEventData();
            m_eventSystem.Broadcast($"{type.Name}_FormClose", eventData);
        }

        public void ToggleForm<TForm>()
        {
            Type type = typeof(TForm);
            if (!UIForm.ContainsKey(type))
            {
                Debug.LogError($"No UIForm named {type}");
                return;
            }
            UIForm[type].SetActive(!UIForm[type].activeSelf);
        }

        public void Broadcast(EEventId eventId, in CEventData data)
        {
            m_eventSystem.Broadcast(eventId, data);
        }
    }
}

