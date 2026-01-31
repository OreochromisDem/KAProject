using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext,PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory) { }

    public override void EnterState()
    {
         Debug.Log("Entrou em IDLE"); // Útil para debugar
        // Aqui futuramente tocaremos a animação de "Respirando"
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState(){ }

    public override void CheckSwitchStates()
    {
        
        // Prioridade 1: Dash
        if (ctx.IsDashPressed && ctx.DashCooldownTimer <= 0)
        {
            SwitchState(factory.Dash());
            return;
        }
        // Prioridade 2: Ataque (Só se estiver no chão)
         if (ctx.IsAttackPressed && ctx.IsGrounded)
        {
            ctx.UseAttackInput();
            SwitchState(factory.Attack());
            return;
        }
        
        if (ctx.IsJumpPressed & ctx.IsGrounded)
        {
            ctx.UseJumpInput();
            SwitchState(factory.Jump());
            return;
        }
        //Se houver movimento no analogico, troca para Move
        if (ctx.CurrentMovementInput.magnitude > 0.1f)
        {
            SwitchState(factory.Move());
            return;
        }
    }
    public override void InitializeSubState(){ }
    public override void FixedUpdateState(){ }
    
}
