using UnityEngine;

public class AudioManager : MonoBehaviour
{
   public static AudioManager Instance;

   [Header("Setup")] [SerializeField] private AudioSource _sfxObjectPrefab;


   private void Awake()
   {
      if(Instance == null) Instance = this;
      else Destroy(gameObject);
   }

   public void PlaySFX(AudioConfigSO audioData, Vector3 position)
   {
      if (audioData == null) return;
      //  Cria um objeto vazio com AudioSource na posição desejada
      AudioSource newSource = Instantiate(_sfxObjectPrefab,position,Quaternion.identity);
      
      // Aplica as configurações do SO (Volume, Pitch, Clip Aleatório)
      audioData.ApplyTo(newSource);
      
      //Toca
      newSource.Play();
      
      //Destroi o objeto assim que o som acabar (Limpeza de memória)
      Destroy(newSource.gameObject, newSource.clip.length + 0.1f);
   }
}
