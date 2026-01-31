using UnityEngine;

// Interface: É apenas um contrato. Não tem variáveis, só a assinatura do método.
public interface IDamageable
{
    void TakeDamage(float damageAmount,Vector3 hitDirection, float knockbackForce);
}
