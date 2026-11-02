using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("TextMeshPro References")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timerText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateCoinsDisplay(int coinCount)
    {
        if (coinsText != null) coinsText.text = "Coins: " + coinCount;
    }

    public void UpdateHealthDisplay(int currentHealth)
    {
        if (healthText != null)
        {
            string heartsDisplay = "";
            for (int i = 0; i < currentHealth; i++)
            {
                heartsDisplay += "♥ ";
            }
            healthText.text = "Health: " + heartsDisplay;
        }
    }

    public void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = "Time: " + seconds + "s";
        }
    }
}