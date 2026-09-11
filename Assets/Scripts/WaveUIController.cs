using UnityEngine;
using TMPro;

public class WaveUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Textos UI (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private TextMeshProUGUI activeWaveTimerText;

    void Update()
    {
        if (waveSpawner == null) return;

        // Actualizar número de oleada
        if (waveText != null)
        {
            waveText.text = $"Oleada: {waveSpawner.CurrentWaveNumber} / {waveSpawner.TotalWaves}";
        }

        // Actualizar enemigos restantes y eliminados
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = $"Enemigos restantes: {waveSpawner.EnemiesRemaining}";
        }

        if (enemiesKilledText != null)
        {
            enemiesKilledText.text = $"Eliminados: {waveSpawner.EnemiesKilled} / {waveSpawner.TotalEnemiesInWave}";
        }

        // Actualizar temporizador de descanso (cuenta regresiva antes de empezar)
        if (countdownTimerText != null)
        {
            if (waveSpawner.IsCountingDown)
            {
                countdownTimerText.gameObject.SetActive(true);
                countdownTimerText.text = $"Siguiente horda en: {Mathf.Ceil(waveSpawner.WaveTimer)}s";
            }
            else
            {
                countdownTimerText.gameObject.SetActive(false);
            }
        }

        // Actualizar temporizador del tiempo transcurrido *durante* la horda
        if (activeWaveTimerText != null)
        {
            if (waveSpawner.IsWaveActive)
            {
                activeWaveTimerText.gameObject.SetActive(true);

                // Formatear segundos a minutos:segundos (ej: 02:15)
                float time = waveSpawner.ActiveWaveTime;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                activeWaveTimerText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                // Opcional: Ocultarlo o mostrar en ceros cuando no esté activa la horda
                activeWaveTimerText.text = "00:00";
            }
        }
    }
}