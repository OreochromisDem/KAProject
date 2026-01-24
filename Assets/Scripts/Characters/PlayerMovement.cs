using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController controller;
    private Vector2 currentInputVector;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    
    // --- MÉTODOS PÚBLICOS (Conectados via Unity Events) ---

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        //Le o valor de x e y do controle (vector2)
        currentInputVector = context.ReadValue<Vector2>();
    }
    
    // Loop de fisica

    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    private void HandleGravity()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        
        //Move o personagem apenass no eixo Y
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // Converte o Input 2D (X, Y) para Movimento 3D (X, 0, Z)
        // No mundo 3D, "Y" é altura, então o "Y" do controle vira "Z" no mundo
        Vector3 move = new Vector3(currentInputVector.x, 0, currentInputVector.y);
        
        // Se houver movimento significativo
        if (move.magnitude >= 0.1f)
        {
            controller.Move(move.normalized * (moveSpeed * Time.deltaTime));
            
            // Gira o personagem para olhar na direção que está andando
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
        }
        
        
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
}
