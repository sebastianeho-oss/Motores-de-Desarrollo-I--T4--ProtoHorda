using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canShoot = true;

    [Header("Referencias")]
    public Transform firePoint;
    public Camera mainCamera;
    public GameObject rocketPrefab;
    public PlayerHealth playerHealth;

    [Header("Ajustes de Disparo")]
    public float fireRate = 0.8f;
    public float maxAimDistance = 100f;
    public LayerMask shootableMask;

    private float nextFireTime = 0f;
    private WeaponAmmo weaponAmmo;

    void Start()
    {
        weaponAmmo = GetComponent<WeaponAmmo>();

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
    }

    void Update()
    {
        // Cancelar si no hay permiso, el jugador está muerto o el juego pausado
        if (!canShoot || (playerHealth != null && playerHealth.isDead) || GameManager.IsPaused) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                return; 
            }

            nextFireTime = Time.time + fireRate;
            ShootRocket();
            
            if (weaponAmmo != null) weaponAmmo.ConsumeBullet();
        }        
    }

    void ShootRocket()
    {
        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxAimDistance, shootableMask))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(maxAimDistance);
        }

        Vector3 launchDirection = (targetPoint - firePoint.position).normalized;
        Quaternion launchRotation = Quaternion.LookRotation(launchDirection);
        Instantiate(rocketPrefab, firePoint.position, launchRotation);
    }
}