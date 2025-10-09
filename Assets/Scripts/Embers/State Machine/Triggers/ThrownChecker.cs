
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ThrownChecker : MonoBehaviour
{
    public Transform Spoon { get; set; }
    private EmberBase _ember;
    private bool thrown = false;
    int spoonChild;
    int currentEmberCOunt;
    private void Awake()
    {
        _ember = GetComponent<EmberBase>();
        thrown = projectile.Isthrown;
        spoonChild = Spoon.childCount;
        currentEmberCOunt = spoonChild;
    }

    private void Update()
    {

        if (spoonChild < currentEmberCOunt)
        {
            _ember.SetIsThrown(true);
            currentEmberCOunt = spoonChild;
        }
        else if (spoonChild > currentEmberCOunt)
        {
                _ember.SetIsThrown(false);
                currentEmberCOunt = spoonChild;
        }
        else
        {
            _ember.SetIsThrown(false);
        }
    }
}
