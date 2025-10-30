using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NUnit.Framework;

public class OnIngredient : MonoBehaviour
{
    // first get the ingredeint that this script is attached too
    //then any ember that collides with the ingredient will be added to teh list
    //the ingredeint will beocome the child of the first ember
    

    //if an ember entersthe following _state add them to this list
    public List<GameObject> OnAnIgredeint;

    public void Start()
    {
        OnAnIgredeint = new List<GameObject>();
    }
}
