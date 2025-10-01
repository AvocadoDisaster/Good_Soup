using UnityEngine;
using UnityEngine.AI;
using UnityEngine.WSA;

public class Throwable : MonoBehaviour
{


    public Transform PickUpPoint;
    public Transform Grandma;
    public GameObject target;
    [SerializeField] private float pickUpDistance;
    public Rigidbody spoonrigid;
    [SerializeField]private float force = 50;
    

    public NavMeshAgent Ember;

    public bool Inposition;
    public bool EmberisPicked;
    public bool catapulted;

    public Rigidbody body;
    
    

    void Start()
    {
        body = GetComponent<Rigidbody>();
        Grandma = GameObject.Find("Grandma").transform;
        PickUpPoint = GameObject.Find("Spoon").transform;
        Ember = GetComponent<NavMeshAgent>();
        spoonrigid = GetComponent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.J) && EmberisPicked == true && Inposition)
        {
            

        }
        pickUpDistance = Vector3.Distance(Grandma.position, transform.position);

        if (pickUpDistance <= 7)
        {
            if (Input.GetKeyDown(KeyCode.J) && EmberisPicked == false && PickUpPoint.childCount < 1)
            {
                OnlyOne();
                Ember.enabled = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.J) && EmberisPicked)
        {
            

            Throwing(target);
            Ember.enabled = true;

            
        }
    }

    private void Throwing( GameObject target)
        {
        if (Inposition)
        {

            Vector3 arch = new Vector3(Ember.transform.position.x, Ember.transform.position.y, Ember.transform.position.z * force * Time.deltaTime);
            spoonrigid.AddTorque(arch);
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<BoxCollider>().enabled = true;

            this.transform.parent = null;
            

            EmberisPicked = false;
            
            Inposition = false;
            catapulted = true;

           
           
           
            print("Ember is Thrown");
        }
    }

    private void OnlyOne()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<BoxCollider>().enabled = false;
        this.transform.position = PickUpPoint.position;
        this.transform.parent = GameObject.Find("Spoon").transform;
        
        EmberisPicked = true;
        
        Inposition = true;

    }
    
}


