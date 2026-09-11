using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rocket : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 35f;
    public float maxLifetime = 6f;

    [Header("Efectos y Área de Explosión")]
    public GameObject explosionPrefab;
    public float explosionRadius = 6f;
    public float explosionForce = 800f;
    public float damage = 100f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Darle velocidad hacia adelante desde el momento de instanciarlo
        rb.linearVelocity = transform.forward * speed;

        // Evitar que el cohete se quede flotando eternamente en la escena
        Destroy(gameObject, maxLifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Al chocar contra cualquier objeto (pared, suelo, enemigo) se detona
        Explode();
    }

    void Explode()
    {
        // Instanciar partículas de explosión
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Detectar todos los objetos dentro del radio de explosión
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        // Registro para evitar aplicar daño duplicado al mismo enemigo si tiene varios colliders
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (Collider hit in hitColliders)
        {
            // Aplicar empuje a objetos que tengan Rigidbody
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Aplicar daño a enemigos dentro del área
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health == null)
            {
                health = hit.GetComponentInParent<EnemyHealth>();
            }

            if (health != null && !damagedEnemies.Contains(health))
            {
                health.TakeDamage(damage);
                damagedEnemies.Add(health);
            }
        }
                
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}