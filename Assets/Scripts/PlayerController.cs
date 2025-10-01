using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody playerbody;
    public float movespeed;
    private Vector2 moveInput;
    public SpriteRenderer grandma;
    public Animator anim;

    private bool moveingback;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        moveInput.Normalize();

        playerbody.linearVelocity = new Vector3(moveInput.x * movespeed, playerbody.linearVelocity.y, moveInput.y * movespeed );

        if (!grandma.flipX && moveInput.x < 0)
        {
            grandma.flipX = true;
        }
        else if (grandma.flipX && moveInput.x >0)
        {
            grandma.flipX= false;
        }

        if (!moveingback && moveInput.y < 0)
        {
            moveingback = true;

        }

        else if (moveingback && moveInput.y > 0)
        {
            moveingback = false;
        }
    }

}
