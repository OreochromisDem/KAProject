using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatFeedback : MonoBehaviour
{
    [Header("Configuração de Impacto")]
    [Tooltip("Quanto tempo o jogo congela ao acertar um golpe (0.05 a 0.15 é bom)")]
    public float HitStopDuration = 0.1f;
    
    [Tooltip("Força do tremor da câmera")]
    public float ShakeIntensity = 0.5f;
    
    [Tooltip("Duração do tremor")]
    public float ShakeDuration = 0.2f;

    private ArenaCamera _camera;
    private bool _isStopped = false;

    private void Start()
    {
        _camera = FindObjectOfType<ArenaCamera>();

        // Auto-registro: Procura todos os HealthComponents na cena
        var players = FindObjectsOfType<HealthComponent>();

        foreach (var player in players)
        {
            // Inscreve no evento de dano de cada um
            player.OnDamageTaken.AddListener(OnDamage);
        }
    }

    private void OnDamage(float damageAmount)
    {
        // 1. Aciona o Tremor da Câmera
        if (_camera != null)
        {
            // Se o dano for alto, pode aumentar a intensidade 
            _camera.Shake(ShakeIntensity, ShakeDuration);
        }

        // 2. Aciona o Hit Stop (Parada no tempo)
        if (!_isStopped)
        {
            StartCoroutine(HitStopRoutine());
        }
    }

    private IEnumerator HitStopRoutine()
    {
        _isStopped = true;

        // Congela o tempo do jogo
        Time.timeScale = 0.0f;

        // Espera em tempo REAL (pois o tempo do jogo está parado)
        yield return new WaitForSecondsRealtime(HitStopDuration);

        // Volta o tempo ao normal
        Time.timeScale = 1.0f;

        _isStopped = false;
    }
}