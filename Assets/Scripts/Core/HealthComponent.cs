using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Configuração")]
    public float MaxHealth = 100f;

    [Header("Defesa")] 
    public bool IsBlocking = false;
    
    [Range(0f,1f)]
    public float BlockDamageMultiplier = 0.3f; // 30% do dano recebido
    
    [Header("Debug")]
    public float CurrentHealth;

    [Header("Eventos")] 
    public UnityEvent OnDeath;
    public UnityEvent<float> OnDamageTaken; // Passa o dano recebido
    public UnityEvent<Vector3, float> OnKnockback; // Passa a direção e força


    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }


    public void TakeDamage(float damageAmount, Vector3 hitDirection, float knockbackForce)
    {
        if (CurrentHealth <= 0) return; // Morreu, sem ação.

        if (IsBlocking)
        {
            //Reduz o dano
            damageAmount *= BlockDamageMultiplier;
            
            
            //Reduz knockback
            knockbackForce *= 0.2f;
            
            Debug.Log($"<color=blue>BLOQUEIO! Dano reduzido para {damageAmount}</color>");
        }
        
        
        //1.Aplica o dano
        CurrentHealth -= damageAmount;
        Debug.Log($"{gameObject.name}Tomou{damageAmount}de dano. Vida:{CurrentHealth}");
        
        //2.Dispara eventos
        OnDamageTaken?.Invoke(damageAmount);
        OnKnockback?.Invoke(hitDirection, knockbackForce);
        
        //3. Checa a morte
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        CurrentHealth = 0;
        Debug.Log($"{gameObject.name} MORREU!");
        OnDeath?.Invoke();
        
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
