using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Ajustes de Salud")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Método público para recibir daño desde las armas o proyectiles del jugador
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

        // Opcional: Aquí puedes instanciar partículas de muerte o soltar munición/botiquines

        Destroy(gameObject); // Destruye el objeto del enemigo de la escena
    }
}