using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Comparers;
using UnityEngine.InputSystem;

public class projectile : MonoBehaviour
{
    [SerializeField] float _InitialVelocity;
    [SerializeField] float _Angle;
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] float _Step;
    [SerializeField] float _Height;
    [SerializeField] Transform _FirePoint;
    [SerializeField]private Camera _cam;
    [SerializeField] Transform RallyCaller;

    private GameObject heldobj;
    private Rigidbody heldObjectRB;
    //physics perameters for pick up
    [SerializeField] private float pickupRange = 5.0f;
    [SerializeField] private float pickupForce = 150.0f;

    private void Start()
    {
        _cam = Camera.main; 
    }
    private void Update()
    {
      Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 direction =hit.point - _FirePoint.position;
            Vector3 groundDirection = new Vector3(direction.x, 0, direction.y);

            Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);
            
            float height = targetPos.y = targetPos.magnitude / 2f;
            height = Mathf.Max(0.01f, height);
            float angle;
            float v0;
            float time;
            CalculatePathWithHeight(targetPos, height, out angle, out v0, out time);
            DrawPath(groundDirection.normalized, v0, angle, time, _Step);
            

            if (Input.GetKeyDown(KeyCode.J))
            {
                if(heldobj == null)
                {
                    RaycastHit hitting;
                    if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitting, pickupRange))
                    {
                        pickupObject(hitting.transform.gameObject);
                    }
                }
            }
            if (heldobj != null)
            {
                moveObj();
                if (Input.GetKeyUp(KeyCode.J))
                {

                    StopAllCoroutines();
                    StartCoroutine(Coroutine_Movment(groundDirection.normalized,v0, angle, time));
                    dropObject();
                }
            }
        }
    }

    private float QuadraticEquation(float a, float b, float c, float sighn)
    {
        return (-b + sighn * Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
    }
    private void CalculatePathWithHeight(Vector3 targetPos, float h, out float angle, out float v0, out float time)
    {
        float xt = targetPos.x;
        float yt = targetPos.y;
        float g = -Physics.gravity.y;

        float b = Mathf.Sqrt(2 * g * h);
        float a = (-0.5f * g);
        float c  = -yt;

        float tplus = QuadraticEquation(a, b, c, 1);
        float tmin = QuadraticEquation(a, b, c, -1);
        time = tplus > tmin? tplus : tmin;  
        angle = Mathf.Atan(b* time /xt);
        v0 = b /Mathf.Sin(angle);

    }
    private void CalculatePath(Vector3 targetPos, float angle, out float v0, out float time)
    {
        float xt = targetPos.x;
        float yt = targetPos.y;
        float g = -Physics.gravity.y;

        float v1 = Mathf.Pow(xt, 2) * g;
        float v2 = 2 * xt * Mathf.Sin(angle) * Mathf.Cos(angle);
        float v3 = 2* yt * Mathf.Pow(Mathf.Cos(angle), 2);
        v0 = Mathf.Sqrt(v1/(v2-v3));   
        
        time = xt / (v0 * Mathf.Cos(angle));
    }
    private void DrawPath(Vector3 direction,float v0, float angle, float time, float step)
    {
        
        step = Mathf.Max(0.01f, step);
        _lineRenderer.positionCount = (int)(time / step) + 2;
        int count = 0;
        for (float i =0; i < time; i +=step)
        {
            float x= v0 * i * Mathf.Cos(angle);
            float y = v0 * i * Mathf.Sin(angle)  -0.5f * - Physics.gravity.y * Mathf.Pow(i, 2);
            _lineRenderer.SetPosition(count, _FirePoint.position + direction * x + Vector3.up * y);
            count++;

        }
        float xfinal = v0 * time * Mathf.Cos(angle);
        float yfinal = v0 * time *Mathf.Sin(angle) - 0.5f * -Physics.gravity.y *Mathf.Pow(time,2);
        _lineRenderer.SetPosition(count, _FirePoint.position + direction * xfinal + Vector3.up * yfinal);
         
        


    }
    IEnumerator Coroutine_Movment(Vector3 direction,float v0, float angle, float time)
    {
        float t = 0;
        while(t <time) 
        {
            float x = v0 *t *Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - (1f / 2f) * -Physics.gravity.y * Mathf.Pow(t, 2);
            transform.position = _FirePoint.position + direction * x + Vector3.up *y;
            t += Time.deltaTime;
            yield return null;
        }
    }
   
    void pickupObject(GameObject pickObj)
    {
        if(pickObj.GetComponent<Rigidbody>())
        {
            heldObjectRB = pickObj.GetComponent<Rigidbody>();
            heldObjectRB.useGravity = false;
            heldObjectRB.linearDamping = 10f;
            
            heldObjectRB.transform.parent = _FirePoint;
            heldobj = pickObj;
        }
    }
    void dropObject()
    {
        
            
            heldObjectRB.useGravity = true;
            heldObjectRB.linearDamping = 1f;

            heldObjectRB.transform.parent = null;
        heldobj = null ;
        
    }

    void moveObj()
    {
        if (Vector3.Distance(heldobj.transform.position, _FirePoint.position) > 0.1f)
        {
            Vector3 movedDirection = (_FirePoint.position - heldobj.transform.position).normalized;
            heldObjectRB.AddForce(movedDirection * pickupForce, ForceMode.Impulse);
        }
    }
}
