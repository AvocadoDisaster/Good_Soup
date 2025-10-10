using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInputVector { get; private set; }
    private void OnMove(InputValue inputValue)
    {
        MoveInputVector = inputValue.Get<Vector2>();
    }
}
