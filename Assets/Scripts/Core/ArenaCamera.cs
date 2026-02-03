using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class ArenaCamera : MonoBehaviour
{
    [Header("Alvos")] 
    public List<Transform> Targets; // Lista de targets da camera
    
    [Header("Configuração de movimento")]
    public Vector3 Offset = new Vector3(0, 5, -10); // Posição padrão da camera
    public float SmoothTime = 0.5f; //Tempo para a câmera chegar no destino

    [Header("Config de zoom")] 
    public float MinZoom = 40f;
    public float MaxZoom = 10f;
    public float ZoomLimiter = 50f;

    private Vector3 _velocity;
    private Camera _cam;
    private Vector3 _shakeOffset;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (Targets.Count == 0) return;
        
        Move();
        Zoom();
    }

    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        
        // A posição final é o Centro dos jogadores + o Offset (altura/distância configurada)
        Vector3 newPosition = centerPoint + Offset;
        
        // SmoothDamp faz o movimento ser "manteiga", sem travar
        Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, newPosition, ref _velocity, SmoothTime);
        
        //soma o tremor na posição final
        transform.position = smoothedPos + _shakeOffset;
        
        
    }

    private void Zoom()
    {
        //Calcula a maior distancia entre os jogadores
        float greatestDistance = GetGreatestDistance();
        
        // Regra de 3: Quanto maior a distância, maior o FOV
        // Mathf.Lerp faz a transição suave entre o zoom mínimo e máximo
        float newZoom = Mathf.Lerp(MaxZoom,MinZoom,greatestDistance / ZoomLimiter);
        
        // Aplica no Field of View (pode trocar por transform.position.z se preferir mover a câmera)
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, newZoom, Time.deltaTime);
    }
    
    //Matematica para achar o meio exato entre todos os alvos
    private Vector3 GetCenterPoint()
    {
        if (Targets.Count == 1)
        {
            return Targets[0].position;
        }
        var bounds = new Bounds(Targets[0].position, Vector3.zero);
        for (int i = 0; i < Targets.Count; i++)
        {
            bounds.Encapsulate(Targets[i].position);
        }
        return bounds.center;
    }
    // Matemática para saber quão longe eles estão
    private float GetGreatestDistance()
    {
        var bounds = new Bounds(Targets[0].position, Vector3.zero);
        for (int i = 0; i < Targets.Count; i++)
        {
            bounds.Encapsulate(Targets[i].position);
        }
        // Retorna a largura (X) ou profundidade (Z), o que for maior
        return Mathf.Max(bounds.size.x, bounds.size.z);
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeRoutine(intensity, duration));

    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            //Cria um vetor aleatório pequeno
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            _shakeOffset = new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null; // Espera o proximo frame
        }
        _shakeOffset = Vector3.zero; // Reseta quando acabar
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
