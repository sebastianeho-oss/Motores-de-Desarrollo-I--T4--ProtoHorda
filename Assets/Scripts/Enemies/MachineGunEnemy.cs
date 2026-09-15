using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MachineGunEnemy : MonoBehaviour
{
    [Header("Ajustes de Disparo (Ametralladora)")]
    public float shootingRange = 15f;      // Rango de visión/disparo
    public float fireRate = 0.1f;         // Cadencia rápida (ej: 0.1s = 10 balas/segundo)
    public float damage = 5f;             // Daño por cada impacto
    public float bulletSpread = 0.05f;     // Dispersión/Imprecisión de las balas
    public float tracerDuration = 0.04f;   // Cuánto tiempo dura visible la línea del disparo

    [Header("Capas de Colisión")]
    public LayerMask collisionLayers;      // Selecciona aquí Ground y Environment en el Inspector

    [Header("Referencias Visuales y Componentes")]
    public Transform firePoint;            // Punto de origen del disparo (cañón)
    public LineRenderer lineRenderer;      // Componente para dibujar la trazadora
    public ParticleSystem muzzleFlash;    // (Opcional) Partículas de chispas en el cañón
    public GameObject impactEffect;        // (Opcional) Prefab de chispas/polvo al impactar
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextFireTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        agent.stoppingDistance = shootingRange - 2f;

        // Asegurar que el LineRenderer esté oculto al iniciar
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Moverse hacia el jugador
        agent.SetDestination(playerTransform.position);

        // Si está en rango de disparo
        if (distanceToPlayer <= shootingRange)
        {
            RotateTowardsPlayer();

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootHitscan();
            }
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }

    void ShootHitscan()
    {
        if (firePoint == null) return;

        //Calcula dirección hacia el centro del jugador + Dispersión
        Vector3 targetCenter = playerTransform.position + Vector3.up * 1f; // Apuntar al pecho
        Vector3 baseDirection = (targetCenter - firePoint.position).normalized;

        // Añadir imprecisión aleatoria
        Vector3 spreadDirection = baseDirection + new Vector3(
            Random.Range(-bulletSpread, bulletSpread),
            Random.Range(-bulletSpread, bulletSpread),
            Random.Range(-bulletSpread, bulletSpread)
        );

        Vector3 endPoint;

        // Realizar el Raycast incluyendo la LayerMask (Ground y Environment)        
        if (Physics.Raycast(firePoint.position, spreadDirection, out RaycastHit hit, shootingRange, collisionLayers))
        {
            endPoint = hit.point;

            // Comprobar si golpeó al jugador
            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Instanciar efecto visual de impacto
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // Si no golpea nada dentro del rango, la línea va hasta el límite máximo
            endPoint = firePoint.position + spreadDirection * shootingRange;
        }

        // Efectos visuales de disparo
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (lineRenderer != null)
        {
            StartCoroutine(ShowTracer(firePoint.position, endPoint));
        }
    }

    IEnumerator ShowTracer(Vector3 startPos, Vector3 endPos)
    {
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(tracerDuration);

        lineRenderer.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}