using UnityEngine;

namespace MainGame
{
    public class ResetFallingBool : StateMachineBehaviour
    {
        public string IsInteractingBool;
        public bool IsInteractingStatus;

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(IsInteractingBool, IsInteractingStatus);
        }
    }
}