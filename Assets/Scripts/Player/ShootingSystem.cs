using System.Collections;
using UnityEngine;

public class ShootingSystem : MonoBehaviour
{
    // Enum para seleccionar el modo de disparo en el Inspector
    public enum FireMode { SemiAutomatic, FullAutomatic, Burst }

    [Header("Permisos Locales")]
    public bool canShoot = true;

    [Header("Referencias de Origen")]
    public Transform firePoint;
    public Camera mainCamera;
    public LineRenderer lineRenderer;
    public PlayerHealth playerHealth;

    [Header("Modo de Disparo")]
    [Tooltip("Elige el comportamiento de disparo del arma.")]
    public FireMode fireMode = FireMode.FullAutomatic;

    [Header("Ajustes de Ráfaga (Solo si FireMode = Burst)")]
    [Tooltip("Cantidad de balas a disparar por cada ráfaga (ej: 2 o 3).")]
    public int burstCount = 3;
    [Tooltip("Tiempo de espera entre cada bala dentro de la ráfaga.")]
    public float burstDelay = 0.08f;

    [Header("Ajustes de Disparo General")]
    public float damage = 25f;
    public float maxShootDistance = 100f;
    [Tooltip("Tiempo de espera entre disparos/ráfagas.")]
    public float fireRate = 0.15f;
    public float tracerDuration = 0.04f;
    public LayerMask shootableMask;

    private float nextFireTime = 0f;
    private bool isShootingBurst = false; // Bloquea nuevos disparos durante una ráfaga

    public WeaponRecoil weaponRecoil;
    private WeaponAmmo weaponAmmo;

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
       
        weaponAmmo = GetComponent<WeaponAmmo>();

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
    }

    void Update()
    {
        // Cancelar si no hay permiso, si está realizando una ráfaga, si murió o si está en pausa
        if (!canShoot || isShootingBurst || (playerHealth != null && playerHealth.isDead) || GameManager.IsPaused) return;

        // Determinar si se detecta mantenido o un solo clic según el modo activo
        bool isTriggerPressed = (fireMode == FireMode.FullAutomatic) 
            ? Input.GetButton("Fire1")       // Mantiene presionado
            : Input.GetButtonDown("Fire1");   // Clic único (Semi y Burst)

        if (isTriggerPressed && Time.time >= nextFireTime)
        {
            // Validar munición antes de intentar disparar
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                return; 
            }

            // Ejecutar según el modo seleccionado
            if (fireMode == FireMode.Burst)
            {
                StartCoroutine(FireBurstRoutine());
            }
            else
            {
                ExecuteSingleShot();
            }
        }
    }

    // Realiza un solo disparo y calcula la cadencia normal
    private void ExecuteSingleShot()
    {
        nextFireTime = Time.time + fireRate;
        Shoot();

        if (weaponAmmo != null) weaponAmmo.ConsumeBullet();
    }

    // Corrutina encargada de procesar las ráfagas
    private IEnumerator FireBurstRoutine()
    {
        isShootingBurst = true;

        for (int i = 0; i < burstCount; i++)
        {
            // Romper ráfaga si se queda sin balas
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                break;
            }

            Shoot();

            if (weaponAmmo != null) weaponAmmo.ConsumeBullet();

            // Espera entre balas dentro de la ráfaga
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }

        // Establece el cooldown general tras completar la ráfaga
        nextFireTime = Time.time + fireRate;
        isShootingBurst = false;
    }

    void Shoot()
    {
        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;
        bool hasCameraHit = Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxShootDistance, shootableMask);

        if (hasCameraHit)
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(maxShootDistance);
        }

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
        Vector3 finalImpactPoint = targetPoint;

        float distanceToTarget = Vector3.Distance(firePoint.position, targetPoint);
        bool hasFireHit = Physics.Raycast(firePoint.position, shootDirection, out RaycastHit fireHit, distanceToTarget, shootableMask);

        Collider hitCollider = null;

        if (hasFireHit)
        {
            finalImpactPoint = fireHit.point;
            hitCollider = fireHit.collider;
        }
        else if (hasCameraHit)
        {
            hitCollider = cameraHit.collider;
        }

        if (hitCollider != null)
        {
            EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        if (lineRenderer != null)
        {
            StartCoroutine(RenderTracerLine(firePoint.position, finalImpactPoint));
        }

        if (weaponRecoil != null)
        {
           weaponRecoil.TriggerRecoil();
        }
    }

    private IEnumerator RenderTracerLine(Vector3 startPoint, Vector3 endPoint)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        yield return new WaitForSeconds(tracerDuration);

        lineRenderer.enabled = false;
    }
}