using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;

    [Header("Configuración de Olas y Spawns")]
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de la Tienda (Estilo Killing Floor)")]
    [SerializeField] private GameObject shopPodPrefab;
    [SerializeField] private Transform[] shopSpawnPoints;
    private GameObject activeShopPod;

    private int currentWaveIndex = 0;
    private int activeEnemiesCount = 0;
    private int totalEnemiesInCurrentWave = 0;
    private int enemiesKilledInCurrentWave = 0;
    private bool isSpawning = false;
    private float waveTimer = 0f;
    private bool isCountingDown = false;

    private float activeWaveRemainingTime = 0f;
    private bool isWaveActive = false;

    private PlayerInputActions inputActions;

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => waves.Count;
    public int EnemiesRemaining => activeEnemiesCount;
    public int EnemiesKilled => enemiesKilledInCurrentWave;
    public int TotalEnemiesInWave => totalEnemiesInCurrentWave;
    public float WaveTimer => waveTimer;
    public bool IsCountingDown => isCountingDown;
    public float ActiveWaveRemainingTime => activeWaveRemainingTime;
    public bool IsWaveActive => isWaveActive;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.PassTime.performed += _ => SkipCountdown();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

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
        if (isWaveActive)
        {
            activeWaveRemainingTime -= Time.deltaTime;

            if (activeWaveRemainingTime <= 0f)
            {
                activeWaveRemainingTime = 0f;
            }
        }
    }

    public void SkipCountdown()
    {
        // Solo permite omitir el tiempo si está en la cuenta regresiva antes de iniciar la oleada
        if (isCountingDown)
        {
            waveTimer = 0f;
        }
    }

    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("¡Felicidades! Has sobrevivido a todas las olas.");
            yield break;
        }

        WaveData currentWave = waves[currentWaveIndex];

        // --- FASE DE DESCANSO / TIENDA ---
        waveTimer = currentWave.timeBeforeWave;
        isCountingDown = true;
        isWaveActive = false;

        SpawnShopPod();

        while (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            yield return null;
        }
        isCountingDown = false;

        CloseAndDespawnShop();

        // --- FASE DE COMBATE ---
        isWaveActive = true;
        activeWaveRemainingTime = currentWave.waveDuration;

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
                health.OnEnemyDiedWithReward += (coinReward) => OnEnemyKilled(coinReward);
            }
            else
            {
                Debug.LogError("El prefab " + prefab.name + " no tiene el componente EnemyHealth.");
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnShopPod()
    {
        if (shopPodPrefab != null && shopSpawnPoints.Length > 0)
        {
            Transform randomPoint = shopSpawnPoints[Random.Range(0, shopSpawnPoints.Length)];
            activeShopPod = Instantiate(shopPodPrefab, randomPoint.position, randomPoint.rotation);
        }
    }

    private void CloseAndDespawnShop()
    {
        if (shopManager != null)
        {
            shopManager.ForceCloseShop();
        }

        if (activeShopPod != null)
        {
            Destroy(activeShopPod);
        }
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

    private void OnEnemyKilled(int coinReward)
    {
        activeEnemiesCount--;
        enemiesKilledInCurrentWave++;

        if (shopManager != null)
        {
            shopManager.AddCoins(coinReward);
        }

        if (activeEnemiesCount <= 0 && !isSpawning)
        {
            isWaveActive = false;

            Debug.Log("¡Ola " + (currentWaveIndex + 1) + " completada!");

            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }
}