using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(HealthComponent))]
public class PlayerStateMachine : MonoBehaviour
{
  // --- Referências Globais ( o que os estados podem acessar) ---
  
  //Getters e Setters para que os estados leiam as variaveis do player
  [field : SerializeField] public CharacterController Controller { get; private set; }
  [field : SerializeField] public PlayerInput Input { get; private set; }
  [HideInInspector] public bool IsBlockPressed;
  
  //Variaveis de configuração(stats)
  [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
  [field: SerializeField] public float RotationSpeed { get; private set; } = 10f;
  [field: SerializeField] public float GravityValue { get; private set; } = -9.81f;
  [field: SerializeField] public float JumpForce { get; private set; } = 5f;
  [field: SerializeField] public HealthComponent HealthComp { get; private set; }

  [field: SerializeField] public float JumpBufferTime { get; private set; } = 0.2f;

  
  private Vector3 _impactVelocity;
  private PlayerInput _playerInput;
  
  
  
  [Header("Visuals")]
  public TrailRenderer DashTrail;

  public GameObject ShieldVisual;
  
  [Header("Combat")] 
  public AttackSO CurrentAttack; // Onde arrastaremos o arquivo do golpe

  public LayerMask HitLayer; // Configuração crucial: O que é inimigo?
  public Renderer PlayerRenderer; // Alterar cor(debug)
  [HideInInspector] public Color OriginalColor;

//  public float AttackCooldown = 0.5f;
  //[HideInInspector] public float AttackCooldownTimer;
  
  //Input
  
  
  
  [Header("Dash configs")]
  public float DashSpeed = 20f;
  public float DashDuration = 0.2f;
  public float DashCooldown = 1f;
  public AnimationCurve DashSpeedCurve;
  
  public bool IsInvincible = false;
  public float DashCooldownTimer;
  [HideInInspector] public bool IsDashPressed;
  
  // -- Variaveis de Estados --

  public Vector3 PlayerVelocity; // Para controlar pulo e gravidade
  public bool IsGrounded;
  public Vector2 CurrentMovementInput; // X e Y do controle
  private float _jumpBufferTimer;
  public bool IsJumpPressed => _jumpBufferTimer > 0;
  
  
  [Header("Input buffer")]
  public float InputBufferTime = 0.2f;

  private float _attackBufferTimer;
  
  [HideInInspector] public bool IsAttackPressed => _attackBufferTimer > 0;

  public void UseAttackInput() => _attackBufferTimer = 0;
  
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
        HealthComp = GetComponent<HealthComponent>();
        _playerInput = GetComponent<PlayerInput>();
        
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
        CurrentMovementInput = _playerInput.actions["Move"].ReadValue<Vector2>();

        // Lógica do Buffer:
        // Se apertou o botão, enche o timer (0.2s)
        if (_playerInput.actions["Jump"].triggered)
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        //o timer diminui a cada frame
        _jumpBufferTimer -= Time.deltaTime;
        
        //Leitura do dash
        IsDashPressed = _playerInput.actions["Dash"].triggered;
        
        //Leitura do attack
        if (_playerInput.actions["Attack"].triggered)
        {
            _attackBufferTimer = InputBufferTime;
        }
        
        //O timer diminui a cada frame
        if (_attackBufferTimer > 0)
        {
            _attackBufferTimer -= Time.deltaTime;
        }

        if (_playerInput.actions["Block"].IsPressed())
        {
            IsBlockPressed = true;
        }
        else
        {
            IsBlockPressed = false;
        }
        
    }
    


    public void ApplyGravity()
    {
        if(!Controller.enabled)return;
        
        //"Existe algum objeto da layer 'Ground' dentro de uma esfera no meu pé?"
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundCheckRadius, groundLayer);

        if (IsGrounded && PlayerVelocity.y < 0)
        {
            PlayerVelocity.y = -2f;
        }
        
        PlayerVelocity.y += GravityValue * Time.deltaTime;
        Controller.Move(PlayerVelocity * Time.deltaTime);

        if (_impactVelocity.magnitude > 0.2f)
        {
            Controller.Move(_impactVelocity *  Time.deltaTime);
            _impactVelocity = Vector3.Lerp(_impactVelocity, Vector3.zero, 5f* Time.deltaTime);
        }
    }
    
     //Desenha a esfera no editor
    private void OnDrawGizmos()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);
        }

        if (CurrentState != null)
        {
            Gizmos.color = Color.red;
            // Simula a posição final da hitbox usando o TransformPoint
            Vector3 gizmoPos = transform.TransformPoint(CurrentAttack.HitboxOffset);
            Gizmos.DrawWireSphere(gizmoPos, CurrentAttack.HitboxRadius);
        }
    }

    private void OnEnable()
    {
        // INSCREVER: Quando o evento OnDeath acontecer, execute o método HandleDeath
        HealthComp.OnDeath.AddListener(HandleDeath);
        HealthComp.OnKnockback.AddListener(HandleKnockback);
    }

    private void OnDisable()
    {
        // DESINSCREVER
        HealthComp.OnDeath.RemoveListener(HandleDeath);
        HealthComp.OnKnockback.RemoveListener(HandleKnockback);
        
    }

    private void HandleDeath()
    {
        if (CurrentState != null)
        {
            CurrentState.ExitState();
        }
        
        CurrentState = _states.Dead();
        
        CurrentState.EnterState();
    }

    private void HandleKnockback(Vector3 direction, float force)
    {
        _impactVelocity = direction * force;
    }
    
    public void SwitchState(PlayerBaseState newState)
    {
        // 1. Se tem estado atual, roda a limpeza (Exit)
        if (CurrentState != null)
        {
            CurrentState.ExitState();
        }

        // 2. Troca a referência
        CurrentState = newState;

        // 3. Inicia o novo estado (Enter)
        CurrentState.EnterState();
    }

    private void FixedUpdate()
    {
        if (_currentState != null)
        {
            _currentState.FixedUpdateState();
        }
    }
}

