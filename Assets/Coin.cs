using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotateSpeed = 100f;

    [Header("Floating / Bobbing Motion")]
    public bool enableBobbing = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Continuous rotation around the Y-axis
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // Gentle floating up-and-down motion
        if (enableBobbing)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
            }

            Destroy(gameObject);
        }
    }
}