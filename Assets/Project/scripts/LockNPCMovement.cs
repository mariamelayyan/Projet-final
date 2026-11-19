using UnityEngine;
using UnityEngine.AI;

public class LockNPCMovement : StateMachineBehaviour
{
    NavMeshAgent agent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Skeleton attack state entered");
        agent = animator.GetComponentInParent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null) agent.isStopped = false;
    }
}
