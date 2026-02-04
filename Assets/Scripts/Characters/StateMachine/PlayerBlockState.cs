using UnityEngine;

public class PlayerBlockState : PlayerBaseState
{
    public PlayerBlockState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
    }

    public override void EnterState()
    {
        //Avisa a vida que estamos protegidos
        if (ctx.HealthComp != null)
        {
            ctx.HealthComp.IsBlocking = true;
            ctx.PlayerVelocity.y = -2f;

            if (ctx.ShieldVisual != null)
            {
                ctx.ShieldVisual.SetActive(true);
            }
        }
        
        Debug.Log("🛡️ Entrou em Postura Defensiva");
        
        
    }

    public override void UpdateState()
    {
       CheckSwitchStates();
       
       //Logica de Strafing
       //Enquanto usa block, player se move 30% da velocidade normal
       Vector3 moveDir = new Vector3(ctx.CurrentMovementInput.x,0,ctx.CurrentMovementInput.y).normalized;
       
       // Converte para rotação da câmera
       if (moveDir.magnitude > 0.1f)
       {
           float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + ctx.MainCameraTransform.eulerAngles.y;
           Vector3 moveDirection = Quaternion.Euler(0f,targetAngle,0f) * Vector3.forward;
           
           // Move devagar
           ctx.Controller.Move(moveDirection * (ctx.MoveSpeed * 0.5f * Time.deltaTime));
           
           ctx.transform.rotation = Quaternion.Euler(0f,targetAngle,0f);
           
       }
       
       
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        if (ctx.HealthComp != null)
        {
            ctx.HealthComp.IsBlocking = false;

            if (ctx.ShieldVisual != null)
            {
                ctx.ShieldVisual.SetActive(false);
            }
        }
    }

    public override void CheckSwitchStates()
    {
        if (!ctx.IsBlockPressed)
        {
            if (ctx.CurrentMovementInput.magnitude > 0.1f)
            {
                SwitchState(factory.Move());
            }
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
