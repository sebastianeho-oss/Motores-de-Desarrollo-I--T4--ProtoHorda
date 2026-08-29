using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Configuración de Curación")]
    public float healAmount = 25f; // Cantidad de vida que recupera

    [Header("Efecto Visual (Opcional)")]
    public float rotationSpeed = 50f;
    public float floatSpeed = 2f;
    public float floatAmount = 0.25f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Animación de flotación y rotación (estilo videojuego)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        // Buscar el componente PlayerHealth en el jugador o en sus objetos padres
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            // Solo curar si el jugador no está muerto y le falta vida
            if (!playerHealth.isDead && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.Heal(healAmount);
                Debug.Log($"¡Botiquín recogido! Curado: {healAmount} de salud.");

                // Opcional: Reproducir efecto de sonido o partículas de curación aquí

                Destroy(gameObject); // Se consume el botiquín
            }
            else if (playerHealth.currentHealth >= playerHealth.maxHealth)
            {
                Debug.Log("La vida del jugador ya está al máximo. Botiquín no recogido.");
            }
        }
    }
}