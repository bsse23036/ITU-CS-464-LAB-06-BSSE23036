using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public Transform cameraTransform;

    [Header("Jump Physics")]
    public float jumpForce = 7f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Game Feel Tweaks")]
    [Tooltip("Grace period (seconds) to jump after walking off a ledge.")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Tooltip("Time window (seconds) to store a jump press before landing.")]
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
{
    rb = GetComponent<Rigidbody>();

    // Automatically locate the main camera transform if unassigned
    if (cameraTransform == null && Camera.main != null)
    {
        cameraTransform = Camera.main.transform;
    }
}

    void Update()
    {
        // Ground detection raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // 1. Coyote Time Logic
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 2. Input Buffer Logic
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // 3. Trigger Jump when both buffer and coyote time are valid
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

            // Reset counters to prevent double-jumping
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 
                                (cameraTransform != null ? cameraTransform.eulerAngles.y : 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // Apply custom gravity multipliers for snappy jump arcs
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}