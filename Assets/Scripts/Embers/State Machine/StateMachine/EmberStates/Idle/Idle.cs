using System;
using UnityEngine;

public class Idle : EmberState
{
    private Vector3 _targetPos;
    private Vector3 _direction;
    public Idle(EmberBase ember, EmberStateMachine emberStateMachine) : base(ember, emberStateMachine)
    {
    }

    public override void AnimationTriggerEvent(EmberBase.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
        _targetPos = GetRandomPointInCricle();
    }

   

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        _direction = (_targetPos - ember.transform.position).normalized;
        ember.MoveEmber(_direction * ember.movementSpeed * Time.deltaTime);

        if((ember.transform.position - _targetPos).sqrMagnitude < 0.01f)
        {
            _targetPos = GetRandomPointInCricle();
        }

        if (ember.detectsIngredient)
        {
            ember.EmberStateMachine.ChangeState(ember.CarryState);
        }

        if(ember.isSlapped)
        {
            ember.EmberStateMachine.ChangeState(ember.FollowingState);
        }

        if (ember.isRallied)
        {
            ember.EmberStateMachine.ChangeState(ember.FollowingState);
        }
       

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private Vector3 GetRandomPointInCricle()
    {
        return ember.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * ember.movementRange;
    }
}
