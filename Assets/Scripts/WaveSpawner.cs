using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager; 
    [SerializeField] private int coinsPerWave = 100;//monedas tienda
    [Header("Configuración de Olas y Spawns")]
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private int activeEnemiesCount = 0;
    private int totalEnemiesInCurrentWave = 0;
    private int enemiesKilledInCurrentWave = 0;
    private bool isSpawning = false;
    private float waveTimer = 0f;
    private bool isCountingDown = false;

    // Nuevas variables para el cronómetro de la horda activa
    private float activeWaveTimer = 0f;
    private bool isWaveActive = false;

    // Propiedades públicas para la Interfaz (UI)
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => waves.Count;
    public int EnemiesRemaining => activeEnemiesCount;
    public int EnemiesKilled => enemiesKilledInCurrentWave;
    public int TotalEnemiesInWave => totalEnemiesInCurrentWave;
    public float WaveTimer => waveTimer;
    public bool IsCountingDown => isCountingDown;

    // Propiedades para el temporizador de la horda activa
    public float ActiveWaveTime => activeWaveTimer;
    public bool IsWaveActive => isWaveActive;

    private void Start()
    {
        if (waves.Count > 0)
        {
            StartCoroutine(StartNextWave());
        }
        else
        {
            Debug.LogWarning("No se asignaron WaveData al WaveSpawner.");
        }
    }

    private void Update()
    {
        // Si la horda está activa (ya pasaron los preparativos y empezó el combate), sumamos tiempo
        if (isWaveActive)
        {
            activeWaveTimer += Time.deltaTime;
        }
    }
    public void StartNextWaveFromShop()
    {
        StartCoroutine(StartNextWave());
    }

    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("¡Felicidades! Has sobrevivido a todas las olas.");
            yield break;
        }

        WaveData currentWave = waves[currentWaveIndex];

        // Reiniciamos y configuramos el temporizador de descanso antes de la ola
        waveTimer = currentWave.timeBeforeWave;
        isCountingDown = true;
        isWaveActive = false; // Aseguramos que el cronómetro de combate esté apagado en la pausa

        while (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            yield return null;
        }
        isCountingDown = false;

        // Inicia oficialmente la horda de combate
        isWaveActive = true;
        activeWaveTimer = 0f; // Reiniciamos el cronómetro de la horda actual

        // Crear y mezclar la lista de enemigos para esta ola
        List<GameObject> enemiesToSpawn = BuildEnemyList(currentWave);
        totalEnemiesInCurrentWave = enemiesToSpawn.Count;
        activeEnemiesCount = totalEnemiesInCurrentWave;
        enemiesKilledInCurrentWave = 0;
        isSpawning = true;

        foreach (GameObject prefab in enemiesToSpawn)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            if (enemyInstance.TryGetComponent<EnemyHealth>(out EnemyHealth health))
            {
                health.OnEnemyDied += OnEnemyKilled;
            }
            else
            {
                Debug.LogError($"El prefab {prefab.name} no tiene el componente EnemyHealth.");
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        isSpawning = false;
    }

    private List<GameObject> BuildEnemyList(WaveData wave)
    {
        List<GameObject> list = new List<GameObject>();

        foreach (var group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                list.Add(group.enemyPrefab);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }

    private void OnEnemyKilled()
    {
        activeEnemiesCount--;
        enemiesKilledInCurrentWave++;

        if (activeEnemiesCount <= 0 && !isSpawning)
        {
            isWaveActive = false;

            Debug.Log($"¡Ola {currentWaveIndex + 1} completada en {activeWaveTimer:F2} segundos!");

            if (shopManager != null)
            {
                shopManager.AddCoins(coinsPerWave);
            }

            currentWaveIndex++;

            if (shopManager != null)
            {
                shopManager.OpenShop();
            }
        }
    }
}


