using System.Collections.Generic;
using UnityEngine;

public enum EmberState
{
    FREE,
    IN_RALLY_PARTY,
    BEING_THROWN,
    ON_INGREDIENT,
    ON_CHARCOAL,
    ON_GARBANZO_BEANS
}

/// <summary>
/// Singleton manager that tracks the state of all embers in the game.
/// Prevents embers from being in multiple lists simultaneously.
/// </summary>
public class EmberStateManager : MonoBehaviour
{
    private static EmberStateManager _instance;
    public static EmberStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<EmberStateManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EmberStateManager");
                    _instance = go.AddComponent<EmberStateManager>();
                }
            }
            return _instance;
        }
    }

    private Dictionary<GameObject, EmberState> emberStates = new Dictionary<GameObject, EmberState>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Duplicate EmberStateManager found! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("<color=green>✓ EmberStateManager initialized</color>");
    }

    private void Update()
    {
        // DEBUG: Press M to show all ember states
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log($"<color=cyan>=== EMBER STATE MANAGER ({emberStates.Count} embers) ===</color>");
            foreach (var kvp in emberStates)
            {
                if (kvp.Key != null)
                {
                    Debug.Log($"  {kvp.Key.name}: {kvp.Value}");
                }
            }
        }
    }

    /// <summary>
    /// Register an ember with the manager. Call this in the ember's Start() method.
    /// </summary>
    public void RegisterEmber(GameObject ember)
    {
        if (ember == null)
        {
            Debug.LogWarning("Attempted to register null ember!");
            return;
        }

        if (!emberStates.ContainsKey(ember))
        {
            emberStates[ember] = EmberState.FREE;
            Debug.Log($"Registered ember: {ember.name} (ID: {ember.GetInstanceID()})");
        }
        else
        {
            Debug.LogWarning($"Ember {ember.name} (ID: {ember.GetInstanceID()}) already registered! Current state: {emberStates[ember]}");
        }
    }

    /// <summary>
    /// Get the current state of an ember.
    /// </summary>
    public EmberState GetEmberState(GameObject ember)
    {
        if (ember == null)
        {
            Debug.LogWarning("GetEmberState called with null ember!");
            return EmberState.FREE;
        }

        if (emberStates.ContainsKey(ember))
        {
            return emberStates[ember];
        }
        Debug.LogWarning($"Ember {ember.name} not registered with EmberStateManager! Auto-registering...");
        RegisterEmber(ember);
        return EmberState.FREE;
    }

    /// <summary>
    /// Try to claim an ember for a specific purpose. Returns false if already claimed by ANOTHER system.
    /// Allows transitions within the same ownership (e.g. IN_RALLY_PARTY → BEING_THROWN).
    /// </summary>
    public bool TryClaimEmber(GameObject ember, EmberState newState)
    {
        if (ember == null)
        {
            Debug.LogError("<color=red> TryClaimEmber called with NULL ember!</color>");
            return false;
        }

        if (!emberStates.ContainsKey(ember))
        {
            Debug.LogWarning($"<color=yellow>⚠ Ember {ember.name} not registered! Registering now...</color>");
            RegisterEmber(ember);
        }

        EmberState currentState = emberStates[ember];

        Debug.Log($"<color=cyan>TryClaimEmber: {ember.name}</color>");
        Debug.Log($"  Current State: {currentState}");
        Debug.Log($"  Requested State: {newState}");

        // Allow these state transitions:
        // FREE → anything
        // IN_RALLY_PARTY → BEING_THROWN (player owns both)
        // BEING_THROWN → ON_INGREDIENT/ON_CHARCOAL/ON_GARBANZO_BEANS (landed on something)
        // ON_CHARCOAL → IN_RALLY_PARTY (rally claims ember carrying charcoal)
        // ON_GARBANZO_BEANS → IN_RALLY_PARTY (rally claims ember carrying beans)
        // ON_INGREDIENT → IN_RALLY_PARTY (rally claims ember carrying ingredient)

        bool canTransition = false;

        if (currentState == EmberState.FREE)
        {
            canTransition = true;
        }
        else if (currentState == EmberState.IN_RALLY_PARTY && newState == EmberState.BEING_THROWN)
        {
            canTransition = true; // Player is throwing from their party
        }
        else if (currentState == EmberState.BEING_THROWN &&
                 (newState == EmberState.ON_INGREDIENT ||
                  newState == EmberState.ON_CHARCOAL ||
                  newState == EmberState.ON_GARBANZO_BEANS))
        {
            canTransition = true; // Thrown ember landed on something
        }
        else if ((currentState == EmberState.ON_CHARCOAL ||
                  currentState == EmberState.ON_GARBANZO_BEANS ||
                  currentState == EmberState.ON_INGREDIENT) &&
                 newState == EmberState.IN_RALLY_PARTY)
        {
            canTransition = true; // Rally can claim embers that are carrying things
            Debug.Log($"<color=magenta>Rally claiming ember from {currentState} state</color>");
        }

        if (canTransition)
        {
            emberStates[ember] = newState;
            Debug.Log($"<color=green>✓ SUCCESS! {ember.name}: {currentState} → {newState}</color>");
            return true;
        }

        Debug.Log($"<color=red> FAILED! {ember.name} cannot transition from {currentState} to {newState}</color>");
        return false;
    }

    /// <summary>
    /// Release an ember back to FREE state.
    /// </summary>
    public void ReleaseEmber(GameObject ember)
    {
        if (ember == null)
        {
            Debug.LogWarning("ReleaseEmber called with null ember!");
            return;
        }

        if (emberStates.ContainsKey(ember))
        {
            EmberState oldState = emberStates[ember];
            emberStates[ember] = EmberState.FREE;
            Debug.Log($"Ember {ember.name}: {oldState} → FREE");
        }
        else
        {
            Debug.LogWarning($"Attempted to release unregistered ember: {ember.name}");
        }
    }

    /// <summary>
    /// Force set an ember's state (use sparingly, prefer TryClaimEmber).
    /// </summary>
    public void SetEmberState(GameObject ember, EmberState newState)
    {
        if (ember == null)
        {
            Debug.LogWarning("SetEmberState called with null ember!");
            return;
        }

        if (emberStates.ContainsKey(ember))
        {
            emberStates[ember] = newState;
            Debug.Log($"Ember {ember.name} state forced to: {newState}");
        }
    }
}