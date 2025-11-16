using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public Vector2 MovementInputVector {  get; private set; }
   private void Movement(InputValue inputValue)
    {
        MovementInputVector = inputValue.Get<Vector2>();
    }
}
