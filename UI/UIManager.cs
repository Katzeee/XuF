using System;
using System.Collections.Generic;
using UnityEngine;
using Xuf.Common;

namespace Xuf.UI
{
    public class CUIManager : Singleton<CUIManager>
    {
        Dictionary<Type, GameObject> UIPanel = new();
        private Transform _UIRoot;
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

        // public CUIManager()
        // {
        //     UIRoot = new GameObject("UIRoot").transform;
        //     UnityEngine.Object.DontDestroyOnLoad(UIRoot.gameObject);
        // }

        private void RegisterUIPanel<TPanel>() where TPanel : PanelBase
        {
            Type type = typeof(TPanel);
            var attrs = (UIPrefab)Attribute.GetCustomAttribute(type, typeof(UIPrefab));
            if (attrs == null)
            {
                Debug.LogError($"Can't get attribute \"UIPrefab\" from {type}");
            }
            GameObject prefab = Resources.Load<GameObject>(attrs.path);
            if (prefab == null)
            {
                Debug.LogError($"Can't load ui prefab at {attrs.path}");
            }
            GameObject ui = GameObject.Instantiate(prefab, UIRoot);
            if (!UIPanel.TryAdd(type, ui))
            {
                Debug.LogWarning($"UIPanel {type} has already be registered");
            }
            ui.GetComponent<TPanel>().RegisterListeners();
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

        public void ActivateUI<TPanel>() where TPanel : PanelBase
        {
            Type type = typeof(TPanel);
            if (!UIPanel.ContainsKey(type))
            {
                RegisterUIPanel<TPanel>();
            }
            UIPanel[type].SetActive(true);
            CEventSystem.Instance.Broadcast($"{type.Name}_PanelOpen", new());
        }

        public void DeActivateUI<TPanel>()
        {
            Type type = typeof(TPanel);
            if (!UIPanel.ContainsKey(type))
            {
                Debug.LogError($"No UIPanel named {type}");
                return;
            }
            UIPanel[type].SetActive(false);
            CEventSystem.Instance.Broadcast($"{type.Name}_PanelClose", new());
        }

        public void ToggleUI<TPanel>()
        {
            Type type = typeof(TPanel);
            if (!UIPanel.ContainsKey(type))
            {
                Debug.LogError($"No UIPanel named {type}");
                return;
            }
            UIPanel[type].SetActive(!UIPanel[type].activeSelf);
        }

        // public void AddEventListener(EEventId eventId, Action<CEventData> action)
        // {
        //     CEventSystem.Instance.AddEventListener(eventId, action);
        // }

        // public void RemoveEvenetListener(EEventId eventId, Action<CEventData> action)
        // {
        //     CEventSystem.Instance.RemoveEventListener(eventId, action);
        // }

        public void Broadcast(EEventId eventId, in CEventData data)
        {
            CEventSystem.Instance.Broadcast(eventId, data);
        }
    }
}

