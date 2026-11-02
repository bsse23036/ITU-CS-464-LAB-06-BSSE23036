using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Hazard Damage Settings")]
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DealDamage(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    private void DealDamage(GameObject playerObj)
    {
        Health playerHealth = playerObj.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }
        else if (GameManager.Instance != null)
        {
            // Fallback safety if the Health component is missing from the Player
            GameManager.Instance.ReloadCurrentLevel();
        }
    }
}