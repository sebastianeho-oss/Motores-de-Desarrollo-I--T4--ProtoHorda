using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Configuración de Olas y Spawns")]
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private int activeEnemiesCount = 0;
    private bool isSpawning = false;

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

    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("¡Felicidades! Has sobrevivido a todas las olas.");
            yield break;
        }

        WaveData currentWave = waves[currentWaveIndex];
        Debug.Log($"Iniciando Ola {currentWaveIndex + 1} en {currentWave.timeBeforeWave} segundos...");
        yield return new WaitForSeconds(currentWave.timeBeforeWave);

        // Crear y mezclar la lista de enemigos para esta ola
        List<GameObject> enemiesToSpawn = BuildEnemyList(currentWave);
        activeEnemiesCount = enemiesToSpawn.Count;
        isSpawning = true;

        foreach (GameObject prefab in enemiesToSpawn)
        {
            // Elegir un punto de aparición aleatorio de los configurados
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            // Conectar el evento de muerte con el script EnemyHealth
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

        // Mezclar aleatoriamente el orden en que aparecen los enemigos
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

        // Cuando la pantalla está limpia de enemigos y ya no hay spawns pendientes
        if (activeEnemiesCount <= 0 && !isSpawning)
        {
            Debug.Log($"¡Ola {currentWaveIndex + 1} completada!");
            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }
}