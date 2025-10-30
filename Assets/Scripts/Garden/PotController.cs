using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PotController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int requiredIngredients = 9;
    [SerializeField]
    public List<string> allPossibleIngredients = new List<string>
    {
        "Carrot", "Mushroom", "Tomato", "Onion", "Potato",
        "Radish", "Lettuce", "Parsley", "Jalepenos",
        "Limon", "Garbonzo"
    };

    public GameObject Ingredeient;

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private GameObject emberPrefab;
    [SerializeField] private GameObject winUI; // Assign your win UI panel here
    [SerializeField] private string winSceneName = "WinScene"; // Name of your win scene

    [Header("Penalty")]
    [SerializeField] private float wrongIngredientTimePenalty = 15f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem correctIngredientFX;
    [SerializeField] private ParticleSystem wrongIngredientFX;

    [Header("Events")]
    public UnityEvent OnSoupCompleted;

    private HashSet<string> selectedIngredients = new HashSet<string>();
    private HashSet<string> collectedIngredients = new HashSet<string>();
    private HashSet<string> wrongIngredients = new HashSet<string>();

    private void Start()
    {
        // Hide win UI at start
        if (winUI != null)
        {
            winUI.SetActive(false);
        }

        // Randomly select ingredients (Meat is always included)
        SelectRandomIngredients();

        Debug.Log($"<color=cyan>=== POT CONTROLLER INITIALIZED ===</color>");
        Debug.Log($"Selected Ingredients ({selectedIngredients.Count}): {string.Join(", ", selectedIngredients)}");
        Debug.Log($"Wrong Ingredients ({wrongIngredients.Count}): {string.Join(", ", wrongIngredients)}");
    }

    private void Update()
    {
        // DEBUG: Press P to show pot status
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"<color=cyan>=== POT STATUS ===</color>");
            Debug.Log($"Required Ingredients: {requiredIngredients}");
            Debug.Log($"Collected: {collectedIngredients.Count}/{requiredIngredients}");
            Debug.Log($"Collected List: {string.Join(", ", collectedIngredients)}");
            Debug.Log($"Selected List: {string.Join(", ", selectedIngredients)}");
            Debug.Log($"Wrong List: {string.Join(", ", wrongIngredients)}");
        }
    }

    private void SelectRandomIngredients()
    {
        // MEAT is always required
        selectedIngredients.Add("Meat");
        Debug.Log("<color=green>✓ Meat is ALWAYS required!</color>");

        // We need 8 more ingredients (total of 9 including Meat)
        int remainingSlots = requiredIngredients - 1;

        if (allPossibleIngredients.Count < remainingSlots)
        {
            Debug.LogError($"Not enough ingredients defined! Need at least {remainingSlots} (excluding Meat)");
            return;
        }

        // Shuffle and select random ingredients
        List<string> shuffled = allPossibleIngredients.OrderBy(x => Random.value).ToList();

        // Add first 8 shuffled ingredients
        for (int i = 0; i < remainingSlots; i++)
        {
            selectedIngredients.Add(shuffled[i]);
        }

        // The remaining ingredients are wrong
        for (int i = remainingSlots; i < allPossibleIngredients.Count; i++)
        {
            wrongIngredients.Add(shuffled[i]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=yellow>Pot OnTriggerEnter: {other.gameObject.name}</color>");

        // Check if it's an ingredient
        IngredientCarrier ingredient = other.GetComponent<IngredientCarrier>();
        EmberBehavior ember = other.GetComponent<EmberBehavior>();
        if (ingredient != null|| ember != null)
        {
            Debug.Log($"<color=cyan>Ingredient detected: {ingredient.ingredientName}</color>");
            ReceiveIngredient(ingredient);
        }
    }

    public void ReceiveIngredient(IngredientCarrier ingredient)
    {
        string ingredientName = ingredient.ingredientName;

        Debug.Log($"<color=cyan>ReceiveIngredient called: {ingredientName}</color>");
        Debug.Log($"  Is in selected list? {selectedIngredients.Contains(ingredientName)}");
        Debug.Log($"  Is already collected? {collectedIngredients.Contains(ingredientName)}");

        // Check if it's a correct ingredient
        if (selectedIngredients.Contains(ingredientName))
        {
            // Check if already collected
            if (collectedIngredients.Contains(ingredientName))
            {
                Debug.Log($"<color=yellow>Already collected {ingredientName}! Ignoring.</color>");
                IngredientAlreadyCollected(ingredient);
                return;
            }

            // Add to collected
            collectedIngredients.Add(ingredientName);

            if (correctIngredientFX != null)
            {
                correctIngredientFX.Play();
            }

            Debug.Log($"<color=green>✓ Collected {ingredientName}! ({collectedIngredients.Count}/{requiredIngredients})</color>");

            // Release embers and destroy ingredient
            ingredient.ReleaseAllEmbers();

            Debug.Log($"<color=red>Destroying ingredient: {ingredient.gameObject.name}</color>");
            Destroy(ingredient.gameObject);

            // Check win condition
            if (collectedIngredients.Count >= requiredIngredients)
            {
                CompleteSoup();
            }
        }
        else if (wrongIngredients.Contains(ingredientName))
        {
            // Wrong ingredient!
            WrongIngredientAdded(ingredient);
        }
        else
        {
            Debug.LogWarning($"<color=red>Unknown ingredient: {ingredientName} (not in selected or wrong lists!)</color>");
        }
    }

    private void WrongIngredientAdded(IngredientCarrier ingredient)
    {
        Debug.Log($"<color=red> WRONG INGREDIENT: {ingredient.ingredientName}!</color>");

        if (wrongIngredientFX != null)
        {
            wrongIngredientFX.Play();
        }

        // Time penalty
        if (gameTimer != null)
        {
            gameTimer.SubtractTime(wrongIngredientTimePenalty);
            Debug.Log($" TIME PENALTY: -{wrongIngredientTimePenalty} seconds!");
        }

        // Release embers and destroy
        ingredient.ReleaseAllEmbers();
        Debug.Log($"<color=red>Destroying wrong ingredient: {ingredient.gameObject.name}</color>");
        Destroy(ingredient.gameObject);
    }

    private void IngredientAlreadyCollected(IngredientCarrier ingredient)
    {
        // Release embers and destroy duplicate ingredient
        ingredient.ReleaseAllEmbers();
        Debug.Log($"<color=yellow>Destroying duplicate ingredient: {ingredient.gameObject.name}</color>");
        Destroy(ingredient.gameObject);
    }

    private void CompleteSoup()
    {
        Debug.Log("<color=green> SOUP COMPLETED! YOU WIN! </color>");

        // Stop game timer
        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }

        // Show win UI
        if (winUI != null)
        {
            winUI.SetActive(true);
            Debug.Log("<color=green>Win UI activated!</color>");
        }

        // Invoke event
        OnSoupCompleted?.Invoke();

        // Load win scene after delay
        if (!string.IsNullOrEmpty(winSceneName))
        {
            Invoke(nameof(LoadWinScene), 3f); // 3 second delay for celebration
        }
    }

    private void LoadWinScene()
    {
        Debug.Log($"<color=cyan>Loading win scene: {winSceneName}</color>");
        SceneManager.LoadScene(winSceneName);
    }

    public void SpawnNewEmber(Vector3 position)
    {
        if (emberPrefab != null)
        {
            // Spawn slightly offset from position
            Vector3 spawnPos = position + Random.insideUnitSphere * 0.5f;
            spawnPos.y = position.y;

            GameObject newEmber = Instantiate(emberPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"<color=cyan> New ember spawned at pot!</color>");
        }
        else
        {
            Debug.LogWarning("Ember prefab not assigned to PotController!");
        }
    }

    public float GetProgress()
    {
        return (float)collectedIngredients.Count / requiredIngredients;
    }

    public int GetCollectedCount()
    {
        return collectedIngredients.Count;
    }

    public int GetRequiredCount()
    {
        return requiredIngredients;
    }

    // Debug display
    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 10, 400, 30),
                $"Ingredients: {collectedIngredients.Count}/{requiredIngredients}");
            GUI.Label(new Rect(10, 40, 400, 30),
                $"Progress: {GetProgress() * 100:F0}%");

            // Show collected ingredients
            int yPos = 70;
            GUI.Label(new Rect(10, yPos, 400, 30), "Collected:");
            yPos += 25;
            foreach (string ing in collectedIngredients)
            {
                GUI.Label(new Rect(20, yPos, 400, 20), $" {ing}");
                yPos += 20;
            }
        }
    }
}