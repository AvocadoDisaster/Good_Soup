using UnityEngine;

public class FollowCheck : MonoBehaviour
{
   public GameObject grandmaTarget { get; set; }
    public GameObject RallyCaller { get; private set; }

    private EmberBase _ember;

    private void Awake()
    {
        grandmaTarget = GameObject.FindGameObjectWithTag("Player");
        RallyCaller = GameObject.FindGameObjectWithTag("RallyCaller");

        _ember = GetComponent<EmberBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == grandmaTarget || other.gameObject == RallyCaller)
        {
            _ember.SetIsSlappedStatus(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _ember.SetIsSlappedStatus(false);
    }
}
