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
        //Se houver movimento no analogico, troca para Move
        if (ctx.CurrentMovementInput.magnitude > 0.1f)
        {
            SwitchState(factory.Move());
        }
    }
    public override void InitializeSubState(){ }
    public override void FixedUpdateState(){ }
    
}
