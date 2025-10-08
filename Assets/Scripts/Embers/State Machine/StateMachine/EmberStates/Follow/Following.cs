using UnityEngine;

public class Following : EmberState
{
    private Transform _playerTransform;
    private float _MovementSpeed = 5.7f;
    public Following(EmberBase ember, EmberStateMachine emberStateMachine) : base(ember, emberStateMachine)
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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
        Vector2 _MovementDirection = (_playerTransform.position - ember.transform.position).normalized;
        ember.MoveEmber(_MovementDirection *  _MovementSpeed);

       // if(ember.IsThrown)
       // {
       //     ember.EmberStateMachine.ChangeState(ember.ThrownState);
        //}
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
