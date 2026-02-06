using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float _timer;
    
    //O Enum define em qual fase do soco esstamos
    private enum AttackPhase{Startup,Active,Recovery,Finished}

    private AttackPhase _currentPhase;

    // Lista para garantir que não acertamos o mesmo inimigo 2x no mesmo soco
    private System.Collections.Generic.List<IDamageable> _hitTargets = new System.Collections.Generic.List<IDamageable>();
    
    public PlayerAttackState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {}

    
    
    
    public override void EnterState()
    {
        
        // Consome o Input Buffer imediatamente para não repetir ataque sem querer
        ctx.UseAttackInput();
        
        
        
        //Zera tudo
        _timer = 0;
        _currentPhase = AttackPhase.Startup;
        _hitTargets.Clear(); // Limpa a lista de quem apanhou
        
        //Security: Garante que temos um ataque configurado
        if (ctx.CurrentAttack == null)
        {
            SwitchState(factory.Idle());
            return;
        }
        
        //Trava o personagem
        ctx.PlayerVelocity = Vector3.zero;

        if (ctx.CurrentAttack.SwingSound != null)
        {
            //Toca na posição do jogador
            AudioManager.Instance.PlaySFX(ctx.CurrentAttack.SwingSound,ctx.transform.position);
        }
        
        //Debug visual amarelo (preparando)
        if (ctx.PlayerRenderer != null) ctx.PlayerRenderer.material.color = Color.yellow;
        
        // Debug Log
        Debug.Log($"👊 GOLPE {ctx.ComboIndex + 1}: {ctx.CurrentAttack.AttackName}");
    }

    public override void UpdateState()
    {
        // Se por algum milagre o estado travar por 2 segundos, ele se auto-destrói
        if (_timer > 2.0f)
        {
            SwitchState(factory.Idle());
            return;
        }
        
       _timer += Time.deltaTime;
       //A logica das fases
       ProcessAttackPhases();
       CheckSwitchStates();
    }

    private void ProcessAttackPhases()
    {
        //Fase 1 : Startup
        if (_currentPhase == AttackPhase.Startup)
        {
            if (_timer >= ctx.CurrentAttack.StartupTime)
            {
                //Startup acabou -> vai para Active
                _currentPhase = AttackPhase.Active;
                _timer = 0; //Reseta o timer para coontar a nova fase
                
                //Debug visual : Red
                if(ctx.PlayerRenderer != null) ctx.PlayerRenderer.material.color = Color.red;
                
                //Aqui chamaremos CreateHitbox();
            }
        }
        //Fase 2 : Active (dano)
        else if (_currentPhase == AttackPhase.Active)
        {
            // A CADA FRAME DA FASE ACTIVE, verificamos colisão
            DetectHits();
            
            if (_timer >= ctx.CurrentAttack.ActiveTime)
            {
                //Active acabou -> Vai para recovery
                _currentPhase = AttackPhase.Recovery;
                _timer = 0;
                
                //Debug visual : CINZA(recovery)
                if(ctx.PlayerRenderer != null) ctx.PlayerRenderer.material.color = Color.gray;
            }
        }
        //Fase 3 : Recovery
        else if (_currentPhase == AttackPhase.Recovery)
        {
            if (_timer >= ctx.CurrentAttack.RecoveryTime)
            {
                //Recovery acabou,fim do golpe
                _currentPhase = AttackPhase.Finished;
            }
        }
        
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        //Restaura a cor original
        if(ctx.PlayerRenderer != null) ctx.PlayerRenderer.material.color = ctx.OriginalColor;
        
        _hitTargets.Clear();
    }

    public override void CheckSwitchStates()
    {
       // Só tomamos decisão quando o golpe atual termina.
        if (_currentPhase == AttackPhase.Finished)
        {
            // O jogador apertou ataque DURANTE a animação deste golpe? (Input Buffer)
            // E AINDA temos golpes na lista para fazer?
            if (ctx.IsAttackPressed && ctx.ComboIndex <= ctx.ComboChain.Length - 1)
            {
                ctx.ComboIndex++;
                SwitchState(factory.Attack()); // Reinicia o estado para o próximo soco
            }
           //Acabou o combo, voltar a se mover
            else
            {
                //Reseta o combo para proxima vez
                ctx.ComboIndex = 0;
                
                
                // Se o jogador estiver segurando o analógico, vai direto pro Mov
                if(ctx.CurrentMovementInput.magnitude > 0.1f)
                    SwitchState(factory.Move());
                else
                {
                    SwitchState(factory.Idle());
                }

            }
            
            
            
            
            
            
        }
    }

    public override void InitializeSubState()
    {
        
    }

    private void DetectHits()
    {
        //1.Onde é o soco?
        //Pegamos o offset do arquivo e convertemos para posição no Mundo Real
        //baseada na rotação atual do personagem
        Vector3 attackPos = ctx.transform.TransformPoint(ctx.CurrentAttack.HitboxOffset);
        
        //2.Qual o tamanho?
        float attackRange = ctx.CurrentAttack.HitboxRadius;

        
        //3. Detectar colisão
        Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackRange, ctx.HitLayer);

        foreach (Collider enemy in hitEnemies)
        {
            //Security: Ignora o proprio colisor do atacante
            if (enemy.gameObject == ctx.gameObject) continue;
            
            //Tenta achar o contato IDamageable no objeto que tocamos
            IDamageable damageable = enemy.GetComponent<IDamageable>();

            if (damageable != null)
            {
                //Verifica se ja nãoo batemos nesse inimigo neste mesmo ataque
                if (!_hitTargets.Contains(damageable))
                {
                    //Calculo Knockback
                    //1. Para onde o inimigo deve voar?
                    Vector3 direction = (enemy.transform.position - ctx.transform.position).normalized;
                    
                    //2.Ignorar altura para o inimigo não voar para baixo/cima
                    direction.y = 0;
                    // 3. Envia o pacote completo (Dano do Arquivo + Direção Calculada + Força do Arquivo)
                    damageable.TakeDamage(ctx.CurrentAttack.Damage,direction,ctx.CurrentAttack.Knockback,ctx.CurrentAttack.HitStunDuration); // Aplica o dano

                    if (ctx.CurrentAttack.ImpactSound != null)
                    {
                        // Toca exatamente onde o inimigo está (som 3D)
                        AudioManager.Instance.PlaySFX(ctx.CurrentAttack.ImpactSound,enemy.transform.position);
                    }
                    
                    //Impact VFX
                    if (ctx.CurrentAttack.HitEffectPrefab != null)
                    {
                        // Perguntamos ao colisor do inimigo: "Qual seu ponto mais próximo do meu peito?"
                        // Usamos (position + up) para pegar a altura do peito/ombro do atacante como referência.
                        Vector3 attackerCenter = ctx.transform.position + Vector3.up;
                        Vector3 exactHitPoint = enemy.ClosestPoint(attackerCenter);
                        
                        //CALCULAR A ROTAÇÃO (Para as faíscas voarem na direção certa)
                        // A direção é do atacante PARA o ponto de contato.
                        Vector3 impactDirection = (exactHitPoint - ctx.transform.position).normalized;
                        Quaternion hitRotation = Quaternion.LookRotation(impactDirection);
                        
                        //Cria a particula
                        GameObject vfx = Object.Instantiate(ctx.CurrentAttack.HitEffectPrefab, exactHitPoint, hitRotation);
                        
                        //Destroi após 1 seg
                        Object.Destroy(vfx,1.0f);
                        
                    }
                    
                    
                    _hitTargets.Add(damageable); // Adiciona na lista negra para não bater de novo
                }
            }
        }


    }
}
