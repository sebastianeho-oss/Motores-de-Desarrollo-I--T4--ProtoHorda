using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("Configuración de Escudo")]
    public float maxShield = 100f;
    public float currentShield;

    [Header("Mitigación de Daño")]
    [Range(0f, 1f)]
    public float shieldMitigation = 0.70f; // 70% mitigado por el escudo

    [Header("Referencias")]
    public PlayerHealth playerHealth; // Referencia a tu PlayerHealth existente

    void Start()
    {
        currentShield = maxShield;

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    // Método para recibir daño (reemplaza o actúa como puente antes de PlayerHealth)
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        // Si el jugador tiene vida o escudo activo (puedes validar si está muerto con playerHealth)
        if (playerHealth != null && playerHealth.isDead) return;

        if (currentShield > 0f)
        {
            float damageToShield = amount * shieldMitigation;
            float damageToHealth = amount * (1f - shieldMitigation);

            currentShield -= damageToShield;

            // Si el daño quiebra el escudo por completo
            if (currentShield <= 0f)
            {
                float excessDamage = -currentShield; // Lo que sobró de romper el escudo
                currentShield = 0f;

                // Aplicamos el daño a la salud sumando el excedente o el 30% restante
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToHealth + excessDamage);
                }
            }
            else
            {
                // El escudo sigue con vida, aplicamos el 30% restante a la salud
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToHealth);
                }
            }

            Debug.Log($"Escudo mitigó el impacto. Escudo actual: {currentShield} / {maxShield}");
        }
        else
        {
            // Si el escudo está en 0, el daño pasa al 100% a la vida del jugador
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(amount);
            }

            Debug.Log("¡Escudo agotado! El daño pasó íntegro (100%) a la vida.");
        }
    }

    public void RechargeShield(float amount)
    {
        currentShield += amount;
        currentShield = Mathf.Clamp(currentShield, 0f, maxShield);
    }
}