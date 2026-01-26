using UnityEngine;

public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }
    
    // Aqui vamos listar todos os estados possíveis
    // Por enquanto, só deixaremos preparado
    
    
    public PlayerBaseState Idle()
    {
        return new PlayerIdleState(_context, this);
    }

    public PlayerBaseState Move()
    {
        return new PlayerMoveState(_context, this);
    }

    public PlayerBaseState Jump()
    {
        return new PlayerJumpState(_context, this);
    }
    
    
}
