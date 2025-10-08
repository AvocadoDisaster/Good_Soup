using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public interface IDamageable 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Damage(float damageAmount);

    void Die();

    float MaxHealth { get; set; }
    float currentHealth { get; set; }
}
