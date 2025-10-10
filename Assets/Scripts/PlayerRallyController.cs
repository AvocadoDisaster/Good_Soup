using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerRallyController : MonoBehaviour
{
    public float scale = 0.1f;
    public Collider RallyCaller;
   

    private void Awake()
    {
        RallyCaller = GetComponent<Collider>();
       
    }

    private void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.Space))
        {
           transform.localScale = transform.localScale + new Vector3(scale,scale,scale);
          // for (int i = 0; i<0; i++)
          //  {
            //    SoundManager.Playsound(Soundtype.grannydemo6);
            //    i++;
           /// }
           
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            transform.localScale = new Vector3(0,0,0);
        }
        else
            transform.localScale = new Vector3(0, 0, 0);
    }

}