using System;
using System.Collections.Generic;
using UnityEngine;
using Xuf.Common;
using Xuf.Core.EventSystem;
using Xuf.Core.GameManager;

namespace Xuf.UI
{
    /// <summary>
    /// UI Manager system, should be registered and managed by GameManager.
    /// </summary>
    public class CUIManager : IGameSystem
    {
        Dictionary<Type, GameObject> UIPanel = new();
        private Transform _UIRoot;
        private CEventSystem m_eventSystem = CGameManager.Instance.GetSystem<CEventSystem>();

        /// <summary>
        /// Priority of the UI system. Lower than event system, higher than most logic systems.
        /// </summary>
        public override int Priority => 500;

        /// <summary>
        /// Update method for the UI system. (Empty for now)
        /// </summary>
        public override void Update(float deltaTime, float unscaledDeltaTime)
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

        private void RegisterUIPanel<TPanel>() where TPanel : PanelBase
        {
            Type type = typeof(TPanel);
            var attrs = (UIPrefab)Attribute.GetCustomAttribute(type, typeof(UIPrefab));
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
            if (!UIPanel.TryAdd(type, ui))
            {
                Debug.LogWarning($"UIPanel {type} has already be registered");
                return;
            }
            var panel = ui.GetComponent<TPanel>();
            if (panel == null)
            {
                Debug.LogWarning($"UIPanel {type} has no PanelBase attached.");
                return;
            }
            ui.SetActive(false);
        }

        public void RemoveUIPanel<TPanel>()
        {
            Type type = typeof(UIPrefab);
            if (!UIPanel.Remove(type))
            {
                Debug.LogWarning($"No registered UIPanel named {type}");
            }
        }

        public void OpenForm<TPanel>() where TPanel : PanelBase
        {
            Type type = typeof(TPanel);
            if (!UIPanel.ContainsKey(type))
            {
                RegisterUIPanel<TPanel>();
            }
            UIPanel[type].SetActive(true);
            UIPanel[type].GetComponent<TPanel>().OnActivate();

            var eventData = new CEventData();
            m_eventSystem.Broadcast($"{type.Name}_PanelOpen", eventData);
        }

        public void CloseForm<TPanel>() where TPanel : PanelBase
        {
            Type type = typeof(TPanel);
            if (!UIPanel.ContainsKey(type))
            {
                Debug.LogError($"No UIPanel named {type}");
                return;
            }
            UIPanel[type].GetComponent<TPanel>().OnDeActivate();
            UIPanel[type].SetActive(false);

            var eventData = new CEventData();
            m_eventSystem.Broadcast($"{type.Name}_PanelClose", eventData);
        }

        public void ToggleForm<TPanel>()
        {
            Type type = typeof(TPanel);
            if (!UIPanel.ContainsKey(type))
            {
                Debug.LogError($"No UIPanel named {type}");
                return;
            }
            UIPanel[type].SetActive(!UIPanel[type].activeSelf);
        }

        public void Broadcast(EEventId eventId, in CEventData data)
        {
            m_eventSystem.Broadcast(eventId, data);
        }
    }
}

