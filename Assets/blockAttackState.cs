using UnityEngine;

public class BlockAttackState : StateMachineBehaviour
{
    public string blockParameter = "blockAttack";
    public string triggerName = "attack";   // your trigger, spelled exactly

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(blockParameter, true);
        animator.ResetTrigger(triggerName);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(blockParameter, true);     // stays locked even when two blocked states follow each other
        animator.ResetTrigger(triggerName);         // throws away presses made during this state
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(blockParameter, false);
    }
}
