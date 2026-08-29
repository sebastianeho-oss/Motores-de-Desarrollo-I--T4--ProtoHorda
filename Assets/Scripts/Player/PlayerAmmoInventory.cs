using System.Collections.Generic;
using UnityEngine;

public class PlayerAmmoInventory : MonoBehaviour
{
    [System.Serializable]
    public class AmmoSlot
    {
        public AmmoType ammoType;
        public int currentReserve = 60; // Cantidad actual
        public int maxReserve = 120;    // Límite máximo permitido
    }

    [Header("Reservas de Munición")]
    public List<AmmoSlot> startingAmmo = new List<AmmoSlot>();

    private Dictionary<AmmoType, AmmoSlot> ammoDictionary = new Dictionary<AmmoType, AmmoSlot>();

    void Awake()
    {
        // Inicializar el diccionario
        foreach (var slot in startingAmmo)
        {
            if (!ammoDictionary.ContainsKey(slot.ammoType))
            {
                // Clampear valor inicial por si en el Inspector se puso más del máximo
                slot.currentReserve = Mathf.Clamp(slot.currentReserve, 0, slot.maxReserve);
                ammoDictionary[slot.ammoType] = slot;
            }
        }
    }

    // Obtener la reserva actual
    public int GetReserveAmmo(AmmoType type)
    {
        return ammoDictionary.TryGetValue(type, out var slot) ? slot.currentReserve : 0;
    }

    // Obtener el máximo de reserva
    public int GetMaxReserveAmmo(AmmoType type)
    {
        return ammoDictionary.TryGetValue(type, out var slot) ? slot.maxReserve : 0;
    }

    // Consultar si la reserva de un tipo de munición ya está llena
    public bool IsAmmoFull(AmmoType type)
    {
        if (ammoDictionary.TryGetValue(type, out var slot))
        {
            return slot.currentReserve >= slot.maxReserve;
        }
        return true;
    }

    // Extraer munición para recargar el cargador del arma
    public int ExtractAmmo(AmmoType type, int amountNeeded)
    {
        if (!ammoDictionary.TryGetValue(type, out var slot)) return 0;

        int amountToGive = Mathf.Min(slot.currentReserve, amountNeeded);
        slot.currentReserve -= amountToGive;
        return amountToGive;
    }

    // Añadir munición asegurando no sobrepasar el máximo.
    // Devuelve la cantidad REAL que se pudo añadir.
    public int AddAmmo(AmmoType type, int amount)
    {
        if (!ammoDictionary.TryGetValue(type, out var slot)) return 0;

        int spaceAvailable = slot.maxReserve - slot.currentReserve;
        if (spaceAvailable <= 0) return 0; // Ya está completamente lleno

        int amountToAdd = Mathf.Min(spaceAvailable, amount);
        slot.currentReserve += amountToAdd;

        return amountToAdd;
    }
}