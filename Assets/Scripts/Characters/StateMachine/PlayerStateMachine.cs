using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(HealthComponent))]
public class PlayerStateMachine : MonoBehaviour
{
    #region 1. Referências de Componentes
    [Header("Componentes Essenciais")]
    // Propriedades públicas com set privado para garantir encapsulamento seguro
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public PlayerInput Input { get; private set; }
    [field: SerializeField] public HealthComponent HealthComp { get; private set; }
    
    [Header("Referências de Cena")]
    public Transform MainCameraTransform;
    public Renderer PlayerRenderer; // Usado para debug visual (troca de cor)
    
    // Armazena a cor original para restaurar após feedback visual
    [HideInInspector] public Color OriginalColor;
    #endregion

    #region 2. Configurações de Movimento (Stats)
    [Header("Movimentação Básica")]
    [field: SerializeField] public float MoveSpeed { get; private set; } = 6f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 10f;
    [field: SerializeField] public float GravityValue { get; private set; } = -9.81f;
    
    [Header("Pulo")]
    [field: SerializeField] public float JumpForce { get; private set; } = 5f;
    [field: SerializeField] public float JumpBufferTime { get; private set; } = 0.2f;

    [Header("Suavização de Rotação")]
    public float TurnSmoothTime = 0.1f;
    [HideInInspector] public float TurnSmoothVelocity;
    #endregion

    #region 3. Configurações de Dash
    [Header("Habilidade: Dash")]
    public float DashSpeed = 20f;
    public float DashDuration = 0.2f;
    public float DashCooldown = 1f;
    public AnimationCurve DashSpeedCurve;
    public TrailRenderer DashTrail; // Efeito visual do rastro
    
    // Variáveis de Estado do Dash
    public bool IsInvincible = false;
    public float DashCooldownTimer;
    #endregion

    #region 4. Sistema de Combate & Combo
    [Header("Combate")]
    public LayerMask HitLayer; // Define quais layers são consideradas "Inimigos"
    public GameObject ShieldVisual; // Feedback visual do bloqueio

    [Header("Configuração de Combo")]
    public AttackSO[] ComboChain; // Lista ordenada de ataques (ScriptableObjects)
    [HideInInspector] public int ComboIndex = 0; // Índice atual na lista de combos

    /// <summary>
    /// Propriedade inteligente que retorna o Ataque atual baseado no índice do combo.
    /// Possui verificação de limites de array para evitar erros.
    /// </summary>
    public AttackSO CurrentAttack
    {
        get
        {
            if (ComboChain == null || ComboChain.Length == 0) return null;
            
            // Segurança: Se o índice estourar o array, retorna o primeiro ataque
            if (ComboIndex >= ComboChain.Length) return ComboChain[0];

            return ComboChain[ComboIndex];
        }
    }
    #endregion

    #region 5. Variáveis de Estado e Input
    // --- Estados da State Machine ---
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

    // --- Variáveis de Física e Controle ---
    public Vector3 PlayerVelocity; // Controla velocidade vertical (gravidade/pulo)
    public bool IsGrounded;
    private Vector3 _impactVelocity; // Força externa (Knockback)
    [HideInInspector] public float PendingStunDuration;

    // --- Inputs e Buffers ---
    public Vector2 CurrentMovementInput;
    [HideInInspector] public bool IsBlockPressed;
    [HideInInspector] public bool IsDashPressed;
    
    // Buffers de entrada (permitem pressionar botões levemente antes da ação)
    [Header("Input Buffer")]
    public float InputBufferTime = 0.2f;
    
    private float _jumpBufferTimer;
    public bool IsJumpPressed => _jumpBufferTimer > 0;

    private float _attackBufferTimer;
    [HideInInspector] public bool IsAttackPressed => _attackBufferTimer > 0;
    
    // Métodos para limpar os buffers após consumo
    public void UseJumpInput() => _jumpBufferTimer = 0;
    public void UseAttackInput() => _attackBufferTimer = 0;
    #endregion

    #region 6. Checagem de Chão (Physics)
    [Header("Configurações de Chão")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    #endregion

    #region 7. Métodos Unity (Lifecycle)
    private void Awake()
    {
        // Inicialização de referências e componentes
        Controller = GetComponent<CharacterController>();
        Input = GetComponent<PlayerInput>();
        HealthComp = GetComponent<HealthComponent>();
        
        // Inicializa a Fábrica de Estados
        _states = new PlayerStateFactory(this);

        // Busca automática da câmera principal (Fallback seguro)
        if (Camera.main != null)
        {
            MainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("PlayerStateMachine: Nenhuma MainCamera encontrada na cena (Tag MainCamera)!");
        }

        // Cache da cor original para feedback de dano/ataque
        if (PlayerRenderer != null)
        {
            OriginalColor = PlayerRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        // Inscrição nos eventos do HealthComponent
        HealthComp.OnDeath.AddListener(HandleDeath);
        HealthComp.OnKnockback.AddListener(HandleKnockback);
    }

    private void OnDisable()
    {
        // Desinscrição para evitar Memory Leaks
        HealthComp.OnDeath.RemoveListener(HandleDeath);
        HealthComp.OnKnockback.RemoveListener(HandleKnockback);
    }

    void Start()
    {
        // Define o estado inicial como Idle
        _currentState = _states.Idle();
        _currentState.EnterState();
    }

    void Update()
    {
        // 1. Gerenciamento de Cooldowns
        if (DashCooldownTimer > 0)
        {
            DashCooldownTimer -= Time.deltaTime;
        }

        // 2. Leitura de Inputs e Física
        ReadInput();
        ApplyGravity();

        // 3. Delegação para o Estado Atual (Lógica Específica)
        if (_currentState != null)
        {
            _currentState.UpdateState();
        }
    }

    private void FixedUpdate()
    {
        if (_currentState != null)
        {
            _currentState.FixedUpdateState();
        }
    }
    #endregion

    #region 8. Lógica Principal (Input & Física)
    private void ReadInput()
    {
        // Leitura do Vetor de Movimento (WASD / Analógico)
        CurrentMovementInput = Input.actions["Move"].ReadValue<Vector2>();

        // Lógica de Jump Buffer
        if (Input.actions["Jump"].triggered)
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        _jumpBufferTimer -= Time.deltaTime;

        // Leitura de Dash (Trigger único)
        IsDashPressed = Input.actions["Dash"].triggered;

        // Lógica de Attack Buffer
        if (Input.actions["Attack"].triggered)
        {
            _attackBufferTimer = InputBufferTime;
        }
        if (_attackBufferTimer > 0)
        {
            _attackBufferTimer -= Time.deltaTime;
        }

        // Leitura de Bloqueio (Hold)
        IsBlockPressed = Input.actions["Block"].IsPressed();
    }

    public void ApplyGravity()
    {
        if (!Controller.enabled) return;

        // Verifica colisão com o chão
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundCheckRadius, groundLayer);

        // Reseta velocidade Y se estiver no chão (valor pequeno negativo para manter grounded)
        if (IsGrounded && PlayerVelocity.y < 0)
        {
            PlayerVelocity.y = -2f;
        }

        // Aplica gravidade padrão
        PlayerVelocity.y += GravityValue * Time.deltaTime;
        Controller.Move(PlayerVelocity * Time.deltaTime);

        // Aplica forças externas (Knockback) com suavização (Lerp)
        if (_impactVelocity.magnitude > 0.2f)
        {
            Controller.Move(_impactVelocity * Time.deltaTime);
            // Reduz a força de impacto gradualmente
            _impactVelocity = Vector3.Lerp(_impactVelocity, Vector3.zero, 5f * Time.deltaTime);
        }
    }
    #endregion

    #region 9. Gerenciamento de Estados e Eventos
    public void SwitchState(PlayerBaseState newState)
    {
        // Ciclo de vida da transição de estado:
        // 1. Sai do estado antigo (Limpeza)
        if (CurrentState != null)
        {
            CurrentState.ExitState();
        }

        // 2. Troca a referência
        CurrentState = newState;

        // 3. Entra no novo estado (Inicialização)
        CurrentState.EnterState();
    }

    private void HandleDeath()
    {
        // Força a saída do estado atual e entra no estado de Morte
        if (CurrentState != null)
        {
            CurrentState.ExitState();
        }

        CurrentState = _states.Dead();
        CurrentState.EnterState();
    }

    private void HandleKnockback(Vector3 direction, float force, float stunDuration)
    {
        // Aplica uma força instantânea que decai no ApplyGravity
        _impactVelocity = direction * force;
        
        //Se estiver defendendo , não entra no estado de HitStun
        if (HealthComp.IsBlocking) return;
        
        //Se não estiver morto, aplica o HitStun
        if (CurrentState != _states.Dead())
        {
            //Salva o tempo na variavel publica para o estado ler
            PendingStunDuration = stunDuration;
            //Força a entrada no estado de hit stun(interrompe ataque)
            SwitchState(_states.HitStun());
        }
    }
    #endregion

    #region 10. Debug e Editor
    private void OnDrawGizmos()
    {
        // Visualização do Ground Check
        if (groundCheckPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);
        }

        // Visualização da Hitbox do Ataque Atual
        if (CurrentAttack != null)
        {
            Gizmos.color = Color.red;
            // Converte o offset local para posição global
            Vector3 gizmoPos = transform.TransformPoint(CurrentAttack.HitboxOffset);
            Gizmos.DrawWireSphere(gizmoPos, CurrentAttack.HitboxRadius);
        }
    }
    #endregion
}