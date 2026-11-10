using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Guard : MonoBehaviour
{

    public float speed = 5;
    public float waitTime = 0.3f;
    public float turnSpeed = 90;

    public Light spotLight;
    public float viewDistance;
    public LayerMask viewMask;

    float viewAngle;

    public Transform pathHolder;
    Transform player;
    Color originalSpotLightColor;

    public Animator animator;
    public string walkParameter = "walking";   // must match the Bool name in the Animator Controller

    int walkId;

    void SetWalking(bool walking) {
        if (animator != null) animator.SetBool(walkId, walking);
    }

    void Start() {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        walkId = Animator.StringToHash(walkParameter);
        if (animator != null) animator.applyRootMotion = false;
        
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotLight.spotAngle;
        originalSpotLightColor = spotLight.color;

        Vector3[] waypoints = new Vector3[pathHolder.childCount] ;
        for (int i = 0; i < waypoints.Length; i++) {
            waypoints[i] = pathHolder.GetChild (i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }
    void Update() {
        if (CanSeePlayer()) {
            spotLight.color = Color.red;
        } else {
            spotLight.color = originalSpotLightColor;
        }
    }

    bool CanSeePlayer() {
        if (Vector3.Distance(transform.position, player.position) < viewDistance) {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleBetweenGuardAndPlayer < viewAngle / 2f) {
                if (!Physics.Linecast(transform.position, player.position, viewMask)) {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints) {
        transform.position = waypoints [0];
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints [targetWaypointIndex];
        transform.LookAt (targetWaypoint);

        while (true) {
            SetWalking(true);

            Vector3 flatTarget = new Vector3(targetWaypoint.x, transform.position.y, targetWaypoint.z);
            transform.position = Vector3.MoveTowards(transform.position, flatTarget, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, flatTarget) < 0.05f) {
                SetWalking(false);
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
                Debug.Log("Reached waypoint, heading to index " + targetWaypointIndex);
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget) {
        Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(directionToLookTarget.z, directionToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f) {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    void OnDrawGizmos( ) {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach(Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position,.3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine (previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(spotLight.transform.position, spotLight.transform.forward * viewDistance);
    }
}
