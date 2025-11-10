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

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI ingredientCountText;
    public TextMeshProUGUI emberCountText;

    [Header("Scoring")]
    public int ingredientPoints = 1;
    public int emberPoints = 2;

    // Game Stats
    private int totalScore = 0;
    private int ingredientsScored = 0;
    private int embersScored = 0;
    private float timeRemaining;
    private bool gameActive = false;

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
        UpdateUI();
    }

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
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

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }

        if (ingredientCountText != null)
        {
            ingredientCountText.text = "Ingredients: " + ingredientsScored;
        }

        if (emberCountText != null)
        {
            emberCountText.text = "Embers: " + embersScored;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void EndGame()
    {
        gameActive = false;

        // Save all stats for end screen
        PlayerPrefs.SetInt("FinalScore", totalScore);
        PlayerPrefs.SetInt("IngredientsScored", ingredientsScored);
        PlayerPrefs.SetInt("EmbersScored", embersScored);

        // Save ingredient breakdown (as JSON or simple string)
        string breakdown = "";
        foreach (var kvp in ingredientCounts)
        {
            breakdown += kvp.Key + ":" + kvp.Value + ";";
        }
        PlayerPrefs.SetString("IngredientBreakdown", breakdown);

        PlayerPrefs.Save();

        Debug.Log("<color=cyan>Game Over! Loading End Scene...</color>");
        SceneManager.LoadScene("YouWin");
    }

    public bool IsGameActive()
    {
        return gameActive;
    }

    public int GetTotalScore()
    {
        return totalScore;
    }
}