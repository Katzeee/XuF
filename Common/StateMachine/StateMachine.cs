using System.Collections.Generic;
using System;
using UnityEngine;

namespace Xuf.Common
{
    public class CStateBase : IState
    {
        public CStateMachine stateMachine { get; internal set; }
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
    }

    public class CStateMachine
    {
        public delegate CStateBase CreateWrapper(object param);
        private Dictionary<Type, CreateWrapper> m_allStates = new();
        private CStateBase m_currentState;
        public CStateBase CurState { get { return m_currentState; } }

        public void RegisterState<T>(CreateWrapper creator)
        {
            m_allStates.TryAdd(typeof(T), creator);
        }

        // For every state changing, you should directly new a new state object,
        // because state is with state, it has its internal infomation
        public void ChangeState<T>(object param = null) where T : CStateBase
        {
            Type type = typeof(T);
            if (!m_allStates.ContainsKey(type))
            {
                Debug.LogWarning($"{type} has not been registered");
                return;
            }

            // Exit current state
            if (m_currentState != null)
            {
                m_currentState.OnExit();
            }

            // Create and enter new state
            var newState = m_allStates[type](param);
            newState.stateMachine = this;
            newState.OnEnter();
            m_currentState = newState;
        }

        public void Clear()
        {
            if (m_currentState != null)
            {
                m_currentState.OnExit();
                m_currentState = null;
            }
        }

        public void OnUpdate()
        {
            m_currentState?.OnUpdate();
        }
    }
}
