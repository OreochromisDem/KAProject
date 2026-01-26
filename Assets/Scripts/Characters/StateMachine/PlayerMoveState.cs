using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory){ }

    public override void EnterState()
    {
        Debug.Log("Entrou em MOVE");
        // Futuro: Tocar animação de Correr
        
    }

    public override void UpdateState()
    {
        Move();
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
    }

    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
        if (ctx.IsJumpPressed && ctx.IsGrounded)
        {
            ctx.UseJumpInput();
            SwitchState(factory.Jump());
        }
        
        //Se parou de mexer o analógico, volta para IDLE;
        if (ctx.CurrentMovementInput.magnitude < 0.1f)
        {
            SwitchState(factory.Idle());
        }
    }

    private void Move()
    {
        //Lógica de movimentação
        Vector3 move = new Vector3(ctx.CurrentMovementInput.x, 0, ctx.CurrentMovementInput.y);
        
        //Movimento relativo a camera
        ctx.Controller.Move(move * (ctx.MoveSpeed * Time.deltaTime));
        
        //Rotação
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRotation, ctx.MoveSpeed * Time.deltaTime);
        }
        
    }

    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }
}
