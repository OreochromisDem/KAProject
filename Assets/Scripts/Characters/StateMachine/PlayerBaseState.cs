using UnityEngine;

public  abstract class PlayerBaseState
{
    // *O "Contexto" Todo estado precisa saber quem ele esta controlando
    protected PlayerStateMachine ctx;
    protected PlayerStateFactory factory;
    
    // Construtor: Recebe o dono e a fabrica de estados
    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        ctx = currentContext;
        factory = playerStateFactory;
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();
        newState.EnterState();
        ctx.CurrentState = newState;
    }
    
    //Ocorre uma vez quando o estado começa(Ex: Tocar som de inicio de pulo)
    public abstract void EnterState();
    
    //Ocorre a cada frame(Update do Unity)
    public abstract void UpdateState();
    
    //Ocorre a cada frame de física (FixedUpdate)
    public abstract void FixedUpdateState();
    
    //Ocorre quando saimos do estado(Ex: Limpar variaveis)
    public abstract void ExitState();
    
    //Metodo para verificar se devemos trocar de estados ( Ex: Se apertou X, vai para ataque)
    public abstract void CheckSwitchStates();
    
    //Metodo auxiliar para iniciar sub-estados
    public abstract void InitializeSubState();

}
