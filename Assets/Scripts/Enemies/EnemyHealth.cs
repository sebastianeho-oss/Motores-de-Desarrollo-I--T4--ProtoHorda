using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Ajustes de Salud y Recompensa")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Economía de la Tienda")]
    [SerializeField] private int coinReward = 15; // 🪙 Monedas que otorga este enemigo al morir

    // Evento que notifica al WaveSpawner enviando la cantidad de monedas correspondientes
    public event Action<int> OnEnemyDiedWithReward;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(gameObject.name + " recibió " + amount + " de daño. Vida restante: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " ha muerto.");

        // Notificar al WaveSpawner enviando la recompensa de monedas de este enemigo
        OnEnemyDiedWithReward?.Invoke(coinReward);

        Destroy(gameObject);
    }
}