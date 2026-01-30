using System;
using UnityEngine;
using System.Collections;
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private Renderer _renderer;
    private Color _originalColor;

    private void Awake()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
    }


    // Essa é a função que o Player vai chamar quando acertar o soco
    public void TakeDamage(float damageAmount)
    {
        Debug.Log($"[DUMMY] Aii! Tomei {damageAmount} de dano.");
        StartCoroutine(FlashRed());

    }

    private IEnumerator FlashRed()
    {
        _renderer.material.color = Color.white; // Pisca branco
        yield return new WaitForSeconds(0.1f);
        _renderer.material.color = _originalColor;
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
