using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PotController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int requiredIngredients = 9; // 9 ingredients total (including Meat)
    [SerializeField] Transform potl;
    [SerializeField] private int requiredGarbanzoBeans = 15; // Number of individual beans needed
    [SerializeField]
    public List<string> allPossibleIngredients = new List<string>
    {
        "Meat","Carrot", "Mushroom", "Tomato", "Onion", "Potato",
        "Radish", "Lettuce", "Parsley", "Jalepenos", "Limon"
        // Note:  and "Garbonzo" are handled separately - Meat is always required, Garbonzo needs 15 beans
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

    public HashSet<string> selectedIngredients = new HashSet<string>();
    private HashSet<string> collectedIngredients = new HashSet<string>();
    private HashSet<string> wrongIngredients = new HashSet<string>();
    private int garbanzoBeanCount = 0; // Track individual bean count
    private bool garbanzoComplete = false; // Track if garbanzo beans are complete

    private void Start()
    {
        // Hide win UI at start
        if (winUI != null)
        {
            winUI.SetActive(false);
        }

        // Randomly select ingredients (Meat & garbonzo is always included)
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
            Debug.Log($"Garbanzo Beans: {garbanzoBeanCount}/{requiredGarbanzoBeans} ({GetGarbanzoProgress() * 100:F1}%)");
            Debug.Log($"Garbanzo Complete: {garbanzoComplete}");
            Debug.Log($"Selected List: {string.Join(", ", selectedIngredients)}");
            Debug.Log($"Wrong List: {string.Join(", ", wrongIngredients)}");
        }
    }

    private void SelectRandomIngredients()
    {
        
        selectedIngredients.Add("Garbonzo");
        Debug.Log("<color=green>✓ Garbonzo is ALWAYS required!</color>");

        // We need 8 more ingredients (total of 9 including Meat)
        int remainingSlots = requiredIngredients;
        ;
        Debug.Log("<color=green>✓ Garbonzo is ALWAYS required!</color>");

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
        selectedIngredients.Add("Garbonzo");
        // The remaining ingredients are wrong
        for (int i = remainingSlots; i < allPossibleIngredients.Count; i++)
        {
            wrongIngredients.Add(shuffled[i]);
            if(wrongIngredients.Contains("Garbonzo"))
            {
                wrongIngredients.Remove("Garbonzo");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=yellow>Pot OnTriggerEnter: {other.gameObject.name} (Tag: {other.tag})</color>");

        // Check if it's an ingredient
        IngredientCarrier ingredient = other.GetComponent<IngredientCarrier>();
        if (ingredient != null)
        {
            Debug.Log($"<color=cyan>Ingredient detected: {ingredient.ingredientName}</color>");
            ReceiveIngredient(ingredient);
            return;
        }

        // Check if it's a Garbanzo Bean
        GarbanzoBean bean = other.GetComponent<GarbanzoBean>();
        if (bean != null)
        {
            Debug.Log($"<color=cyan>Garbanzo Bean detected!</color>");
            ReceiveGarbanzoBean(bean);
            return;
        }

        // Check if it's Charcoal
        Charcoal charcoal = other.GetComponent<Charcoal>();
        if (charcoal != null)
        {
            Debug.Log($"<color=orange>Charcoal detected!</color>");
            ReceiveCharcoal(charcoal);
            return;
        }

        Debug.Log($"<color=grey>Object entered pot but has no relevant component</color>");
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

    private void ReceiveGarbanzoBean(GarbanzoBean bean)
    {
        Debug.Log($"<color=cyan>========== GARBANZO BEAN RECEIVED ==========</color>");
        Debug.Log($"  Current count: {garbanzoBeanCount}/{requiredGarbanzoBeans}");

        // Check if Garbonzo is a correct ingredient
        if (selectedIngredients.Contains("Garbonzo"))
        {
            // Check if already completed all beans
            if (garbanzoComplete)
            {
                Debug.Log($"<color=yellow>Already collected all {requiredGarbanzoBeans} Garbonzo beans! Ignoring.</color>");
                Destroy(bean.gameObject);
                return;
            }

            // Add to bean count
            garbanzoBeanCount++;

            Debug.Log($"<color=green>✓ Garbanzo Bean #{garbanzoBeanCount} collected!</color>");
            Debug.Log($"  Progress: {garbanzoBeanCount}/{requiredGarbanzoBeans} ({GetGarbanzoProgress() * 100:F1}%)</color>");

            if (correctIngredientFX != null)
            {
                correctIngredientFX.Play();
            }

            // Check if we've collected all beans
            if (garbanzoBeanCount >= requiredGarbanzoBeans)
            {
                garbanzoComplete = true;
                collectedIngredients.Add("Garbonzo");
                Debug.Log($"<color=green>🎉 ALL GARBANZO BEANS COLLECTED! ({requiredGarbanzoBeans}/{requiredGarbanzoBeans})</color>");
                Debug.Log($"<color=green>Garbonzo added to collected ingredients ({collectedIngredients.Count}/{requiredIngredients})</color>");
            }

            // Destroy bean
            Destroy(bean.gameObject);

            // Check win condition
            if (collectedIngredients.Count >= requiredIngredients)
            {
                CompleteSoup();
            }
        }
        
        else
        {
            Debug.LogWarning($"<color=yellow>Garbonzo not in selected or wrong lists!</color>");
            Destroy(bean.gameObject);
        }
    }

    private void ReceiveCharcoal(Charcoal charcoal)
    {
        Debug.Log($"<color=orange>========== CHARCOAL RECEIVED ==========</color>");
        Debug.Log($"  Spawning new ember at pot position: {potl.position}");

        // Charcoal always spawns a new ember
        SpawnNewEmber(potl.position);

        // Destroy charcoal
        Debug.Log($"<color=orange>  Destroying charcoal: {charcoal.gameObject.name}</color>");
        Destroy(charcoal.gameObject);

        Debug.Log($"<color=green>✓ Charcoal processed successfully!</color>");
    }

    private void LoadWinScene()
    {
        Debug.Log($"<color=cyan>Loading win scene: {winSceneName}</color>");
        SceneManager.LoadScene(winSceneName);
    }

    public void SpawnNewEmber(Vector3 position)
    {
        if (emberPrefab == null)
        {
            Debug.LogError("<color=red> Ember prefab not assigned to PotController!</color>");
            return;
        }

        // Try to find a valid NavMesh position near the pot
        Vector3 spawnPos = position;
        spawnPos.y = position.y + 0.5f; // Start slightly above ground

        NavMeshHit hit;
        // Search within 5 units for a valid NavMesh position
        if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
            spawnPos.y += 0.5f; // Spawn slightly above the NavMesh
            Debug.Log($"<color=green>Found valid NavMesh position at {spawnPos}</color>");
        }
        else
        {
            Debug.LogWarning($"<color=yellow> Could not find NavMesh near {position}, spawning at original position</color>");
        }

        GameObject newEmber = Instantiate(emberPrefab, spawnPos, Quaternion.identity);

        // Verify the ember has NavMeshAgent and try to place it on NavMesh
        NavMeshAgent agent = newEmber.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Disable agent temporarily
            agent.enabled = false;

            // Set position
            newEmber.transform.position = spawnPos;

            // Try to warp to NavMesh
            if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
            {
                newEmber.transform.position = hit.position;
                Debug.Log($"<color=cyan>Warped new ember to NavMesh at {hit.position}</color>");
            }

            // Re-enable agent
            agent.enabled = true;

            // Verify it's on NavMesh
            if (agent.isOnNavMesh)
            {
                Debug.Log($"<color=green>✓ New ember successfully spawned on NavMesh at {newEmber.transform.position}!</color>");
            }
            else
            {
                Debug.LogError($"<color=red> New ember NOT on NavMesh after spawn! Position: {newEmber.transform.position}</color>");
            }
        }
        else
        {
            Debug.LogError($"<color=red> Spawned ember has no NavMeshAgent component!</color>");
        }
    }

    public float GetProgress()
    {
        return (float)collectedIngredients.Count / requiredIngredients;
    }

    public float GetGarbanzoProgress()
    {
        return (float)garbanzoBeanCount / requiredGarbanzoBeans;
    }

    public int GetCollectedCount()
    {
        return collectedIngredients.Count;
    }

    public int GetRequiredCount()
    {
        return requiredIngredients;
    }

    public int GetGarbanzoBeanCount()
    {
        return garbanzoBeanCount;
    }

    public int GetRequiredGarbanzoBeanCount()
    {
        return requiredGarbanzoBeans;
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

            // Show garbanzo bean progress
            if (selectedIngredients.Contains("Garbonzo"))
            {
                string garbanzoStatus = garbanzoComplete ? " COMPLETE" : "IN PROGRESS";
                GUI.Label(new Rect(10, 70, 400, 30),
                    $"Garbonzo Beans: {garbanzoBeanCount}/{requiredGarbanzoBeans} ({GetGarbanzoProgress() * 100:F0}%) {garbanzoStatus}");
            }

            // Show collected ingredients
            int yPos = 100;
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