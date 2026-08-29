using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Ajustes del Proyectil")]
    public float speed = 20f;
    public float damage = 15f;
    public float lifeTime = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Asignar velocidad constante
        rb.linearVelocity = transform.forward * speed;

        // Autodestrucción por tiempo si no choca con nada
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Ignorar si la bala toca a otro enemigo o a otra bala
        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
        {
            return;
        }

        // 2. Buscar PlayerHealth en el objeto impactado, en sus padres o en sus hijos
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInChildren<PlayerHealth>();
        }

        // 3. Si se encontró el PlayerHealth, aplicar daño
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"¡Proyectil enemigo dañó al jugador! -{damage} HP");
            
            Destroy(gameObject); // Destruir la bala tras dañar
            return;
        }

        // 4. Si choca contra el suelo, paredes u obstáculos del mapa
        if (!other.isTrigger) 
        {
            Destroy(gameObject);
        }
    }
}