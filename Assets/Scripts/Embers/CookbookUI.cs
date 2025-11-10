using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CookbookUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PotController potController;
    [SerializeField] private InParty inParty;
    [SerializeField] private GameObject cookbookPanel;

    [Header("Ingredient Display")]
    [SerializeField] private Transform ingredientListContainer;
    [SerializeField] private GameObject ingredientItemPrefab;

    [Header("Ember Counter")]
    [SerializeField] private TextMeshProUGUI ralliedEmbersText;
    [SerializeField] private TextMeshProUGUI worldEmbersText;
    [SerializeField] private TextMeshProUGUI totalEmbersText;
    [SerializeField] private GameObject emberIconPrefab;
    [SerializeField] private Transform ralliedEmbersContainer;
    [SerializeField] private int maxEmberIconsToShow = 10;

    [Header("Progress Bar")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool startOpen = false;
    [SerializeField] private Color collectedColor = Color.green;
    [SerializeField] private Color missingColor = Color.gray;

    private Dictionary<string, IngredientUIItem> ingredientItems = new Dictionary<string, IngredientUIItem>();
    private List<GameObject> emberIcons = new List<GameObject>();
    private int totalEmbersInWorld = 0;

    private class IngredientUIItem
    {
        public GameObject gameObject;
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public Image checkmark;
    }

    private void Start()
    {
        

        if (cookbookPanel != null)
        {
            cookbookPanel.SetActive(startOpen);
        }

        InitializeCookbook();
        CountTotalEmbers();

        Debug.Log("<color=cyan>✓ Cookbook UI initialized</color>");
    }

    private void Update()
    {
        // Toggle cookbook
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleCookbook();
        }

        // Update ember counts
        UpdateEmberDisplay();

        // Update progress
        UpdateProgress();
    }

    private void InitializeCookbook()
    {
        if (potController == null || ingredientListContainer == null) return;

        // Clear existing items
        foreach (Transform child in ingredientListContainer)
        {
            Destroy(child.gameObject);
        }
        ingredientItems.Clear();

        Debug.Log("<color=cyan>=== INITIALIZING COOKBOOK ===</color>");

        // Get required ingredients from pot
        HashSet<string> selectedIngredients = GetSelectedIngredients();

        foreach (string ingredientName in selectedIngredients)
        {
            CreateIngredientItem(ingredientName);
        }

        Debug.Log($"<color=green>✓ Created {ingredientItems.Count} ingredient items</color>");
    }

    private HashSet<string> GetSelectedIngredients()
    {
        // Use reflection to access private field
        var field = typeof(PotController).GetField("selectedIngredients",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (HashSet<string>)field.GetValue(potController);
        }

        Debug.LogError("<color=red>Cannot access selectedIngredients from PotController!</color>");
        return new HashSet<string>();
    }

    private HashSet<string> GetCollectedIngredients()
    {
        var field = typeof(PotController).GetField("collectedIngredients",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (HashSet<string>)field.GetValue(potController);
        }

        return new HashSet<string>();
    }

    private void CreateIngredientItem(string ingredientName)
    {
        if (ingredientItemPrefab == null)
        {
            Debug.LogError("<color=red>Ingredient Item Prefab not assigned!</color>");
            return;
        }

        GameObject item = Instantiate(ingredientItemPrefab, ingredientListContainer);
        item.name = $"Ingredient_{ingredientName}";

        IngredientUIItem uiItem = new IngredientUIItem
        {
            gameObject = item,
            iconImage = item.transform.Find("Icon")?.GetComponent<Image>(),
            nameText = item.transform.Find("Name")?.GetComponent<TextMeshProUGUI>(),
            checkmark = item.transform.Find("Checkmark")?.GetComponent<Image>()
        };

        if (uiItem.nameText != null)
        {
            uiItem.nameText.text = ingredientName;
        }

        if (uiItem.checkmark != null)
        {
            uiItem.checkmark.gameObject.SetActive(false);
        }

        // Set initial color
        UpdateIngredientItemColor(uiItem, false);

        ingredientItems[ingredientName] = uiItem;
    }

    private void UpdateIngredientItemColor(IngredientUIItem item, bool collected)
    {
        Color targetColor = collected ? collectedColor : missingColor;

        if (item.nameText != null)
        {
            item.nameText.color = targetColor;
        }

        if (item.iconImage != null)
        {
            item.iconImage.color = targetColor;
        }

        if (item.checkmark != null)
        {
            item.checkmark.gameObject.SetActive(collected);
        }
    }

    public void UpdateIngredientStatus()
    {
        HashSet<string> collected = GetCollectedIngredients();

        foreach (var kvp in ingredientItems)
        {
            bool isCollected = collected.Contains(kvp.Key);
            UpdateIngredientItemColor(kvp.Value, isCollected);
        }
    }

    private void UpdateEmberDisplay()
    {
        int ralliedCount = inParty != null ? inParty.InCurrentParty.Count : 0;
        int worldCount = totalEmbersInWorld - ralliedCount;

        // Update text displays
        if (ralliedEmbersText != null)
        {
            ralliedEmbersText.text = $"Party: {ralliedCount}";
        }

        if (worldEmbersText != null)
        {
            worldEmbersText.text = $"World: {worldCount}";
        }

        if (totalEmbersText != null)
        {
            totalEmbersText.text = $"Total: {totalEmbersInWorld}";
        }

        // Update ember icons
        UpdateEmberIcons(ralliedCount);

        // Auto-update ingredient status when cookbook is open
        if (cookbookPanel != null && cookbookPanel.activeSelf)
        {
            UpdateIngredientStatus();
        }
    }

    private void UpdateEmberIcons(int ralliedCount)
    {
        if (ralliedEmbersContainer == null || emberIconPrefab == null) return;

        int iconsToShow = Mathf.Min(ralliedCount, maxEmberIconsToShow);

        // Create new icons if needed
        while (emberIcons.Count < iconsToShow)
        {
            GameObject icon = Instantiate(emberIconPrefab, ralliedEmbersContainer);
            emberIcons.Add(icon);
        }

        // Show/hide icons based on count
        for (int i = 0; i < emberIcons.Count; i++)
        {
            emberIcons[i].SetActive(i < iconsToShow);
        }

        // Show "+X" text if more than max
        if (ralliedCount > maxEmberIconsToShow)
        {
            // Could add a "+X more" text here
        }
    }

    private void UpdateProgress()
    {
        if (potController == null) return;

        float progress = potController.GetProgress();
        int collected = potController.GetCollectedCount();
        int required = potController.GetRequiredCount();

        if (progressBar != null)
        {
            progressBar.value = progress;
        }

        if (progressText != null)
        {
            progressText.text = $"{collected}/{required}";
        }
    }

    private void CountTotalEmbers()
    {
        // Count all embers in the scene (using new Unity API)
        GameObject[] allEmbers = GameObject.FindGameObjectsWithTag("Ember");
        totalEmbersInWorld = allEmbers.Length;

        Debug.Log($"<color=cyan>Total embers in world: {totalEmbersInWorld}</color>");
    }

    public void ToggleCookbook()
    {
        if (cookbookPanel != null)
        {
            bool isActive = !cookbookPanel.activeSelf;
            cookbookPanel.SetActive(isActive);

            if (isActive)
            {
                UpdateIngredientStatus();
                CountTotalEmbers();
            }

            Debug.Log($"<color=yellow>Cookbook {(isActive ? "opened" : "closed")}</color>");
        }
    }

    public void OpenCookbook()
    {
        if (cookbookPanel != null && !cookbookPanel.activeSelf)
        {
            ToggleCookbook();
        }
    }

    public void CloseCookbook()
    {
        if (cookbookPanel != null && cookbookPanel.activeSelf)
        {
            ToggleCookbook();
        }
    }

    public void RefreshEmberCount()
    {
        CountTotalEmbers();
    }
}