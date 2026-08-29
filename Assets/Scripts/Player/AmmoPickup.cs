using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Configuración de Munición")]
    public AmmoType ammoType;    // Seleccionas en el Inspector si es Rifle, Rocket, etc.
    public int ammoAmount = 30;  // Cantidad de balas/cohetes que otorga al recogerlo

    [Header("Efecto Visual (Opcional)")]
    public float rotationSpeed = 50f; // Velocidad a la que gira el cubo en el suelo
    public float floatSpeed = 2f;     // Velocidad de flotación
    public float floatAmount = 0.25f; // Altura del suave movimiento arriba y abajo

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1. Animación simple de giro y flotación para que parezca un pickup de videojuego
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

   void OnTriggerEnter(Collider other)
    {
    PlayerAmmoInventory playerInventory = other.GetComponent<PlayerAmmoInventory>();

    if (playerInventory == null)
    {
        playerInventory = other.GetComponentInParent<PlayerAmmoInventory>();
    }

    if (playerInventory != null)
        {
        // Intenta añadir la munición y nos dice cuántas pildoras/cohetes se añadieron realmente
        int amountAdded = playerInventory.AddAmmo(ammoType, ammoAmount);

        if (amountAdded > 0)
            {
            Debug.Log($"¡Recogiste {amountAdded} unidades de munición para {ammoType}!");
            
            // Opcional: Reproducir sonido de recoger aquí
            
            Destroy(gameObject); // Solo se destruye si el jugador pudo aprovechar munición
            }
        else
            {
            Debug.Log($"Reserva de {ammoType} está al máximo ({playerInventory.GetMaxReserveAmmo(ammoType)}). Caja no recogida.");
            }
        }
    }
}