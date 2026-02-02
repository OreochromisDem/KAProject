using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Configuração")]
    public Image FillImage;

    public HealthComponent HealthTarget;


    [Header("Visual")] 
    public bool AlwaysFaceCamera = true;
    
    public Gradient HealthGradient;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnEnable()
    {
        if (HealthTarget != null)
        {
            // Ouve o evento de dano do nosso HealthComponent
            HealthTarget.OnDamageTaken.AddListener(UpdateHealth);
            
            
        }
    }

    private void OnDisable()
    {
        if (HealthTarget != null)
        {
            HealthTarget.OnDamageTaken.RemoveListener(UpdateHealth);
        }
    }

    private void LateUpdate()
    {
        // BILLBOARD: Faz a barra sempre olhar para a câmera
        // Senão, quando o player girar, a barra gira junto e fica invisível (de lado)
        if (AlwaysFaceCamera && _cam != null)
        {
            transform.rotation = _cam.transform.rotation;
        }
    }
    
    // O evento manda o "amount" de dano, mas vamos recalcular a % total
    private void UpdateHealth(float damageAmount)
    {
        if(HealthTarget == null) return;
        
        //Calculo simples de porcentagem: Atual / Maximo
        float pct =  HealthTarget.CurrentHealth / HealthTarget.MaxHealth;
        
        // Aplica na UI
        FillImage.fillAmount = pct;
        
            //Pinta a barra baseada na porcentagem ---
            // O Evaluate pega a cor na posição "pct" do gradiente
        FillImage.color = HealthGradient.Evaluate(pct);
    }
    
    
    
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
