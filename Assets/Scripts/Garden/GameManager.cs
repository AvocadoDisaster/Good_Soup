using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float gameTime = 60f;
    public int trashLimit = 3; // Number of trash items before game over

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI ingredientCountText;
    public TextMeshProUGUI trashCountText; // Display trash count
    public GameObject gameOverUI; // Optional game over panel for trash

    [Header("Scoring")]
    public int ingredientPoints = 1;
    public int emberPoints = 2;
    public int trashPenalty = -3; // Points lost per trash item in POT

    [Header("Audio")]
    [SerializeField] private AudioSource gameOverSound; // NEW: Womp womp sound for trash game over

    // Game Stats
    private int totalScore = 0;
    private int ingredientsScored = 0;
    private int embersScored = 0;
    private int trashCount = 0;
    private float timeRemaining;
    private bool gameActive = false;
    private bool gameOverByTrash = false;
    EndScreenController endScreenController;

    // Ingredient tracking dictionary
    private Dictionary<string, int> ingredientCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        IsGameActive();
        timeRemaining = gameTime;
        gameActive = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        UpdateUI();
    }

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame(false); // Normal time-based end
        }

        UpdateTimerUI();
    }

    // Called when ingredient enters pot
    public void IngredientScored(string ingredientName = "Ingredient")
    {
        if (!gameActive) return;

        ingredientsScored++;
        totalScore += ingredientPoints;

        // Track specific ingredient type
        if (ingredientCounts.ContainsKey(ingredientName))
        {
            ingredientCounts[ingredientName]++;
        }
        else
        {
            ingredientCounts[ingredientName] = 1;
        }

        Debug.Log($"<color=green>Ingredient scored! Total: {ingredientsScored}</color>");
        UpdateUI();
    }

    // Called when ember enters pot
    public void EmberScored()
    {
        if (!gameActive) return;

        embersScored++;
        totalScore += emberPoints;

        Debug.Log($"<color=orange>Ember scored! +{emberPoints} points! Total: {embersScored}</color>");
        UpdateUI();
    }

    // NEW: Called when trash enters pot
    public void TrashCollected()
    {
        if (!gameActive) return;

        trashCount++;
        totalScore += trashPenalty; // Apply penalty

        Debug.Log($"<color=red>⚠ Trash collected! {trashPenalty} points! Count: {trashCount}/{trashLimit}</color>");

        UpdateUI();

        if (trashCount >= trashLimit)
        {
            EndGame(true); // Game over by trash - LOSE CONDITION
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }
        else
        {
            Debug.LogWarning("scoreText is not assigned in GameManager!");
        }

        if (ingredientCountText != null)
        {
            ingredientCountText.text = "Ingredients: " + ingredientsScored;
        }
        else
        {
            Debug.LogWarning("ingredientCountText is not assigned in GameManager!");
        }

        // Update trash count UI with visual warnings
        if (trashCountText != null)
        {
            trashCountText.text = "Trash: " + trashCount + "/" + trashLimit;

            // Color changes based on trash count
            if (trashCount >= trashLimit - 1)
            {
                trashCountText.color = Color.red;
            }
            else if (trashCount >= trashLimit / 2)
            {
                trashCountText.color = new Color(1f, 0.5f, 0f); // Orange
            }
            else
            {
                trashCountText.color = Color.white;
            }
        }
        else
        {
            Debug.LogWarning("trashCountText is not assigned in GameManager!");
        }

        Debug.Log($"<color=cyan>UI Updated - Score: {totalScore}, Ingredients: {ingredientsScored}, Trash: {trashCount}</color>");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Optional: Make timer red when low
            if (timeRemaining <= 10f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining <= 30f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    void EndGame(bool endedByTrash)
    {
        gameActive = false;
        gameOverByTrash = endedByTrash;

        if (endedByTrash)
        {
            // LOSE CONDITION - Too much trash
            Debug.Log("<color=red>💀 GAME OVER! Too much trash in the soup!</color>");

            // Play womp womp sound effect
            if (gameOverSound != null)
            {
                gameOverSound.Play();
            }

            // Show trash game over UI if available
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
            else
            {
                // Load game over scene
                DontDestroyOnLoad(this);
                SaveAndLoadEndScene();
            }
        }
        else
        {
            // WIN CONDITION - Time ran out normally
            Debug.Log("<color=cyan>🎉 Time's up! Loading End Scene...</color>");
            DontDestroyOnLoad(this);
            SaveAndLoadEndScene();
        }
    }

    private void SaveAndLoadEndScene()
    {
        // Save all stats for end screen
        PlayerPrefs.SetInt("FinalScore", totalScore);
        PlayerPrefs.SetInt("IngredientsScored", ingredientsScored);
        PlayerPrefs.SetInt("EmbersScored", embersScored);
        PlayerPrefs.SetInt("TrashCount", trashCount);
        PlayerPrefs.SetInt("GameOverByTrash", gameOverByTrash ? 1 : 0); // 1 = LOSE, 0 = WIN

        // Save ingredient breakdown
        string breakdown = "";
        foreach (var kvp in ingredientCounts)
        {
            breakdown += kvp.Key + ":" + kvp.Value + ";";
        }
        PlayerPrefs.SetString("IngredientBreakdown", breakdown);

        PlayerPrefs.Save();

        // Load appropriate scene
        if (gameOverByTrash)
        {
            // Load a "GameOver" scene or the same scene with different display
            // Check in your end screen: if (PlayerPrefs.GetInt("GameOverByTrash") == 1) show LOSE
            SceneManager.LoadScene("YouWin"); // You can rename this to "EndScreen" 
        }
        else
        {
            SceneManager.LoadScene("YouWin"); // WIN condition
        }
    }

    // Restart game (can be called from game over UI button)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameActive()
    {
        return gameActive;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public int GetTrashCount()
    {
        return trashCount;
    }

    public bool IsGameOverByTrash()
    {
        return gameOverByTrash;
    }

    // NEW: Helper to check if player lost
    public bool DidPlayerLose()
    {
        return gameOverByTrash;
    }

    // NEW: Helper to check if player won
    public bool DidPlayerWin()
    {
        return !gameOverByTrash && !gameActive;
    }
}