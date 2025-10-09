using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Carry : EmberState
{
    private Transform _Pot;
    private float _MovementSpeed ;
    private Transform Ingredent;
    private float slowSpeed = 3;
    private float fastSpeed = 9;
    private float mediumSpeed = 6;
    

    public Carry(EmberBase ember, EmberStateMachine emberStateMachine) : base(ember, emberStateMachine)
    {
        _Pot = GameObject.FindGameObjectWithTag("Pot").transform;

        Ingredent = GameObject.FindGameObjectWithTag("Ingredient").transform;

        int ingredientChildren = Ingredent.childCount;
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
        Vector2 _MovementDirection = (_Pot.position - ember.transform.position).normalized;
        if (Ingredent.childCount >= 3 && Ingredent.childCount <= 5 && ember.isTransportingIngredient)
        {
            ember.MoveEmber(_MovementDirection * slowSpeed);
        }
        else if ( Ingredent.childCount >=8 && ember.isTransportingIngredient)
        {
            ember.MoveEmber(_MovementDirection *mediumSpeed);
        }
        else if (Ingredent.childCount >=9 && ember.isTransportingIngredient)
        {
            ember.MoveEmber(_MovementDirection * fastSpeed);
        }
        else
        {
            ember.MoveEmber(_MovementDirection * 0);
        }

        if(ember.isRallied)
        {
            ember.EmberStateMachine.ChangeState(ember.FollowingState);
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
