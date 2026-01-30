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
        //Zera tudo
        _timer = 0;
        _currentPhase = AttackPhase.Startup;
        
        //Trava o personagem
        ctx.PlayerVelocity.x = 0;
        ctx.PlayerVelocity.y = 0;
        
        //Debug visual amarelo (preparando)
        if (ctx.PlayerRenderer != null) ctx.PlayerRenderer.material.color = Color.yellow;

        // Debug Log
        Debug.Log($"Iniciando ataque: {ctx.CurrentAttack.AttackName}");
    }

    public override void UpdateState()
    {
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
    }

    public override void CheckSwitchStates()
    {
        //Só permite sair do estado quando todas as fases terminarem
        if (_currentPhase == AttackPhase.Finished)
        {
            if(ctx.CurrentMovementInput.magnitude > 0.1f)
                SwitchState(factory.Move());
            else
            {
                SwitchState(factory.Idle());
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
                    damageable.TakeDamage(ctx.CurrentAttack.Damage); // Aplica o dano
                    _hitTargets.Add(damageable); // Adiciona na lista negra para não bater de novo
                }
            }
        }


    }
}
