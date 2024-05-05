using System;
using System.Collections.Generic;
using UnityEngine;
using Xuf.Common;

namespace Xuf
{
    namespace UI
    {
        public class CUIManager<TEventType, TEventData> : Singleton<CUIManager<TEventType, TEventData>>
            where TEventType : struct, Enum
            where TEventData : struct, IEventData
        {
            Dictionary<string, GameObject> UIPanel = new();
            CEventSystem<TEventType, TEventData> UIEventSystem = new();
            public Transform canvas;

            public void RegisterUIPanel(string name, GameObject panel)
            {
                // UIPanel.Add(name, panel);
                if (!UIPanel.TryAdd(name, panel))
                {
                    Debug.LogWarning($"UIPanel {name} has already be registered");
                }
            }

            public void RemoveUIPanel(string name)
            {
                if (!UIPanel.Remove(name))
                {
                    Debug.LogWarning($"No UIPanel named {name}");
                }
            }

            public void ActivateUI<TPanel>()
            {
                Type type = typeof(TPanel);
                var attrs = (UIPrefab)Attribute.GetCustomAttribute(type, typeof(UIPrefab));
                if (!UIPanel.ContainsKey(attrs.name))
                {
                    GameObject uiPrefab = Resources.Load<GameObject>(attrs.path);
                    RegisterUIPanel(attrs.name, uiPrefab);
                    GameObject ui = GameObject.Instantiate(uiPrefab, canvas.transform);
                    return;
                }
                UIPanel[attrs.name].SetActive(true);
            }

            public void DeActivateUI(string name)
            {
                if (!UIPanel.ContainsKey(name))
                {
                    Debug.LogError($"No UIPanel named {name}");
                    return;
                }
                UIPanel[name].SetActive(false);
            }

            public void ToggleUI(string name)
            {
                if (!UIPanel.ContainsKey(name))
                {
                    Debug.LogError($"No UIPanel named {name}");
                    return;
                }
                UIPanel[name].SetActive(!UIPanel[name].activeSelf);
            }

            public void AddEventListener(TEventType @event, Action<TEventData> action)
            {
                UIEventSystem.AddEventListener(@event, action);
            }

            public void RemoveEvenetListener(TEventType @event, Action<TEventData> action)
            {
                UIEventSystem.RemoveEvenetListener(@event, action);
            }

            public void Broadcast(TEventType @event, in TEventData data)
            {
                UIEventSystem.Broadcast(@event, data);
            }


        }
    }
}
