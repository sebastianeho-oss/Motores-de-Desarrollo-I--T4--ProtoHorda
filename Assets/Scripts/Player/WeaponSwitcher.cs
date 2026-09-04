using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canSwitchWeapon = true;

    [Header("Referencias")]
    public PlayerHealth playerHealth;

    [Header("Arma Seleccionada")]
    public int selectedWeapon = 0;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.SelectWeapon.performed += ctx => OnScroll(ctx.ReadValue<float>());

        inputActions.Player.Weapon1.performed += _ => SelectWeaponIndex(0);
        inputActions.Player.Weapon2.performed += _ => SelectWeaponIndex(1);
        inputActions.Player.Weapon3.performed += _ => SelectWeaponIndex(2);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        SelectWeapon();
    }

    private void OnScroll(float scrollValue)
    {
        if (!CanSwitch()) return;

        int previousSelected = selectedWeapon;

        if (scrollValue > 0f)
        {
            selectedWeapon = (selectedWeapon >= transform.childCount - 1) ? 0 : selectedWeapon + 1;
        }
        else if (scrollValue < 0f)
        {
            selectedWeapon = (selectedWeapon <= 0) ? transform.childCount - 1 : selectedWeapon - 1;
        }

        if (previousSelected != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    private void SelectWeaponIndex(int index)
    {
        if (!CanSwitch()) return;

        if (index < transform.childCount && selectedWeapon != index)
        {
            selectedWeapon = index;
            SelectWeapon();
        }
    }

    private bool CanSwitch()
    {
        return canSwitchWeapon && (playerHealth == null || !playerHealth.isDead) && !GameManager.IsPaused;
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(i == selectedWeapon);
            i++;
        }
    }
}