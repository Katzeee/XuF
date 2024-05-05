using System.Collections.Generic;
using System;

namespace Xuf
{
    namespace Common
    {
        public class StateMachine
        {
            List<StateBase> allStates;
            public int curStateIndex
            {
                get; private set;
            }

            public int InitStateIndedx
            {
                get; set;
            }

            public StateMachine()
            {
                allStates = new List<StateBase>();
            }

            public int AddState(StateBase state)
            {
                allStates.Add(state);
                return allStates.Count - 1;
            }

            public void ChangeState(int index)
            {
                if (index < 0 || index >= allStates.Count)
                {
                    throw new Exception("Wrong index");
                }
                if (curStateIndex == index) // out of range or come from self
                {
                    return;
                }
                if (curStateIndex != -1)
                {
                    allStates[curStateIndex].OnExit();
                }

                curStateIndex = index;
                allStates[curStateIndex].OnEnter();
            }

            public void OnUpdate() // start from
            {
                if (curStateIndex == -1)
                {
                    ChangeState(InitStateIndedx);
                }
                allStates[curStateIndex].OnUpdate();
            }
        }
    }
}
