using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GrandmaController : MonoBehaviour
{
    [SerializeField] private float speed;
    PlayerInput playerinput;
    InputAction Movement;
    InputAction Lobbing;
    InputAction Rally;
    InputAction Aiming;
    Vector2 currentMovement;



    private void Awake()
    {
        playerinput = GetComponent<PlayerInput>();
        Movement = playerinput.actions.FindAction("Movement");
    }
    private void Update()
    {
        
    }
    void MovePlayer()
    {
        Vector2 direction = Movement.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * Time.deltaTime * Time.deltaTime;
        transform.position = transform.position.normalized;
    }



}
