using UnityEngine;

public class AgentTrigger : MonoBehaviour
{
    private RoughEmberPathing pathing;
   
     private void Awake()
    {
        pathing = GetComponentInParent<RoughEmberPathing>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detected on " + gameObject.name + " with " + other.gameObject.name + " (Tag: " + other.tag + ")");

        if (pathing == null)
        {
            Debug.LogError("AgentTrigger cannot find SimpleAgentLogic on parent.");
            return;
        }

        bool isSlapTrigger = gameObject.name == "Grandma";
        bool isSlapTarget = other.CompareTag("Player") || other.CompareTag("RallyCaller");
        bool isIngredientTrigger = gameObject.name == "Ingredient";
        bool isIngredientTarget = other.CompareTag("Ingredient");

        Debug.Log($"isSlapTrigger: {isSlapTrigger}, isSlapTarget: {isSlapTarget}");
        Debug.Log($"isIngredientTrigger: {isIngredientTrigger}, isIngredientTarget: {isIngredientTarget}");

        // "Slapped/Rallied" trigger check
        if (isSlapTarget)
        {
            Debug.Log("Condition passed: Slap/Rally target detected.");
            pathing.OnSlappedOrRallied(other.transform);
            
        }
        // "Ingredient" trigger check
        else if ( isIngredientTarget & !isSlapTarget)
        {
            Debug.Log("Condition passed: Ingredient detected.");
            pathing.OnIngredientDetected(other.gameObject);
            SoundManager.Playsound(Soundtype.item_Pickup);
        }
    }
}
