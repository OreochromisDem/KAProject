using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Combat/Attack/Attack Data")]
public class AttackSO : ScriptableObject
{
    [Header("Identidade")] 
    public string AttackName;

    [Header("Frame Data")] 
    public float StartupTime; // Preparação (Vulneravel, sem dano)
    public float ActiveTime;  // Hitbox ativa (Dá dano, cor vermelha)
    public float RecoveryTime; // Recuperação(Vulneravel,parado)

    [Header("Propriedades")] 
    public float Damage;
    public float Knockback;

}
