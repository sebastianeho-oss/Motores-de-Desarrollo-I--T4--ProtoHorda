using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemy : MonoBehaviour
{
    [Header("Ajustes de Combate")]
    public float attackRange = 2f;       // Distancia a la que se detiene para atacar
    public float attackDamage = 15f;    // Daño por golpe
    public float attackRate = 1.2f;     // Cadencia de ataque (segundos entre golpes)

    [Header("Referencias")]
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Buscar al jugador por Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        // Configurar distancia de frenado en el NavMeshAgent
        agent.stoppingDistance = attackRange - 0.2f;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 1. Moverse hacia el jugador
        agent.SetDestination(playerTransform.position);

        // 2. Si está en rango de ataque, orientarse y atacar
        if (distanceToPlayer <= attackRange)
        {
            RotateTowardsPlayer();

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackRate;
                Attack();
            }
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Evitar que el enemigo se incline hacia arriba/abajo
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    void Attack()
    {
        if (playerHealth != null && !playerHealth.isDead)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"¡Enemigo cuerpo a cuerpo atacó al jugador haciendo {attackDamage} de daño!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}