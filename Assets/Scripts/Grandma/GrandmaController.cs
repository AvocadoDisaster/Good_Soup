using System;
using UnityEngine;
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
    private bool isMirrored = false;

    [SerializeField] private SpriteRenderer spriteRenderer;

    
    [SerializeField] private UnityEngine.KeyCode mirrorKey = UnityEngine.KeyCode.C; // Press C to flip controls

    void Update()
    {
        //  mirror when pressing C
        if (Input.GetKeyDown(mirrorKey))
        {
            isMirrored = !isMirrored;
            Debug.Log("Controls Mirrored: " + isMirrored);
        }

        // Get input from keyboard or controller (old Input system)
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.z = Input.GetAxis("Vertical");

        // If mirrored, invert the controls
        if (isMirrored)
        {
            movementInput *= -1f;
        }
    }

    void FixedUpdate()
    {
        // Apply movement forces to the Rigidbody
        Vector3 moveDirection = new Vector3(movementInput.x, 0f, movementInput.z).normalized;

        // Use Rigidbody.velocity (not linearVelocity)
        Vector3 v = rb.linearVelocity;
        v.x = moveDirection.x * moveSpeed;
        v.z = moveDirection.z * moveSpeed;
        rb.linearVelocity = v;

        // Flip sprite based on movement direction
        if (rb.linearVelocity.x < -0.1f)
        {
            spriteRenderer.flipX = true;
        }
        if (rb.linearVelocity.x > -0.1f)
        {
            spriteRenderer.flipX = false;
        }
    }

   
}
