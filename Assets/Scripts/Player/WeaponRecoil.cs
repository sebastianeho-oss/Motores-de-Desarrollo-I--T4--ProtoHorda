using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Retroceso Base (Sin apuntar / De pie)")]
    public float recoilX = 0.5f;
    public float recoilY = 1.5f;

    [Header("Multiplicadores de Apuntado")]
    [Tooltip("Reducción de retroceso al apuntar (0.5 = 50% del retroceso base)")]
    public float aimRecoilMultiplier = 0.5f;

    [Header("Multiplicadores al Agacharse")]
    [Tooltip("Retroceso al estar agachado Y QUIETO (ej: 0.6 = 60% del retroceso base)")]
    public float crouchStillRecoilMultiplier = 0.6f;

    [Tooltip("Penalización de retroceso al estar agachado Y MOVIÉNDOSE (ej: 1.4 = 140% del retroceso base)")]
    public float crouchMovingRecoilMultiplier = 1.4f;

    [Header("Referencias")]
    public CinemachineStateController cameraController;
    public MovementPlayerController movementController;

    private void Start()
    {
        if (cameraController == null)
        {
            cameraController = GetComponentInParent<CinemachineStateController>();
        }

        if (movementController == null)
        {
            movementController = GetComponentInParent<MovementPlayerController>();
        }
    }

    public void TriggerRecoil()
    {
        float currentRecoilX = recoilX;
        float currentRecoilY = recoilY;

        // Reducción del retroceso por apuntar
        if (cameraController != null && cameraController.IsAiming)
        {
            currentRecoilX *= aimRecoilMultiplier;
            currentRecoilY *= aimRecoilMultiplier;
        }

        // Evaluación de estado agachado
        if (movementController != null && movementController.IsCrouching)
        {
            float crouchMultiplier = movementController.IsMoving
                ? crouchMovingRecoilMultiplier
                : crouchStillRecoilMultiplier;

            currentRecoilX *= crouchMultiplier;
            currentRecoilY *= crouchMultiplier;
        }

        float finalX = Random.Range(-currentRecoilX, currentRecoilX);
        float finalY = currentRecoilY;

        if (cameraController != null)
        {
            cameraController.AddRecoil(finalX, finalY);
        }
    }
}