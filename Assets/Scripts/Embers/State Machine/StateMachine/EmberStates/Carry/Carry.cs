using UnityEngine;

public class Carry : EmberState
{
    public Carry(EmberBase ember, EmberStateMachine emberStateMachine) : base(ember, emberStateMachine)
    {
    }

    public override void AnimationTriggerEvent(EmberBase.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
