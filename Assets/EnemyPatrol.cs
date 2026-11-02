using UnityEngine;

public enum PatrolMode { Loop, PingPong, Random }

// Custom non-NavMesh Enemy Patrol with FOV detection and player chase behavior
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] points;
    [SerializeField] private PatrolMode mode = PatrolMode.Loop;
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float waitTime = 1.0f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Detection & Stealth Settings")]
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float detectionRadius = 10f;
    [Range(0, 360)] [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask; // Assign cover/wall layers to block vision

    private Transform player;
    private int index;
    private int step = 1;
    private float waitUntil;
    private bool isChasing;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // 1. Check if player is within line of sight
        if (player != null && CanSeePlayer())
        {
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            // Resume patrol if player breaks line of sight
            if (isChasing)
            {
                isChasing = false;
                waitUntil = Time.time; // Continue patrol immediately
            }

            Patrol();
        }
    }
    // void LateUpdate()
    // {
    //     // Strip away pitch (X) and roll (Z), keeping only the yaw (Y) turn rotation
    //     Vector3 euler = transform.rotation.eulerAngles;
    //     transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    // }

   private void Patrol()
{
    if (points == null || points.Length == 0) return;
    if (Time.time < waitUntil) return;

    Vector3 target = points[index].position;

    // Ignore Y-height for flat 2D distance calculation
    Vector3 currentPos = transform.position;
    Vector3 targetPos = new Vector3(target.x, currentPos.y, target.z);

    Vector3 toTarget = targetPos - currentPos;
    float distance = toTarget.magnitude;

    // 1. Move only if farther than 0.15 meters from target to prevent overshooting jitter
    if (distance > 0.15f)
    {
        // Move towards target position
        transform.position = Vector3.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);

        // Smooth rotation towards movement direction
        Quaternion lookRotation = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
    }
    else
    {
        // Destination reached cleanly without overshooting
        index = NextIndex();
        waitUntil = Time.time + waitTime;
    }
}

    private void ChasePlayer()
    {
        Vector3 target = player.position;
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0;
        // Direct movement towards player position
        transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);

        // Direct rotation towards player
        if (toTarget != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position);
        float distanceToPlayer = dirToPlayer.magnitude;

        // 1. Distance check
        if (distanceToPlayer <= detectionRadius)
        {
            // 2. FOV Angle check
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer.normalized);
            if (angleToPlayer <= fieldOfViewAngle / 2f)
            {
                // 3. Line of sight Raycast check (checks if obstacles block vision)
                Vector3 eyePosition = transform.position + Vector3.up * 0.5f;
                Vector3 playerTargetPos = player.position + Vector3.up * 0.5f;
                Vector3 rayDir = (playerTargetPos - eyePosition).normalized;

                if (!Physics.Raycast(eyePosition, rayDir, distanceToPlayer, obstacleMask))
                {
                    return true; // Line of sight clear
                }
            }
        }
        return false;
    }

    private int NextIndex()
    {
        if (points == null || points.Length == 0) return 0;

        if (mode == PatrolMode.Loop)
        {
            return (index + 1) % points.Length;
        }

        if (mode == PatrolMode.PingPong)
        {
            if (index + step < 0 || index + step >= points.Length)
            {
                step = -step;
            }
            return Mathf.Clamp(index + step, 0, points.Length - 1);
        }

        // Random mode
        int next = Random.Range(0, points.Length);
        return (next == index && points.Length > 1) ? (next + 1) % points.Length : next;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerReset();
        }
    }

    private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        Health playerHealth = collision.gameObject.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
        }
    }
}

    private void TriggerReset()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReloadCurrentLevel();
        }
    }

    // Visualize FOV cone and patrol path in the Scene View
    private void OnDrawGizmos()
    {
        // Draw Patrol Path
        if (points != null && points.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    Gizmos.DrawSphere(points[i].position, 0.2f);
                    Transform next = points[(i + 1) % points.Length];
                    if (next != null && mode == PatrolMode.Loop)
                    {
                        Gizmos.DrawLine(points[i].position, next.position);
                    }
                }
            }
        }

        // Draw Vision Cone & Detection Radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, Vector3.up) * transform.forward;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, fovLine1 * detectionRadius);
        Gizmos.DrawRay(transform.position, fovLine2 * detectionRadius);
    }
}