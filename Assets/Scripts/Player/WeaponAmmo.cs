using System.Collections;
using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canReload = true;

    [Header("Configuración del Arma")]
    public AmmoType ammoType;
    public int magazineSize = 30;
    public float reloadTime = 2.0f;

    [Header("Estado Actual")]
    public int currentAmmo;
    private bool isReloading = false;

    [Header("Referencias")]
    public PlayerHealth playerHealth;
    private PlayerAmmoInventory playerInventory;

    public bool IsReloading => isReloading;

    void Start()
    {
        playerInventory = GetComponentInParent<PlayerAmmoInventory>();
        currentAmmo = magazineSize;

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (isReloading) return;

        // Evaluar permiso (vivo, canReload activo y NO pausado)
        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        bool allowedToReload = canReload && isAlive && !GameManager.IsPaused;

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize && allowedToReload)
        {
            TryReload();
        }
    }

    public bool CanShoot()
    {
        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        return !isReloading && currentAmmo > 0 && isAlive && !GameManager.IsPaused;
    }

    public void ConsumeBullet()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
        }
    }

    public void TryReload()
    {
        if (playerInventory == null) return;

        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        if (!canReload || !isAlive || GameManager.IsPaused) return;

        int reserve = playerInventory.GetReserveAmmo(ammoType);
        if (reserve > 0 && currentAmmo < magazineSize)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log($"Recargando {ammoType}...");

        yield return new WaitForSeconds(reloadTime);

        // Si el jugador muere durante la recarga, cancelarla
        if (playerHealth != null && playerHealth.isDead)
        {
            isReloading = false;
            yield break;
        }

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoExtracted = playerInventory.ExtractAmmo(ammoType, ammoNeeded);

        currentAmmo += ammoExtracted;
        isReloading = false;
        
        Debug.Log($"Recarga completada. {ammoType}: {currentAmmo}/{magazineSize}");
    }
}