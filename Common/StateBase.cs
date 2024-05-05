using UnityEngine;

namespace Xuf
{
    namespace Common
    {
        public class StateBase
        {
            // public Animator stateAnimator;
            protected StateMachine stateMachine;
            public StateBase(Animator animator, StateMachine stateMachine)
            {
                // this.stateAnimator = tmpAnimator;
                this.stateMachine = stateMachine;
            }
            public virtual void OnEnter()
            {

            }
            public virtual void OnUpdate()
            {

            }
            public virtual void OnExit()
            {

            }
        }
    }
}
