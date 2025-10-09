using System.Runtime.CompilerServices;
using UnityEngine;

public class IdleChecker : MonoBehaviour
{
    private EmberBase _ember;
    public GameObject Grandma {get; set;}
    public GameObject RallyCaller {get; set;}

    private void Awake()
    {
        _ember = new EmberBase();
        Grandma = GameObject.FindGameObjectWithTag("Player");
        RallyCaller = GameObject.FindGameObjectWithTag("RallyCaller");
    }
    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == Grandma)
        {
            _ember.isSlapped = true;
        }
        else
        {
            _ember.isSlapped = false;
        }

        if (other.gameObject == RallyCaller)
        {
            _ember.isRallied = true;
        }
        else
        {
            _ember.isRallied= false;
        }
        if (_ember.detectsIngredient)
        {
            _ember.isRallied = false;
            _ember.isSlapped = false;
        }
    }
}
