using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private PlayerInputActions inputActions;

    // Referencia a la corrutina de recarga
    private Coroutine reloadCoroutine;

    public bool IsReloading => isReloading;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        // Garantizar que el arma no inicie trabada al cambiar a ella
        isReloading = false;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        // Cancelar recarga automáticamente si se enfunda o cambia de arma
        CancelReload();
    }

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

        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        bool allowedToReload = canReload && isAlive && !GameManager.IsPaused;

        if (inputActions.Player.Reload.WasPressedThisFrame() && currentAmmo < magazineSize && allowedToReload)
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
            if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
            reloadCoroutine = StartCoroutine(ReloadRoutine());
        }
    }

    // Método público para cancelar la recarga externamente si lo necesitas
    public void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        isReloading = false;
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log($"Recargando {ammoType}...");

        yield return new WaitForSeconds(reloadTime);

        if (playerHealth != null && playerHealth.isDead)
        {
            isReloading = false;
            yield break;
        }

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoExtracted = playerInventory.ExtractAmmo(ammoType, ammoNeeded);

        currentAmmo += ammoExtracted;
        isReloading = false;
        reloadCoroutine = null;

        Debug.Log($"Recarga completada. {ammoType}: {currentAmmo}/{magazineSize}");
    }
}