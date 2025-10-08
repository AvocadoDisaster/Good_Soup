using UnityEngine;

public class IngredentCheck : MonoBehaviour
{
    public GameObject IngredeintTarget { get; set; }
    private EmberBase _ember;
    private Transform Ingredent;
    private Transform _embers;
    private int ingredientChildren;

    private void Awake()
    {
        IngredeintTarget = GameObject.FindGameObjectWithTag("Ingredient");
        _ember = GetComponent<EmberBase>();
        _embers = GetComponent<Transform>();
        Ingredent = GetComponent<Transform>();

        
    }
    private void Start()
    {
        int ingredientChildren = Ingredent.childCount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == IngredeintTarget)
        {
            _ember.SetIngredientDetectionStatus(true);

            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == IngredeintTarget && ingredientChildren >= 3)
            {
            _ember.SetIngredientDetectionStatus(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _ember.SetIngredientDetectionStatus(false);
    }
}
