using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Estados y Permisos")]
    public bool isInvincible = false; // Interruptor de invencibilidad
    public bool isDead = false;

    [Header("Referencias")]
    public GameManager gameManager;

    void Start()
    {
        currentHealth = maxHealth;

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    public void TakeDamage(float amount)
    {
        // Si el jugador está muerto o la invencibilidad está activa, ignorar el daño
        if (isDead || isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Jugador recibió {amount} de daño. Salud actual: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("El jugador ha muerto.");

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
}