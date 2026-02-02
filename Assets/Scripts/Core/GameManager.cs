using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Config da partida")] 
    [Tooltip("Tempo em segundos para reiniciar após o K.O")]
    public float RestartDelay = 3.0f;
    
    //Lista interna de quem esta jogando
    private List<HealthComponent> _activePlayers = new List<HealthComponent>();
    private bool _isMatchOver = false;

    private void Start()
    {
        InitializeMatch();
    }

    private void InitializeMatch()
    {
        _isMatchOver = false;
        
        //1.Acha todos que tem vida na cena
        _activePlayers  = FindObjectsOfType<HealthComponent>().ToList();

        if (_activePlayers.Count < 2)
        {
            Debug.LogWarning("GameManager: Atenção! Menos de 2 jogadores encontrados. O jogo não vai acabar nunca.");
        }
        
        //2.Config da camera automaticamente
        ArenaCamera arenaCam= FindObjectOfType<ArenaCamera>();

        if (arenaCam != null)
        {
            arenaCam.Targets.Clear(); // Limpa alvos antigos

            foreach (var player in _activePlayers)
            {
                arenaCam.Targets.Add(player.transform);
            }
            Debug.Log($"GameManager: Câmera configurada com {_activePlayers.Count} alvos.");
        }
        else
        {
            Debug.LogWarning("GameManager: Nenhuma ArenaCamera encontrada na cena!");
        }
        //3. Inscrição nos Eventos
        foreach (var player in _activePlayers)
        {
            //Quando alguem morre, avisa o gerente
            player.OnDeath.AddListener(()=> OnPlayerEliminated(player));
        }
        
    }

    private void OnPlayerEliminated(HealthComponent victim)
    {
        if (_isMatchOver) return;
        
        //Conta quantos  ainda tem a vida maior que zero
        int survivors = _activePlayers.Count(p =>p.CurrentHealth <= 0);


        if (survivors <= 1)
        {
            ProcessEndMatch();
        }
    }

    private void ProcessEndMatch()
    {
        _isMatchOver = true;
        
        //Descobre quem sobrou vivo
        HealthComponent winner = _activePlayers.FirstOrDefault(p => p.CurrentHealth > 0);

        if (winner != null)
        {
            Debug.Log($"👑 FIM DE JOGO! VENCEDOR: {winner.name}");
        }
        else
        {
            Debug.Log("💀 FIM DE JOGO! EMPATE!");
        }
        
        //Agenda o reinicio
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        Debug.Log($"Reiniciando em {RestartDelay} segundos...");
        yield return new WaitForSeconds(RestartDelay);
        
        //Recarrega cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }

    private void OnDisable()
    {
        foreach (var player in _activePlayers)
        {
            if(player != null) player.OnDeath.RemoveAllListeners();
        }
    }




}
