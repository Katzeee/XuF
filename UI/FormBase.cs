using System;
using UnityEngine.EventSystems;
using Xuf.Core;

namespace Xuf.UI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor |
        AttributeTargets.Field | AttributeTargets.Method |
        AttributeTargets.Property)]
    public class UIPrefab : Attribute
    {
        public string path;
        public UIPrefab(string path)
        {
            this.path = path;
        }
    }

    public abstract class FormBase : UIBehaviour
    {
        abstract public void OnActivate();
        abstract public void OnDeActivate();

        protected CEventSystem m_eventSystem = CSystemManager.Instance.GetSystem<CEventSystem>();

        public void Broadcast(EEventId eventId, CEventData data)
        {
            m_eventSystem.Broadcast(eventId, data);
        }
    }
}
