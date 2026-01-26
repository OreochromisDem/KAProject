using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
  // --- Referências Globais ( o que os estados podem acessar) ---
  
  //Getters e Setters para que os estados leiam as variaveis do player
  [field : SerializeField] public CharacterController Controller { get; private set; }
  [field : SerializeField] public PlayerInput Input { get; private set; }
  
  //Variaveis de configuração(stats)
  [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
  [field: SerializeField] public float RotationSpeed { get; private set; } = 10f;
  [field: SerializeField] public float GravityValue { get; private set; } = -9.81f;
  
  // -- Variaveis de Estados --

  public Vector3 PlayerVelocity; // Para controlar pulo e gravidade
  public bool IsGrounded;
  public Vector2 CurrentMovementInput; // X e Y do controle
  
     // -- Maquina de Estados --
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    //Getters para a Fábrica
    public PlayerBaseState CurrentState {get {return _currentState;} set{_currentState = value;}}


    private void Awake()
    {
        //Pega as referencias automaticamente
        Controller = GetComponent<CharacterController>();
        Input = GetComponent<PlayerInput>();
        
        //Inicializa a fabrica de estados
        _states = new PlayerStateFactory(this);
    }
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Começa parado
         _currentState = _states.Idle();
         _currentState.EnterState();
    }
    

    // Update is called once per frame
    void Update()
    {
        ReadInput();
        ApplyGravity();
        
        //Delega o trabalho para o estado atual
        if (_currentState != null)
        {
            _currentState.UpdateState();
        }
        
    }

    private void ReadInput()
    {
        //Lê o input direto do sistema novo. "Move" é o nome do action no seu arquivo .inputactions
        CurrentMovementInput = Input.actions["Move"].ReadValue<Vector2>();
        
    }

    public void ApplyGravity()
    {
        IsGrounded = Controller.isGrounded;

        if (IsGrounded && PlayerVelocity.y < 0)
        {
            PlayerVelocity.y = -2f;
        }
        
        PlayerVelocity.y += GravityValue * Time.deltaTime;
        Controller.Move(PlayerVelocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (_currentState != null)
        {
            _currentState.FixedUpdateState();
        }
    }
}

