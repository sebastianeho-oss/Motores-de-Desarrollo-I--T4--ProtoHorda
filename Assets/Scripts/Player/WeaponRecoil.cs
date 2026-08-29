using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí a tu jugador que contiene el AimCameraPlayerController")]
    public AimCameraPlayerController cameraController;
    [Tooltip("El modelo 3D del arma (Hijo del WeaponHolder). NO el padre.")]
    public Transform weaponModel; 

    [Header("Retroceso Normal - Cámara (Disparo desde la cadera)")]
    public float recoilX = 0.5f; // Salto horizontal aleatorio (izq/der)
    public float recoilY = 2.0f; // Salto vertical hacia arriba

    [Header("Retroceso Normal - Visual (Disparo desde la cadera)")]
    public Vector3 visualRecoilRotation = new Vector3(-10f, 0f, 0f); // Rota el cañón hacia arriba
    public Vector3 visualRecoilPosition = new Vector3(0f, 0f, -0.2f); // Empuja el arma hacia atrás

    [Header("Retroceso al Apuntar - Cámara (Valores reducidos)")]
    [Tooltip("Salto horizontal aleatorio al apuntar (usualmente un valor menor).")]
    public float aimRecoilX = 0.2f;
    [Tooltip("Salto vertical al apuntar (usualmente un valor menor).")]
    public float aimRecoilY = 0.8f;

    [Header("Retroceso al Apuntar - Visual (Valores reducidos)")]
    [Tooltip("Rotación visual al apuntar (ej: menos movimiento de cañón).")]
    public Vector3 aimVisualRecoilRotation = new Vector3(-4f, 0f, 0f);
    [Tooltip("Posición visual al apuntar (ej: menor patada hacia atrás).")]
    public Vector3 aimVisualRecoilPosition = new Vector3(0f, 0f, -0.05f);

    [Header("Propiedades del Retroceso")]
    public float snappiness = 15f; // Qué tan rápido y violento es el golpe
    public float returnSpeed = 5f;  // Qué tan rápido el arma vuelve a su posición original

    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 currentPosition;
    private Vector3 targetPosition;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Guardamos la posición y rotación original del modelo del arma
        if (weaponModel != null)
        {
            initialPosition = weaponModel.localPosition;
            initialRotation = weaponModel.localRotation;
        }
    }

    void Update()
    {
        if (weaponModel == null) return;

        // 1. Suavizar los valores objetivo de vuelta a cero con el tiempo
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);

        // 2. Interpolar los valores actuales hacia los objetivos
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, snappiness * Time.deltaTime);

        // 3. Aplicar el resultado al modelo del arma
        weaponModel.localRotation = initialRotation * Quaternion.Euler(currentRotation);
        weaponModel.localPosition = initialPosition + currentPosition;
    }

    public void TriggerRecoil()
    {
        // Consultamos si el jugador está apuntando mediante la propiedad IsAiming
        bool isAiming = cameraController != null && cameraController.IsAiming;

        // Seleccionar los valores de la cámara dependiendo de si se está apuntando
        float currentRecoilX = isAiming ? aimRecoilX : recoilX;
        float currentRecoilY = isAiming ? aimRecoilY : recoilY;

        // Seleccionar los valores visuales dependiendo de si se está apuntando
        Vector3 currentVisualRotation = isAiming ? aimVisualRecoilRotation : visualRecoilRotation;
        Vector3 currentVisualPosition = isAiming ? aimVisualRecoilPosition : visualRecoilPosition;

        // 1. Aplicar salto a la cámara
        if (cameraController != null)
        {
            float randomRecoilX = Random.Range(-currentRecoilX, currentRecoilX);
            cameraController.AddRecoil(randomRecoilX, currentRecoilY);
        }

        // 2. Aplicar golpe visual al arma
        targetRotation += currentVisualRotation;
        targetPosition += currentVisualPosition;
    }
}