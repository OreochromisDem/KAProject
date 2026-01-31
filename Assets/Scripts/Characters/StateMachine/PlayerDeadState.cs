using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        
    }

    public override void EnterState()
    {
        //Trava tudo
        ctx.PlayerVelocity = Vector3.zero;
        
        //  Visual (Tomba o boneco para parecer morto - provisório até ter animação)
        ctx.transform.Rotate(90,0,0);
        
        //Desativa o colisor
        ctx.GetComponent<Collider>().enabled = false;
        
    }

    public override void UpdateState()
    {
        
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void ExitState()
    {
        
    }

    public override void CheckSwitchStates()
    {
        
    }

    public override void InitializeSubState()
    {
        
    }
}
