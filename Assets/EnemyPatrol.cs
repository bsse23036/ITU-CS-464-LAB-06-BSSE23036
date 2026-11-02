
using UnityEngine;

public enum PatrolMode
{
    Loop,
    PingPong,
    Random
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] points;
    [SerializeField] private PatrolMode mode = PatrolMode.Loop;

    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float waitTime = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;

    [Range(0f, 360f)]
    [SerializeField] private float fieldOfViewAngle = 90f;

    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private float chaseMemoryDuration = 3f;

    [Header("Combat Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Optional Reset Settings")]
    [SerializeField] private bool reloadLevelOnContact = false;

    private Rigidbody rb;
    private Transform player;

    private int currentPointIndex;
    private int direction = 1;

    private float waitUntil;
    private float lastSeenTime;
    private float nextDamageTime;

    private bool isChasing;
    private bool hasLastKnownPosition;

    private Vector3 lastKnownPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Prevent physics from tipping the enemy over.
        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;

        // Interpolate movement for smoother visuals.
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "EnemyPatrol: No GameObject with Player tag found.",
                this
            );
        }

        if (points == null || points.Length == 0)
        {
            Debug.LogWarning(
                "EnemyPatrol: No patrol points assigned.",
                this
            );
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        if (CanSeePlayer())
        {
            isChasing = true;
            lastSeenTime = Time.time;

            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
        }
        else if (isChasing &&
                 Time.time - lastSeenTime > chaseMemoryDuration)
        {
            isChasing = false;
            hasLastKnownPosition = false;

            waitUntil = Time.time;
        }
    }

    private void FixedUpdate()
    {
        if (isChasing && hasLastKnownPosition)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // ---------------- PATROL ----------------

    private void Patrol()
    {
        if (points == null || points.Length == 0)
            return;

        if (currentPointIndex >= points.Length ||
            points[currentPointIndex] == null)
            return;

        if (Time.time < waitUntil)
            return;

        Vector3 target = points[currentPointIndex].position;

        // Keep movement on the ground plane.
        target.y = rb.position.y;

        MoveTowards(target, patrolSpeed);

        if (Vector3.Distance(
            new Vector3(rb.position.x, 0f, rb.position.z),
            new Vector3(target.x, 0f, target.z)
        ) < 0.15f)
        {
            currentPointIndex = GetNextPointIndex();
            waitUntil = Time.time + waitTime;
        }
    }

    private int GetNextPointIndex()
    {
        if (points.Length <= 1)
            return 0;

        switch (mode)
        {
            case PatrolMode.Loop:

                return (currentPointIndex + 1) % points.Length;

            case PatrolMode.PingPong:

                if (currentPointIndex + direction >= points.Length ||
                    currentPointIndex + direction < 0)
                {
                    direction *= -1;
                }

                return currentPointIndex + direction;

            case PatrolMode.Random:

                int next = currentPointIndex;

                while (next == currentPointIndex)
                {
                    next = Random.Range(0, points.Length);
                }

                return next;

            default:

                return 0;
        }
    }

    // ---------------- CHASE ----------------

    private void ChasePlayer()
    {
        Vector3 target = lastKnownPosition;

        target.y = rb.position.y;

        MoveTowards(target, chaseSpeed);

        if (Vector3.Distance(
            new Vector3(rb.position.x, 0f, rb.position.z),
            new Vector3(target.x, 0f, target.z)
        ) < 0.2f && !CanSeePlayer())
        {
            hasLastKnownPosition = false;
        }
    }

    // ---------------- MOVEMENT ----------------

    private void MoveTowards(Vector3 target, float speed)
    {
        Vector3 directionToTarget = target - rb.position;

        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude < 0.001f)
            return;

        Vector3 movement = directionToTarget.normalized;

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        Quaternion targetRotation = Quaternion.LookRotation(movement);

        Quaternion newRotation = Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    // ---------------- PLAYER DETECTION ----------------

    private bool CanSeePlayer()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 0.8f;

        Vector3 playerPosition = player.position + Vector3.up * 0.8f;

        Vector3 directionToPlayer = playerPosition - eyePosition;

        // Ignore vertical difference when calculating FOV.
        Vector3 flatDirection = directionToPlayer;
        flatDirection.y = 0f;

        float distance = directionToPlayer.magnitude;

        // Check detection distance.
        if (distance > detectionRadius)
            return false;

        // Check field of view.
        float angle = Vector3.Angle(
            transform.forward,
            flatDirection.normalized
        );

        if (angle > fieldOfViewAngle / 2f)
            return false;

        // Check whether walls or cover block vision.
        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        ))
        {
            return false;
        }

        return true;
    }

    // ---------------- DAMAGE ----------------

    private void OnCollisionEnter(Collision collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject target)
    {
        if (!target.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time + damageCooldown;

        Health playerHealth = target.GetComponent<Health>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);

            Debug.Log("Enemy damaged the player!");
        }

        if (reloadLevelOnContact &&
            GameManager.Instance != null)
        {
            GameManager.Instance.ReloadCurrentLevel();
        }
    }

    // ---------------- GIZMOS ----------------

    private void OnDrawGizmosSelected()
    {
        // Detection radius.
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );

        // Field of view.
        Vector3 leftDirection =
            Quaternion.Euler(0f, -fieldOfViewAngle / 2f, 0f)
            * transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(0f, fieldOfViewAngle / 2f, 0f)
            * transform.forward;

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            transform.position,
            leftDirection * detectionRadius
        );

        Gizmos.DrawRay(
            transform.position,
            rightDirection * detectionRadius
        );

        // Patrol paths.
        if (points == null || points.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
                continue;

            Gizmos.DrawSphere(points[i].position, 0.2f);

            int nextIndex = (i + 1) % points.Length;

            if (mode == PatrolMode.Loop ||
                (mode == PatrolMode.PingPong && i < points.Length - 1))
            {
                if (points[nextIndex] != null)
                {
                    Gizmos.DrawLine(
                        points[i].position,
                        points[nextIndex].position
                    );
                }
            }
        }
    }
}
