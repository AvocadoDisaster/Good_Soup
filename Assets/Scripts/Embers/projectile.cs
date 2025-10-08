using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class projectile : MonoBehaviour
{

    [SerializeField] float _InitialVelocity;
    [SerializeField] float _Angle;
    [SerializeField] float _Step;
    [SerializeField] private LineRenderer _Line;
    [SerializeField] private Transform _FirePoint;
    [SerializeField] private Transform grandampos;
    private float _Height;
    private Camera _cam;
    [SerializeField] GameObject Ember;
    [SerializeField] Transform embertransform;
    public GameObject RallyCaller;


    [Header("Pickup Settings")]
    [SerializeField] Transform holdArea;
    private GameObject heldObj;
    private Rigidbody heldObjRB;

    [Header("Physics Parameters")]
    [SerializeField] private float pickupRange;
    [SerializeField] private float picupForce = 150.0f;

    [SerializeField] private Grandma_Act grandmaact;
    private InputAction _aiming;
    private InputAction _throwing;
    private void Start()
    {
        _cam = Camera.main;
    }
    /*
    private void Awake()
    {
        grandmaact = new Grandma_Act();
    }
    private void OnEnable()
    {
        _aiming = grandmaact.Grandma.Aiming;
        _aiming.Enable();


        _throwing = grandmaact.Grandma.Throwing;
       
        _throwing.Enable();
        

    }


    private void OnDisable()
    {
        _aiming.Disable();
        _throwing.Disable();

        grandmaact.Grandma.Rally.Disable();

    }
    */
    void Update()
    {
        // Vector3 aimingSpoon = _aiming.ReadValue<Vector3>();
        //Vector3 trueaim = new Vector3(aimingSpoon.x, aimingSpoon.y, aimingSpoon.z).normalized;
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 direction = hit.point - _FirePoint.position;
            Vector3 groundDirection = new Vector3(direction.x, 0, direction.z);
            Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);
            float height = targetPos.y + targetPos.magnitude / 2f;
            height = Mathf.Max(0.01f, height);
            float angle;
            float v0;
            float time;
            CalculatePathWithHeight(targetPos, height, out v0, out angle, out time);
            DrawPath(groundDirection.normalized, v0, angle, time, _Step);
            RallyCaller.transform.position = _Line.GetPosition(_Line.positionCount - 1);


            if (Input.GetMouseButtonDown(0))//_throwing.WasReleasedThisFrame()
            {
                if (heldObj == null)
                {
                    RaycastHit hitting;
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hitting) & Input.GetMouseButtonUp(0))
                    {

                        PickupObject(hitting.transform.gameObject);
                        GetComponent<RoughEmberPathing>().enabled = false;
                        print("ember ispicked up");
                    }
                    else
                    {

                        StopAllCoroutines();

                        StartCoroutine(Coroutine_Movement(groundDirection.normalized, v0, angle, time));
                        DropObject();
                        print("Ember is thrown");
                    }
                }
                if (heldObj != null)
                {
                    MOveing();
                }

            }




        }
    }

    private void DrawPath(Vector3 direction, float v0, float angle, float time, float step)
    {
        step = Mathf.Max(0.01f, step);

        _Line.positionCount = (int)(time / step) + 2;
        int count = 0;

        for (float i = 0; i < time; i += step)
        {
            float x = v0 * i * Mathf.Cos(angle);
            float y = v0 * i * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(i, 2);
            _Line.SetPosition(count, _FirePoint.position + direction * x + Vector3.up * y);
            count++;
        }
        float xfinal = v0 * time * Mathf.Cos(angle);
        float yfinal = v0 * time * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(time, 2);
        _Line.SetPosition(count, _FirePoint.position + direction * xfinal + Vector3.up * yfinal);
    }

    private float QuadraticEquation(float a, float b, float c, float sighn)
    {
        return (-b + sighn * Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
    }
    private void CalculatePathWithHeight(Vector3 targetPos, float h, out float v0, out float angle, out float time)
    {
        float xt = targetPos.x;
        float yt = targetPos.y;
        float g = -Physics.gravity.y;

        float b = Mathf.Sqrt(2 * g * h);
        float a = (-0.5f * g);
        float c = -yt;

        float tplus = QuadraticEquation(a, b, c, 1);
        float tmin = QuadraticEquation(a, b, c, -1);
        time = tplus > tmin ? tplus : tmin;

        angle = Mathf.Atan(b * time / xt);

        v0 = b / Mathf.Sin(angle);
    }
    private void CalculatePath(Vector3 TargetPos, float angle, out float v0, out float time)
    {
        float xt = TargetPos.x;
        float yt = TargetPos.y;
        float g = Physics.gravity.y;

        float v1 = Mathf.Pow(xt, 2) * g;
        float v2 = 2 * xt * Mathf.Sin(angle) * Mathf.Cos(angle);
        float v3 = 2 * xt * Mathf.Pow(Mathf.Cos(angle), 2);
        v0 = Mathf.Sqrt(v1 / (v2 - v3));

        time = xt / (v0 * Mathf.Cos(angle));
    }

    IEnumerator Coroutine_Movement(Vector3 direction, float v0, float angle, float time)
    {
        float t = 0;
        while (t < time)
        {
            float x = v0 * t * Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - (1f / 2f) * -Physics.gravity.y * Mathf.Pow(t, 2);
            transform.position = _FirePoint.position + direction * x + Vector3.up * y;
            t += Time.deltaTime;
            yield return null;
        }
    }


    void PickupObject(GameObject pickObj)
    {
        if (pickObj.GetComponent<Rigidbody>())
        {
            heldObjRB = pickObj.GetComponent<Rigidbody>();
            heldObjRB.useGravity = false;
            heldObjRB.linearDamping = 10;
            heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;

            heldObjRB.transform.parent = holdArea;
            heldObj = pickObj;
        }
    }

    void DropObject()
    {


        heldObjRB.useGravity = true;
        heldObjRB.linearDamping = 1;
        heldObjRB.constraints = RigidbodyConstraints.None;

        heldObj.transform.parent = null;
        heldObj = null;

    }

    void MOveing()
    {
        if (Vector3.Distance(heldObj.transform.position, holdArea.position) > 0.1f)
        {
            Vector3 moveDirectioning = (holdArea.position - heldObj.transform.position);
            heldObjRB.AddForce(moveDirectioning * picupForce);
        }
    }

}