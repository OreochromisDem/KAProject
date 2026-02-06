using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Config", menuName = "Audio/Audio Config")]
public class AudioConfigSO : ScriptableObject
{
    [Header("Arquivos de Áudio")]
    [Tooltip("Arraste multiplos sons aui para ele escolher um aleatório")]
    public AudioClip[] Clips;

    [Header("Variação")] [Range(0f, 1f)] public float Volume = 1f;
    
    [Tooltip("Variação de tom")]
    public Vector2 PitchRange = new Vector2(0.9f, 1.1f);
    
    
    //Metódo para aplicar as configs numa fonte de audio
    public void ApplyTo(AudioSource source)
    {
        if (Clips.Length == 0) return;
        
        //Escolhe um clipe aleatório da lista
        source.clip = Clips[Random.Range(0,Clips.Length)];
        source.volume = Volume;
        
        //Aplica pitch
        source.pitch = Random.Range(PitchRange.x, PitchRange.y);
    }
        

}
