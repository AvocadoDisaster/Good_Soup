using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EmberBase : MonoBehaviour, IDamageable, IMovable, ItriggerCheckables
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    public float currentHealth { get; set; }
    public NavMeshAgent agent { get; set; }
    

    #region State Machine Variables
    public EmberStateMachine EmberStateMachine { get; set; }
    public Thrown ThrownState { get; set; }
    public Idle IdleState { get; set; }
    public Following FollowingState { get; set; }
    public Carry CarryState { get; set; }

    #region movement/directionfacing
    public bool IsFacingRight { get; set; } = true;
    public bool IsFacingbakcRight { get; set; } = false;
    #endregion

    #region Carrytriggers
    public bool detectsIngredient { get ; set; }
    public bool isSlapped { get; set; }
     
    #endregion

    #region IdleVariables
    public float movementRange = 10f;
    public float movementSpeed = 5f;
    #endregion

    #region Thrown
    public bool isThrown { get; set; }
    #endregion

    #endregion


    private void Awake()
    {
        EmberStateMachine = new EmberStateMachine();

        IdleState = new Idle(this, EmberStateMachine);
        ThrownState = new Thrown(this, EmberStateMachine);
        FollowingState = new Following(this, EmberStateMachine);
        CarryState = new Carry(this, EmberStateMachine);
    }
    private void Start()
    {
        currentHealth = MaxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        EmberStateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        EmberStateMachine.CurrentEmberState.FrameUpdate();
    }

    private void FixedUpdate()
    {
       EmberStateMachine.CurrentEmberState.PhysicsUpdate();
    }

    #region Health/Die
    public void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Movement function
    public void MoveEmber(Vector3 Movement)
    {
        agent.destination = Movement;
        CheckForLeftorRichtFacing(agent);
        CheckForBackLeftorRichtFacing(agent);
    }

    public void CheckForLeftorRichtFacing(NavMeshAgent agent)
    {
        if (IsFacingRight && agent.velocity.x < 0)
        {
            spriteRenderer.flipX = true;

            IsFacingRight = !IsFacingRight;
        }
        if (!IsFacingRight && agent.velocity.y > 0)
        {
            spriteRenderer.flipX = false;

            IsFacingRight = !IsFacingRight;

        }

    }

    public void CheckForBackLeftorRichtFacing(NavMeshAgent agent)
    {
        if (IsFacingRight && agent.velocity.x < 0)
        {
            spriteRenderer.flipX = true;
            if (IsFacingbakcRight && agent.velocity.z < 0)
            {
                spriteRenderer.flipY = true;
                IsFacingbakcRight = !IsFacingbakcRight;
            }

        }
        if (!IsFacingRight && agent.velocity.y > 0)
        {
            spriteRenderer.flipX = false;
            if (!IsFacingbakcRight && agent.velocity.z > 0)
            {
                spriteRenderer.flipY = false;
                IsFacingbakcRight = !IsFacingbakcRight;
            }


        }
    }
    #endregion

    #region CarryTriggers

    public void SetIngredientDetectionStatus(bool DetectsIngredeint)
    {
        detectsIngredient = DetectsIngredeint;
    }

    public void SetIsSlappedStatus(bool IsSlapped)
    {
        isSlapped = IsSlapped;
    }
    #endregion

    #region Throwing
    public void SetIsThrownStatus(bool IsThrown)
    {
        
    }
    #endregion

    #region Animation triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {

        EmberStateMachine.CurrentEmberState.AnimationTriggerEvent(triggerType);
        
    }

    

    public enum AnimationTriggerType
    {
        PlayFootstepSound
    }
    #endregion
}

