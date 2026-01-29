using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float _timer;
    
    //O Enum define em qual fase do soco esstamos
    private enum AttackPhase{Startup,Active,Recovery,Finished}

    private AttackPhase _currentPhase;
    
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
}
