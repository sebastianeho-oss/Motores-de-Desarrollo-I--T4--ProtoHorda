using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Requerido para PlayerInputActions

public class ShotgunShootingSystem : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canShoot = true;

    [Header("Referencias de Origen")]
    public Transform firePoint;
    public Camera mainCamera;
    public PlayerHealth playerHealth;

    [Header("Ajustes de Escopeta")]
    [Tooltip("Cantidad de perdigones por cartucho.")]
    public int pelletsPerShot = 10;

    [Tooltip("Ángulo cónico máximo de dispersión en grados.")]
    public float spreadAngle = 7f;

    [Tooltip("Daño base de cada perdigón.")]
    public float damagePerPellet = 12f;

    [Tooltip("Alcance máximo efectivo.")]
    public float maxShootDistance = 18f;

    [Tooltip("Cadencia de disparo.")]
    public float fireRate = 0.7f;

    [Tooltip("Aplica caída de daño a larga distancia.")]
    public bool applyDamageFalloff = true;

    [Header("Visuales de Trazadores")]
    public GameObject tracerPrefab;
    public float tracerDuration = 0.04f;

    [Header("Máscaras")]
    public LayerMask shootableMask;

    private float nextFireTime = 0f;

    public WeaponRecoil weaponRecoil;
    private WeaponAmmo weaponAmmo;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        weaponAmmo = GetComponent<WeaponAmmo>();
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();
    }

    void Update()
    {
        if ((ShopManager.Instance != null && ShopManager.Instance.IsShopOpen) ||
            !canShoot || (playerHealth != null && playerHealth.isDead) || GameManager.IsPaused)
        {
            return;
        }

        bool isTriggerPressed = inputActions.Player.Fire.WasPressedThisFrame();

        if (isTriggerPressed && Time.time >= nextFireTime)
        {
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                return;
            }

            ExecuteShotgunShot();
        }
    }

    private void ExecuteShotgunShot()
    {
        nextFireTime = Time.time + fireRate;
        Shoot();

        if (weaponAmmo != null) weaponAmmo.ConsumeBullet();
    }

    void Shoot()
    {
        if (mainCamera == null || firePoint == null) return;

        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxShootDistance, shootableMask))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(maxShootDistance);
        }

        Vector3 baseDirection = (targetPoint - firePoint.position).normalized;

        Dictionary<EnemyHealth, float> damageMap = new Dictionary<EnemyHealth, float>();

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 pelletDirection = GetSpreadDirection(baseDirection, spreadAngle);

            if (Physics.Raycast(firePoint.position, pelletDirection, out RaycastHit pelletHit, maxShootDistance, shootableMask))
            {
                Collider hitCollider = pelletHit.collider;

                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth == null) enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    float distance = pelletHit.distance;
                    float pelletDamage = damagePerPellet;

                    if (applyDamageFalloff)
                    {
                        float distanceRatio = Mathf.Clamp01(distance / maxShootDistance);
                        pelletDamage *= Mathf.Lerp(1.0f, 0.2f, distanceRatio);
                    }

                    if (!damageMap.ContainsKey(enemyHealth))
                        damageMap[enemyHealth] = 0f;

                    damageMap[enemyHealth] += pelletDamage;
                }

                RenderTracer(firePoint.position, pelletHit.point);
            }
            else
            {
                RenderTracer(firePoint.position, firePoint.position + (pelletDirection * maxShootDistance));
            }
        }

        float shopMultiplier = (ShopManager.Instance != null) ? ShopManager.Instance.GetPlayerDamageMultiplier() : 1f;

        foreach (KeyValuePair<EnemyHealth, float> entry in damageMap)
        {
            entry.Key.TakeDamage(entry.Value * shopMultiplier);
        }

        if (weaponRecoil != null) weaponRecoil.TriggerRecoil();
    }

    private Vector3 GetSpreadDirection(Vector3 baseDir, float angle)
    {
        Vector2 randomCircle = Random.insideUnitCircle * Mathf.Tan(angle * Mathf.Deg2Rad);
        Quaternion rotation = Quaternion.LookRotation(baseDir);
        Vector3 spreadVector = rotation * new Vector3(randomCircle.x, randomCircle.y, 1f);
        return spreadVector.normalized;
    }

    private void RenderTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab != null)
        {
            GameObject tracer = Instantiate(tracerPrefab, start, Quaternion.identity);
            LineRenderer lr = tracer.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
            }
            Destroy(tracer, tracerDuration);
        }
    }
}