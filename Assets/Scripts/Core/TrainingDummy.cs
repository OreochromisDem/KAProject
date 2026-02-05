using UnityEngine;
using System.Collections;

// Exige que tenha HealthComponent e Rigidbody para funcionar
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(Rigidbody))]
public class TrainingDummy : MonoBehaviour
{
    private HealthComponent _health;
    private Rigidbody _rb;
    private Renderer _renderer;
    private Color _originalColor;

    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        
        if (_renderer != null) 
            _originalColor = _renderer.material.color;
    }

    private void OnEnable()
    {
        // Conecta os ouvidos nos eventos do componente de vida
        _health.OnDamageTaken.AddListener(HandleDamageVisuals);
        _health.OnKnockback.AddListener(HandleKnockback);
        _health.OnDeath.AddListener(HandleDeath);
    }

    private void OnDisable()
    {
        // Desconecta para evitar erros
        _health.OnDamageTaken.RemoveListener(HandleDamageVisuals);
        _health.OnKnockback.RemoveListener(HandleKnockback);
        _health.OnDeath.RemoveListener(HandleDeath);
    }

    // --- REAÇÕES ---

    // Reação 1: Feedback Visual (Piscar)
    private void HandleDamageVisuals(float amount)
    {
        if (_renderer != null) StartCoroutine(FlashColor());
    }

    // Reação 2: Física (Voar longe)
    private void HandleKnockback(Vector3 direction, float force,float stunDuration)
    {
        // Reseta a velocidade atual para o impacto ser consistente
        _rb.linearVelocity = Vector3.zero; 
        
        // Aplica o empurrão
        _rb.AddForce(direction * force, ForceMode.Impulse);
    }

    // Reação 3: Morte (Sumir ou Desativar)
    private void HandleDeath()
    {
        Debug.Log("Dummy foi destruído!");
        // Opcional: Tocar partículas de explosão
        // Destroy(gameObject, 0.5f); // Destroi após meio segundo
        
        // Para teste, apenas desativa o colisor e tomba
        GetComponent<Collider>().enabled = false;
        transform.Rotate(90, 0, 0);
    }

    private IEnumerator FlashColor()
    {
        _renderer.material.color = Color.white; // Flash Branco
        yield return new WaitForSeconds(0.1f);
        _renderer.material.color = _originalColor;
    }
}