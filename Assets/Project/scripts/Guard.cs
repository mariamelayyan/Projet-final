using UnityEngine;
using System.Collections;

public class Guard : MonoBehaviour
{
    public float speed = 1.5f;
    public float waitTime = 0.3f;
    public float turnSpeed = 90;

    [Header("Chase (starts when the light turns red)")]
    public float chaseSpeed = 2f;
    public float stopDistance = 1f;
    public float loseSightTime = 2f;

    [Header("Vision")]
    public Light spotLight;
    public float viewDistance = 5.31f;
    public LayerMask viewMask;

    [Header("Animation")]
    public Animator animator;
    public string walkParameter = "walking";
    public string runParameter = "running";

    public Transform pathHolder;

    float viewAngle;
    Transform player;
    Color originalSpotLightColor;
    Vector3[] waypoints;
    bool chasing;
    float lastSeenTime;
    int walkId;
    int runId;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotLight.spotAngle;
        originalSpotLightColor = spotLight.color;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        walkId = Animator.StringToHash(walkParameter);
        runId = Animator.StringToHash(runParameter);
        if (animator != null) animator.applyRootMotion = false;

        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++) {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(0, true));
    }

    void Update() {

        if (fallen) return;

        bool seen = CanSeePlayer();
        if (seen) lastSeenTime = Time.time;

        if (!chasing && seen) StartChase();
        else if (chasing && Time.time > lastSeenTime + loseSightTime) StopChase();

        if (chasing) Chase(FlatDistance(player.position));

        // red while he sees the player or is chasing
        spotLight.color = (seen || chasing) ? Color.red : originalSpotLightColor;
    }

    // ---------- Chase ----------

    void StartChase() {
        chasing = true;
        StopAllCoroutines();   // stops the patrol AND the turn coroutine inside it
    }

    void StopChase() {
        chasing = false;
        SetRunning(false);
        SetWalking(false);
        StartCoroutine(FollowPath(NearestWaypointIndex(), false));
    }

    void Chase(float distance) {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * 3f * Time.deltaTime);
        }

        bool moving = distance > stopDistance;
        if (moving) {
            Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.position = Vector3.MoveTowards(transform.position, flatTarget, chaseSpeed * Time.deltaTime);
        }
        SetWalking(moving);
        SetRunning(moving);
    }

    float FlatDistance(Vector3 point) {
        Vector3 d = point - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    int NearestWaypointIndex() {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++) {
            float d = FlatDistance(waypoints[i]);
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return best;
    }

    // ---------- Vision (measured from the light itself) ----------

    bool CanSeePlayer() {
        Vector3 origin = spotLight.transform.position;
        Vector3 toPlayer = player.position - origin;

        if (toPlayer.magnitude < viewDistance) {
            float angle = Vector3.Angle(spotLight.transform.forward, toPlayer);
            if (angle < viewAngle / 2f) {
                if (!Physics.Linecast(origin, player.position, viewMask)) {
                    return true;
                }
            }
        }
        return false;
    }

    // ---------- Patrol ----------

    IEnumerator FollowPath(int targetWaypointIndex, bool teleportToStart) {
        if (teleportToStart) {
            transform.position = waypoints[0];
            targetWaypointIndex = 1 % waypoints.Length;
            transform.LookAt(waypoints[targetWaypointIndex]);
        } else {
            yield return StartCoroutine(TurnToFace(waypoints[targetWaypointIndex]));
        }

        Vector3 targetWaypoint = waypoints[targetWaypointIndex];

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

    void SetWalking(bool walking) {
        if (animator != null) animator.SetBool(walkId, walking);
    }

    void SetRunning(bool running) {
        if (animator != null) animator.SetBool(runId, running);
    }

    // ---------- Fall ----------
    [Header("Fall")]
    public string fallTag = "trapdoor";        // objects with this tag knock him down
    public string fallTrigger = "fall";      // Trigger parameter in the Animator Controller
    public bool turnOffLightWhenDown = true;

        bool fallen;

        void OnCollisionEnter(Collision collision) {
            if (!fallen && collision.collider.CompareTag(fallTag)) Fall();
        }

        void OnTriggerEnter(Collider other) {
            if (!fallen && other.CompareTag(fallTag)) Fall();
        }

        void Fall() {
            fallen = true;
            chasing = false;
            StopAllCoroutines();
            SetWalking(false);
            SetRunning(false);
            if (animator != null) animator.SetTrigger(fallTrigger);
            if (turnOffLightWhenDown) spotLight.enabled = false;
        }
    

    void OnDrawGizmos() {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(spotLight.transform.position, spotLight.transform.forward * viewDistance);
    }
}
