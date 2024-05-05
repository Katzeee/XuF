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
            Dictionary<Type, GameObject> UIPanel = new();
            CEventSystem<TEventType, TEventData> UIEventSystem = new();
            public Transform canvas;

            private void RegisterUIPanel<TPanel>() where TPanel : PanelBase<TEventType, TEventData>
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
                GameObject ui = GameObject.Instantiate(prefab, canvas);
                if (!UIPanel.TryAdd(type, ui))
                {
                    Debug.LogWarning($"UIPanel {type} has already be registered");
                }
                ui.GetComponent<TPanel>().RegisterListeners();
                ui.SetActive(false);
            }

            public void RemoveUIPanel<TPanel>() where TPanel : PanelBase<TEventType, TEventData>
            {
                Type type = typeof(UIPrefab);
                if (!UIPanel.Remove(type))
                {
                    Debug.LogWarning($"No registered UIPanel named {type}");
                }
            }

            public void ActivateUI<TPanel>() where TPanel : PanelBase<TEventType, TEventData>
            {
                Type type = typeof(TPanel);
                if (!UIPanel.ContainsKey(type))
                {
                    RegisterUIPanel<TPanel>();
                }
                UIPanel[type].SetActive(true);
                UIEventSystem.Broadcast($"{type.Name}_PanelOpen", new());
            }

            public void DeActivateUI<TPanel>() where TPanel : PanelBase<TEventType, TEventData>
            {
                Type type = typeof(TPanel);
                if (!UIPanel.ContainsKey(type))
                {
                    Debug.LogError($"No UIPanel named {type}");
                    return;
                }
                UIPanel[type].SetActive(false);
                UIEventSystem.Broadcast($"{type.Name}_PanelClose", new());
            }

            public void ToggleUI<TPanel>() where TPanel : PanelBase<TEventType, TEventData>
            {
                Type type = typeof(TPanel);
                if (!UIPanel.ContainsKey(type))
                {
                    Debug.LogError($"No UIPanel named {type}");
                    return;
                }
                UIPanel[type].SetActive(!UIPanel[type].activeSelf);
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
