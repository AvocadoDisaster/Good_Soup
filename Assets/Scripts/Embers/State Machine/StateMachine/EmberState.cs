using UnityEngine;

public class EmberState
{
    protected EmberBase ember;
    protected EmberStateMachine EmberStateMachine;

    public EmberState(EmberBase ember, EmberStateMachine emberStateMachine)
    {
        this.ember = ember;
        this.EmberStateMachine = emberStateMachine;
    }

    public virtual void EnterState()
    {

    }

    public virtual void ExitState()
    {
    }

    public virtual void FrameUpdate()
    {

    }
    public virtual void PhysicsUpdate()
    {

    }

    public virtual void AnimationTriggerEvent(EmberBase.AnimationTriggerType triggerType)
    {
        
    }
}
