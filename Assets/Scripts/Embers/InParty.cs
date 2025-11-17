using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InParty : MonoBehaviour
{
    
    
        //if an ember entersthe following _state add them to this list
  public List <GameObject> InCurrentParty;

   public void Start()
    {
        InCurrentParty = new List <GameObject> ();
    }

    internal void AddToParty(GameObject newIngredient)
    {
        InCurrentParty.Add (newIngredient);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
