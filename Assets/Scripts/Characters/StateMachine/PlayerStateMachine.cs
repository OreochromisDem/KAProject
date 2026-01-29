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
  [field: SerializeField] public float JumpForce { get; private set; } = 5f;

  [field: SerializeField] public float JumpBufferTime { get; private set; } = 0.2f;


  [Header("Combat")] 
  public AttackSO CurrentAttack; // Onde arrastaremos o arquivo do golpe
  public Renderer PlayerRenderer; // Alterar cor(debug)
  [HideInInspector] public Color OriginalColor;
  
  //Input
  [HideInInspector] public bool IsAttackPressed;
  
  
  [Header("Dash configs")]
  public float DashSpeed = 20f;
  public float DashDuration = 0.2f;
  public float DashCooldown = 1f;

   public float DashCooldownTimer;
  [HideInInspector] public bool IsDashPressed;
  
  // -- Variaveis de Estados --

  public Vector3 PlayerVelocity; // Para controlar pulo e gravidade
  public bool IsGrounded;
  public Vector2 CurrentMovementInput; // X e Y do controle
  private float _jumpBufferTimer;
  public bool IsJumpPressed => _jumpBufferTimer > 0;
  
  [Header("Configurações de Chão")]
  [SerializeField] private Transform groundCheckPos;
  [SerializeField] private float groundCheckRadius = 0.2f;
  [SerializeField] private LayerMask groundLayer;
  
  [Header("Referencia da camera")]
  public Transform MainCameraTransform;

  [Header("Suavização de Rotação")] 
  public float TurnSmoothTime = 0.1f;
  [HideInInspector] public float TurnSmoothVelocity;
  
  public void UseJumpInput() => _jumpBufferTimer = 0;
  
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
        
        //Busca automatica da cam principal
        if (Camera.main != null)
        {
            MainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("ERRO: Nenhuma MainCamera encontrada na cena (Tag MainCamera)!");
        }

        //Salva a cor original para restaurar depois do soco
        if (PlayerRenderer != null)
        {
            OriginalColor = PlayerRenderer.material.color;
        }
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
        
        //Roda o timer do cooldown(dash)
        if (DashCooldownTimer > 0)
        {
            DashCooldownTimer -= Time.deltaTime;
        }
        
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

        // Lógica do Buffer:
        // Se apertou o botão, enche o timer (0.2s)
        if (Input.actions["Jump"].triggered)
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        //o timer diminui a cada frame
        _jumpBufferTimer -= Time.deltaTime;
        
        //Leitura do dash
        IsDashPressed = Input.actions["Dash"].triggered;
        
        //Leitura do attack
        if (Input.actions["Attack"].triggered)
        {
            IsAttackPressed = true;
        }
        
    }

    public void UseAttackInput() => IsAttackPressed = false;
    
    public void ApplyGravity()
    {
        //"Existe algum objeto da layer 'Ground' dentro de uma esfera no meu pé?"
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundCheckRadius, groundLayer);

        if (IsGrounded && PlayerVelocity.y < 0)
        {
            PlayerVelocity.y = -2f;
        }
        
        PlayerVelocity.y += GravityValue * Time.deltaTime;
        Controller.Move(PlayerVelocity * Time.deltaTime);
    }
    
     //Desenha a esfera no editor
    private void OnDrawGizmos()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);
        }
    }

    private void FixedUpdate()
    {
        if (_currentState != null)
        {
            _currentState.FixedUpdateState();
        }
    }
}

