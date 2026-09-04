using UnityEngine;

public class WeaponInfo : MonoBehaviour
{
    [Tooltip("Define si esta arma usa la cámara de apuntado estándar o de sniper.")]
    public CinemachineStateController.AimCameraType cameraType = CinemachineStateController.AimCameraType.Standard;
}