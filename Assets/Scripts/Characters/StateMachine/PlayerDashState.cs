using System;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _dashTimeCounter;
    private Vector3 _dashDirection;

    private float _startDashTime;
    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
    }

    public override void EnterState()
    {
        //Inicia cooldown
        ctx.DashCooldownTimer = ctx.DashCooldown;
        _dashTimeCounter = ctx.DashDuration;
        _startDashTime = ctx.DashDuration;
        
        //Ativa trail
        if (ctx.DashTrail != null)
        {
            ctx.DashTrail.emitting = true;
        }
        
        //Ativa Invencibilidade
        ctx.IsInvincible = true;
        
        //Define a direção do Dash
        Vector2 input = ctx.CurrentMovementInput;

        if (input.magnitude > 0.1f)
        {
            //Calcula o angulo relativo a camera
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + ctx.MainCameraTransform.eulerAngles.y;
            
            //Converte o angulo em direção (vector3)
            _dashDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            //teste: Gira o personagem instantaneamente para a direção do dash 
            ctx.transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
        else
        {
            // Se não estiver tocando no analógico, dá dash para frente do personagem
            _dashDirection = ctx.transform.forward;
        }

    }

    public override void UpdateState()
    {
        //Calcula quanto tempo ja passou em porcentagem
        float normalizedTime = 1 - (_dashTimeCounter/_startDashTime);
        
        //Pega a velocidade na curva 
        // Se a curva for decrescente, ele começa rápido e freia no final.
        float speedMultiplier = ctx.DashSpeedCurve.Evaluate(normalizedTime);
        
        
        //Move sem gravidade para dar sensação de impulso
        ctx.Controller.Move(_dashDirection * (ctx.DashSpeed * Time.deltaTime));

        _dashTimeCounter -= Time.deltaTime;
        
        CheckSwitchStates();

    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
       //Limpeza: Desliga o rastro e a invencibilidade
       if (ctx.DashTrail != null)
       {
           ctx.DashTrail.emitting = false;
       }
       ctx.IsInvincible = false;
    }

    public override void CheckSwitchStates()
    {
       //Sai automaticamente quando o tempo acaba
       if (_dashTimeCounter <= 0)
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
