using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score & Level Settings")]
    public int coinsCollected = 0;
    public bool useTimer = false;
    public float levelTimer = 60f;

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (useTimer && !isGameOver)
        {
            levelTimer -= Time.deltaTime;
            if (levelTimer <= 0f)
            {
                levelTimer = 0f;
                PlayerDied();
            }
        }
    }

    public void AddCoin()
    {
        coinsCollected++;
        UpdateUI();
    }

    public void UpdateUI()
{
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateCoinsDisplay(coinsCollected);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                UIManager.Instance.UpdateHealthDisplay(playerHealth.Current);
            }
        }
    }
}

    public void PlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("Player Died! Reloading scene...");
        ReloadCurrentLevel();
    }

    public void ReloadCurrentLevel()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        isGameOver = false;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("All levels completed!");
        }
    }
}