using System; // <-- Necesario para el evento Action
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Ajustes de Salud")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    // Evento que notifica al WaveSpawner cuándo este enemigo muere
    public event Action OnEnemyDied;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"{gameObject.name} recibió {amount} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} ha muerto.");

        // Notificar al WaveSpawner que este enemigo murió
        OnEnemyDied?.Invoke();

        Destroy(gameObject);
    }
}