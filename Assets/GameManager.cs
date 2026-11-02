using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score & Level Settings")]
    public int coinsCollected = 0;
    public bool useTimer = true;
    public float levelTimer = 60f;

    [Header("Runtime Status")]
    [SerializeField] private float currentTimer;

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentTimer = levelTimer; // Explicit initialization on start
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetLevelState();
    }

    private void ResetLevelState()
    {
        coinsCollected = 0;
        currentTimer = levelTimer;
        isGameOver = false;
        UpdateUI();
    }

    void Update()
    {
        if (useTimer && !isGameOver)
        {
            currentTimer -= Time.deltaTime;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTimerDisplay(currentTimer);
            }

            if (currentTimer <= 0f)
            {
                currentTimer = 0f;
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
            UIManager.Instance.UpdateTimerDisplay(currentTimer);

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
        Debug.Log("Player Died or Time Ran Out! Reloading scene...");
        ReloadCurrentLevel();
    }

    public void ReloadCurrentLevel()
    {
        ResetLevelState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        ResetLevelState();
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