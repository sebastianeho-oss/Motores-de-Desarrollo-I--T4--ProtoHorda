using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canSwitchWeapon = true;

    [Header("Referencias")]
    public PlayerHealth playerHealth;

    [Header("Arma Seleccionada")]
    public int selectedWeapon = 0;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        SelectWeapon();
    }

    void Update()
    {
        // Cancelar en caso de bloqueo local, muerte o pausa
        if (!canSwitchWeapon || (playerHealth != null && playerHealth.isDead) || GameManager.IsPaused) return;

        int previousSelectedWeapon = selectedWeapon;

        // 1. Cambio con la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        else if (scroll < 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }

        // 2. Cambio con teclas numéricas
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2)
        {
            selectedWeapon = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3)
        {
            selectedWeapon = 2;
        }

        // 3. Actualizar
        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }
}