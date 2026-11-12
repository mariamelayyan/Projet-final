using UnityEngine;

public class FallOnTag : MonoBehaviour
{
    public string fallTag = "trapdoor";
    public Animator animator;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(fallTag)) animator.SetTrigger("fall");
    }
}
