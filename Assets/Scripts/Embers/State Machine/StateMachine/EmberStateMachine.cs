using Unity.VisualScripting;
using UnityEngine;

public class EmberStateMachine
{
    public EmberState CurrentEmberState { get; set; }

    public void Initialize(EmberState StartingState)
    {
        CurrentEmberState = StartingState;
        CurrentEmberState.EnterState();
    }

    public void ChangeState(EmberState NewState)
    {
        CurrentEmberState.ExitState();
        CurrentEmberState = NewState;
        CurrentEmberState.EnterState();
        

    }



}
