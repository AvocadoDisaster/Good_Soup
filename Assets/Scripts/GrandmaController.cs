using System;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GrandmaController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8.0f;
    public float jumpForce = 15.0f;

    [Header("Dependencies")]
    public Rigidbody rb;
    public Transform spriteHolder;
    public LayerMask groundLayer;

    private Vector3 movementInput;
    

    [SerializeField] SpriteRenderer spriteRenderer;

    void Update()
    {
        // Get input from keyboard or controller
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.z = Input.GetAxis("Vertical");

        
    }

    void FixedUpdate()
    {
        // Apply movement forces to the Rigidbody
        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.z).normalized;
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        if(rb.linearVelocity.x < 0.1f)
        {
            spriteRenderer.flipX = true;
        }
        if (rb.linearVelocity.x > -0.1f)
        {
            spriteRenderer.flipX = false;
        }
    }

   
}
