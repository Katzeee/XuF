using UnityEngine;
using System;
using Xuf.Common;

namespace Xuf
{
    namespace UI
    {
        [AttributeUsage(AttributeTargets.Class |
            AttributeTargets.Constructor |
            AttributeTargets.Field |
            AttributeTargets.Method |
            AttributeTargets.Property)]
        public class UIPrefab : Attribute
        {
            public string name;
            public string path;
            public UIPrefab(string name, string path)
            {
                this.name = name;
                this.path = path;
            }
        }

        public class PanelBase<TEventType, TEventData> : MonoBehaviour
            where TEventType : struct, Enum
            where TEventData : struct, IEventData
        {
            static protected CUIManager<TEventType, TEventData> s_UIManager = CUIManager<TEventType, TEventData>.Instance;

            public PanelBase()
            {
                // Register panel to UI Manager
            }

            // TODO: select enum from unity inspect window
            public void Broadcast(TEventType @event, in TEventData data)
            {
                s_UIManager.Broadcast(@event, data);
            }

            // use by unity editor button action
            public void Broadcast(string @eventName)
            {
                var @event = (TEventType)Enum.Parse(typeof(TEventType), @eventName, true);
                s_UIManager.Broadcast(@event, new());
            }
        }
    }
}
