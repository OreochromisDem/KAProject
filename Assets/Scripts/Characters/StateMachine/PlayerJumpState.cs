using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext,PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        
    }

    public override void EnterState()
    {
        HandleJump();
    }

    public override void UpdateState()
    {
        HandleAirMovement();
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    { }

    public override void ExitState()
    {
        // Se precisar resetar algo ao tocar no chão, faça aqui
    }

    public override void CheckSwitchStates()
    {
        // Se o personagem já estiver caindo e tocou no chão novamente, volta para Idle
        // Nota: O CharacterController tem um pequeno delay para atualizar o isGrounded após o pulo,
        // então verificamos se a velocidade Y já é negativa (está caindo) para evitar cancelar o pulo instantaneamente.
        if (ctx.IsGrounded && ctx.PlayerVelocity.y < 0)
        {
            SwitchState(factory.Idle());
        }
    }

    private void HandleJump()
    {
        // A lógica do pulo é física simples:
        // V = raiz(h * -2 * g) é a fórmula física para alcançar uma altura 'h',
        // mas aqui vamos usar uma força direta para simplificar.
        ctx.IsGrounded = false;
        ctx.PlayerVelocity.y = ctx.JumpForce;
    }


    private void HandleAirMovement()
    {
        // Copiamos a lógica básica de movimento, talvez com velocidade reduzida?
        // Por enquanto, vamos manter igual para testar fácil.
        Vector3 move = new Vector3(ctx.CurrentMovementInput.x, 0, ctx.CurrentMovementInput.y);
        
        //Aplica o movimento ao CharacterController
        ctx.Controller.Move(move * ctx.MoveSpeed * Time.deltaTime);

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            ctx.transform.rotation = Quaternion.Slerp(ctx.transform.rotation, targetRotation, ctx.RotationSpeed * Time.deltaTime);
        }
        
    }
    
    
    public override void InitializeSubState()
    {
    }
}
