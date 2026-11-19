using UnityEngine;

public class FallStateBehaviour : StateMachineBehaviour
{
    public float depth = 4f;        // how far he drops
    public float duration = 0.8f;   // seconds to reach the bottom

    float startY;
    float enterTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        startY = animator.transform.position.y;
        enterTime = Time.time;
    }

    // Runs after the animation is applied, so it overrides any Y the clip sets
    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        float t = Mathf.Clamp01((Time.time - enterTime) / duration);
        Vector3 p = animator.transform.position;
        p.y = startY - depth * t * t;   // t*t = starts slow, speeds up like gravity
        animator.transform.position = p;
    }
}
