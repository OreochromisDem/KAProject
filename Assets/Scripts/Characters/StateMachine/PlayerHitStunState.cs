using UnityEngine;

public class PlayerHitStunState : PlayerBaseState
{
    private float _timer;
    
    public PlayerHitStunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
    }

    public override void EnterState()
    {
        //Pega o valor que foi armazenado na StateMachine
        _timer = ctx.PendingStunDuration;
        
        //Trava o movimento
        ctx.PlayerVelocity = Vector3.zero;
        ctx.ComboIndex = 0; // Reseta combo se tomar dano
        
        //Feedback visual
        if(ctx.PlayerRenderer != null)
            ctx.PlayerRenderer.material.color = Color.white;
        
        Debug.Log($"⚡ HIT STUN! Travado por {_timer}s");


    }

    public override void UpdateState()
    {
        _timer -= Time.deltaTime;
        CheckSwitchStates();
        
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        //Restaura cor
        if (ctx.PlayerRenderer != null)
            ctx.PlayerRenderer.material.color = ctx.OriginalColor;

    }

    public override void CheckSwitchStates()
    {
        if (_timer <= 0)
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
