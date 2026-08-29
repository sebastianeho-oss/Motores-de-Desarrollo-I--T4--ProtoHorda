using UnityEngine;
using TMPro; // Necesario para trabajar con TextMeshPro

public class AmmoUI : MonoBehaviour
{
    [Header("Componentes de Texto UI")]
    public TextMeshProUGUI weaponNameText; // Muestra el nombre del arma
    public TextMeshProUGUI ammoText;       // Muestra "30 / 60"

    [Header("Referencias del Jugador")]
    public WeaponSwitcher weaponSwitcher;       // Para saber qué arma está activa
    public PlayerAmmoInventory playerInventory; // Para leer la reserva de munición

    [Header("Colores Visuales (Opcional)")]
    public Color normalColor = Color.white;
    public Color reloadingColor = Color.yellow;
    public Color emptyColor = Color.red;

    void Update()
    {
        if (weaponSwitcher == null || playerInventory == null) return;

        // 1. Obtener el arma actualmente seleccionada desde el WeaponSwitcher
        if (weaponSwitcher.selectedWeapon >= weaponSwitcher.transform.childCount) return;

        Transform activeWeaponTransform = weaponSwitcher.transform.GetChild(weaponSwitcher.selectedWeapon);
        
        if (activeWeaponTransform == null || !activeWeaponTransform.gameObject.activeSelf) return;

        // 2. Leer el componente WeaponAmmo del arma activa
        WeaponAmmo activeAmmo = activeWeaponTransform.GetComponent<WeaponAmmo>();

        if (activeAmmo != null)
        {
            // Actualizar Nombre del Arma (usa el nombre del GameObject en la Jerarquía)
            if (weaponNameText != null)
            {
                weaponNameText.text = activeWeaponTransform.gameObject.name.ToUpper();
            }

            // Actualizar Contador de Munición
            if (ammoText != null)
            {
                int currentInMag = activeAmmo.currentAmmo;
                int reserve = playerInventory.GetReserveAmmo(activeAmmo.ammoType);

                // Estado: Recargando
                if (activeAmmo.IsReloading)
                {
                    ammoText.text = "RECARGANDO...";
                    ammoText.color = reloadingColor;
                }
                // Estado: Sin munición
                else if (currentInMag == 0 && reserve == 0)
                {
                    ammoText.text = "SIN MUNICIÓN";
                    ammoText.color = emptyColor;
                }
                // Estado: Normal (Ejemplo: "30 / 120")
                else
                {
                    ammoText.text = $"{currentInMag} <size=70%><color=#A0A0A0>/ {reserve}</color></size>";
                    ammoText.color = (currentInMag == 0) ? emptyColor : normalColor;
                }
            }
        }
    }
}