using UnityEngine;

// The player's hearts, with a moment of invulnerability after every hit.
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int max = 3;
    [SerializeField] private float iFrames = 1.0f;
    public bool shielded = false;

    private int current;
    private float invulnerableUntil;

    public int Current => current;
    public int Max => max;

    void Awake()
    {
        current = max;
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUI();
        }
    }

    // True if the hit landed. A player who is down, or still invulnerable, can't be hit.
    public bool TakeDamage(int amount)
    {
        if (current <= 0 || Time.time < invulnerableUntil) return false;

        invulnerableUntil = Time.time + iFrames;

        if (shielded)
        {
            shielded = false;
            Debug.Log("Shield absorbed the damage!");
            if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
            return false;
        }

        current -= amount;
        Debug.Log($"Player took {amount} damage! Current Health: {current}/{max}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUI();
        }

        if (current <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }
            else
            {
                Debug.LogWarning("Player health reached zero, but GameManager is missing!");
            }
        }

        return true;
    }

    public void Restore(float protection)
    {
        current = max;
        invulnerableUntil = Time.time + protection;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUI();
        }
    }
}