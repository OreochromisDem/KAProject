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
        
        if (ctx.IsDashPressed && ctx.DashCooldownTimer <= 0)
        {
            SwitchState(factory.Dash());
            return;
        }
        
        if (ctx.IsAttackPressed && ctx.IsGrounded)
        {
            ctx.UseAttackInput();
            SwitchState(factory.Attack());
            return;
        }

        if (ctx.IsBlockPressed && ctx.IsGrounded)
        {
            SwitchState(factory.Block());
            return;
        }
        
        if (ctx.IsJumpPressed && ctx.IsGrounded)
        {
            ctx.UseJumpInput();
            SwitchState(factory.Jump());
            return;
        }
        
        //Se parou de mexer o analógico, volta para IDLE;
         if (ctx.CurrentMovementInput.magnitude < 0.1f)
        {
            SwitchState(factory.Idle());
            return;
        }
    }

    private void Move()
    {
       //Calcula o angulo albo baseado no Input + Rotação da camera
       float targetAngle = Mathf.Atan2(ctx.CurrentMovementInput.x, ctx.CurrentMovementInput.y) * Mathf.Rad2Deg + ctx.MainCameraTransform.eulerAngles.y;
       
       //Suaviza a rotação do personagem
       float angle = Mathf.SmoothDampAngle(ctx.transform.eulerAngles.y, targetAngle, ref ctx.TurnSmoothVelocity,
           ctx.TurnSmoothTime);
       
       //Aplica a rotação no personagem
       ctx.transform.rotation = Quaternion.Euler(0f,angle,0f);
       
       //Calcula a direção do movimento baseada no ângulo Alvo
       Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
       
       //Move o perssonagem
       ctx.Controller.Move(moveDir.normalized * (ctx.MoveSpeed * Time.deltaTime));

    }

    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }
}
