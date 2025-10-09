using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class IngredentCheck : MonoBehaviour
{
    public GameObject IngredeintTarget { get; set; }
    private EmberBase _ember;
    private int ingredientChildren;
    public bool transportIngredeintReady;
    public static bool transportIngredeintSlow;
    public static bool transportIngredeintMedium;
    public static bool transportIngredeintFast;


    private void Awake()
    {
        IngredeintTarget = GameObject.FindGameObjectWithTag("Ingredient");
        _ember = GetComponent<EmberBase>();
        



    }
    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == IngredeintTarget)
        {
            _ember.SetIngredientDetectionStatus(true);
            transportIngredeintReady = true;


        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == IngredeintTarget)
        {
            _ember.SetIngredientDetectionStatus(true);

            transportIngredeintReady = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _ember.SetIngredientDetectionStatus(false);
        transportIngredeintReady=false;
    }
}
