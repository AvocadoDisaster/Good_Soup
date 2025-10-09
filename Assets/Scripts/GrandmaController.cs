using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GrandmaController : MonoBehaviour
{
    #region Sprite
    [SerializeField] private SpriteRenderer spriteRenderer;
    #endregion
    #region Input Action Asset
    [SerializeField] private Grandma_Act grandmaact;
    //The different actions
    private InputAction _movement;
   
   
    private InputAction _rally;
    //Note: also name things better  and the controls scheme
    #endregion

    #region Movment fields
    private Rigidbody playerbody;
    [SerializeField]
    private float movementForce;
    [SerializeField]
    private float maxSpeed = 20f;
    private Vector3 forceDirection = Vector3.zero;
    #endregion

    #region Camera
    [SerializeField] private Camera playerCam;

    #endregion

    void Awake()
    {

        playerbody = this.GetComponent<Rigidbody>();
        grandmaact = new Grandma_Act();

    }

    private void OnEnable()
    {
        _movement = grandmaact.Grandma.Movement;
        _movement.Enable();

       

        grandmaact.Grandma.Rally.started += DoRally;
        grandmaact.Grandma.Rally.Enable();


        
    }


    private void OnDisable()
    {
        _movement.Disable();
        
        grandmaact.Grandma.Rally.Disable();
        
    }


    private void FixedUpdate()
    {
        forceDirection += _movement.ReadValue<Vector2>().x * GetCameraRight(playerCam) * movementForce;
        forceDirection += _movement.ReadValue<Vector2>().y * GetCameraForward(playerCam) * movementForce;

        playerbody.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (playerbody.linearVelocity.y < 0f)
        {
            playerbody.linearVelocity += Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
        }
        Vector3 horizontalVelocity = playerbody.linearVelocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            playerbody.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * playerbody.linearVelocity.y;
        }
        flipping();
        
    }
    
    private void DoRally(InputAction.CallbackContext context)
    {
        print("embers are being rallied");
    }
    private void flipping()
    {
        print("aiming embers");
        Vector3 direction = playerbody.linearVelocity;
        direction.y = 0;
        float lol = direction.x;
        if (_movement.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            

            if (_movement.ReadValue<Vector2>().x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (_movement.ReadValue<Vector2>().x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                //idle sprite
            }

        }
        else
        {
            playerbody.angularVelocity = Vector3.zero;
        }

    }


    private Vector3 GetCameraRight(Camera playerCam)
    {
        Vector3 Right = playerCam.transform.right;
        Right.y = 0;
        return Right.normalized;
    }

    private Vector3 GetCameraForward(Camera playerCam)
    {
        Vector3 forward = playerCam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }





}
