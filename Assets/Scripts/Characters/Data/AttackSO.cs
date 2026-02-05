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
    
    [Header("Dano")] 
    public float Damage;
    public float Knockback;

    [Tooltip("Tempo em segundos que o inimigo fica paralisado após o hit")]
    public float HitStunDuration = 0.4f;
    
    [Header("Hitbox Modular")] 
    public float HitboxRadius = 0.5f; //Tamanhoo da esfera
    public Vector3 HitboxOffset = new Vector3(0,0,1f); //Distância do corpo (Z = frente)

}
