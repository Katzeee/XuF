using System.Collections.Generic;
using System;
using UnityEngine;

namespace Xuf.Common
{
    public abstract class CStateBase : IState
    {
        public CStateMachine stateMachine { get; internal set; }
        public abstract void OnEnter();
        public abstract void OnExit();
        public abstract void OnUpdate();
    }

    public class CStateMachine
    {
        public delegate CStateBase CreateWrapper(object param);
        private Dictionary<Type, CreateWrapper> m_allStates = new();
        private Stack<CStateBase> m_stateStack = new();
        public CStateBase CurState { get { return m_stateStack.Peek(); } }

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
            var state = m_allStates[type](param);
            state.stateMachine = this;
            if (m_stateStack.Count > 0)
            {
                m_stateStack.Peek().OnExit();
            }
            state.OnEnter();
            m_stateStack.Push(state);
        }

        public void Exit()
        {
            if (m_stateStack.Count == 0)
            {
                return;
            }
            m_stateStack.Pop().OnExit();
            m_stateStack.Peek()?.OnEnter();
        }

        public void Clear()
        {
            m_stateStack.Clear();
        }

        public void OnUpdate()
        {
            m_stateStack.Peek()?.OnUpdate();
        }
    }
}
