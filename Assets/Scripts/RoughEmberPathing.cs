using UnityEngine;
using UnityEngine.AI;

public class RoughEmberPathing : MonoBehaviour
{
    public NavMeshAgent Ember;
    public Transform Spoon;
    private Rigidbody body;
    public Transform aim;
  

   
    

    void Start()
    {
        Ember = GetComponent<NavMeshAgent>();
       body = GetComponent<Rigidbody>();
       
        Ember.destination = Spoon.position;

    }
     void Update()
    {
        if (Ember.enabled == false)
        {
            print("Beinf Carried");
        }
       // else if()
        //{
        //    Ember.destination = aim.position;
       //}
        else
        {
            Ember.destination = Spoon.position;

        }
        
    }
}
