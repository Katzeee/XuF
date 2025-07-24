using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Xuf.Common;
using Xuf.Core;

namespace Xuf.UI
{
    public class CUIEventEmitter : MonoBehaviour,
        IPointerClickHandler, IPointerUpHandler, IPointerDownHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IScrollHandler,
        ISelectHandler, IDeselectHandler,
        ISubmitHandler, ICancelHandler
    {
        // List of all UI event configs
        public List<CUIEventConfig> eventConfigs = new();

        // Cache the event system reference for better performance
        private CEventSystem m_eventSystem;

        private void Awake()
        {
            // Get the event system reference
            m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>();
            if (m_eventSystem == null)
            {
                Debug.LogError("Failed to get EventSystem from GameManager. Make sure it's registered.");
            }

            // Automatically subscribe to common component events if they exist
            var slider = GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            var toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }

            var inputField = GetComponent<InputField>();
            if (inputField != null)
            {
                inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
                inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            }
        }

        private void OnDestroy()
        {
            // Automatically unsubscribe to prevent memory leaks
            var slider = GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }

            var toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            var inputField = GetComponent<InputField>();
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
                inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
            }
        }

        // Helper: Broadcast event by type
        private void BroadcastByType(EUIEventType type, Transform from, CEventArgBase eventArg = null)
        {
            if (m_eventSystem == null)
            {
                m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>();
                if (m_eventSystem == null)
                {
                    Debug.LogError("EventSystem not found in GameManager");
                    return;
                }
            }

            foreach (var config in eventConfigs)
            {
                if (config.eventType == type)
                {
                    var arg = eventArg ?? config.eventArg ?? new CTransformEventArg { value = from };
                    PublishWithGenericType(config.eventId, arg);
                }
            }
        }

        // Helper method to publish with correct generic type
        private void PublishWithGenericType(EEventId eventId, CEventArgBase arg)
        {
            var argType = arg.GetType();

            // Use reflection to call the generic Publish method with the correct type
            var method = typeof(CEventSystem).GetMethod("Publish");
            var genericMethod = method.MakeGenericMethod(argType);
            genericMethod.Invoke(m_eventSystem, new object[] { eventId, arg });
        }

        public void OnPointerClick(PointerEventData data)
        {
            BroadcastByType(EUIEventType.PointerClick, transform);
        }
        public void OnPointerDown(PointerEventData data)
        {
            BroadcastByType(EUIEventType.PointerDown, transform);
        }
        public void OnPointerUp(PointerEventData data)
        {
            BroadcastByType(EUIEventType.PointerUp, transform);
        }
        public void OnPointerEnter(PointerEventData data)
        {
            BroadcastByType(EUIEventType.PointerEnter, transform);
        }
        public void OnPointerExit(PointerEventData data)
        {
            BroadcastByType(EUIEventType.PointerExit, transform);
        }
        public void OnBeginDrag(PointerEventData data)
        {
            BroadcastByType(EUIEventType.BeginDrag, transform);
        }
        public void OnDrag(PointerEventData data)
        {
            BroadcastByType(EUIEventType.Drag, transform);
        }
        public void OnEndDrag(PointerEventData data)
        {
            BroadcastByType(EUIEventType.EndDrag, transform);
        }
        public void OnScroll(PointerEventData data)
        {
            BroadcastByType(EUIEventType.Scroll, transform);
        }
        public void OnSelect(BaseEventData data)
        {
            BroadcastByType(EUIEventType.Select, transform);
        }
        public void OnDeselect(BaseEventData data)
        {
            BroadcastByType(EUIEventType.Deselect, transform);
        }
        public void OnSubmit(BaseEventData data)
        {
            BroadcastByType(EUIEventType.Submit, transform);
        }
        public void OnCancel(BaseEventData data)
        {
            BroadcastByType(EUIEventType.Cancel, transform);
        }

        // Handler for Slider.onValueChanged
        private void OnSliderValueChanged(float value)
        {
            BroadcastByType(EUIEventType.SliderValueChanged, transform, new CTransformFloatEventArg { transform = transform, floatValue = value });
        }

        // Handler for Toggle.onValueChanged
        private void OnToggleValueChanged(bool value)
        {
            BroadcastByType(EUIEventType.ToggleValueChanged, transform, new CTransformBoolEventArg { transform = transform, boolValue = value });
        }

        // Handler for InputField.onValueChanged
        private void OnInputFieldValueChanged(string value)
        {
            BroadcastByType(EUIEventType.InputFieldValueChanged, transform, new CTransformStringEventArg { transform = transform, stringValue = value });
        }

        // Handler for InputField.onEndEdit
        private void OnInputFieldEndEdit(string value)
        {
            BroadcastByType(EUIEventType.InputFieldEndEdit, transform, new CTransformStringEventArg { transform = transform, stringValue = value });
        }

        // For Animation Broadcast
        public void Broadcast(EEventId @event)
        {
            if (m_eventSystem == null)
            {
                m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>();
                if (m_eventSystem == null)
                {
                    Debug.LogError("EventSystem not found in GameManager");
                    return;
                }
            }
            m_eventSystem.Publish<CTransformEventArg>(@event, new CTransformEventArg { value = transform });
        }
    }

    // Event type enum for UI events
    public enum EUIEventType
    {
        PointerClick,
        PointerDown,
        PointerUp,
        PointerEnter,
        PointerExit,
        BeginDrag,
        Drag,
        EndDrag,
        Scroll,
        Select,
        Deselect,
        Submit,
        Cancel,
        // New event types for common components
        SliderValueChanged,
        ToggleValueChanged,
        InputFieldValueChanged,
        InputFieldEndEdit
    }

    // Event config for UI event emitter
    [Serializable]
    public class CUIEventConfig
    {
        public EUIEventType eventType;
        public EEventId eventId = EEventId.None;
        [SerializeReference]
        public CEventArgBase eventArg;
    }
}
