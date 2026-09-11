using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyGroup
{
    public GameObject enemyPrefab; // Prefab con MeleeEnemy, RangedEnemy, FloatingEnemy o MachineGunEnemy
    public int count;               // Cantidad de este tipo en la ronda
}

[CreateAssetMenu(fileName = "NuevaOleada", menuName = "Horda/Wave Data")]
public class WaveData : ScriptableObject
{
    public List<EnemyGroup> enemyGroups;
    public float spawnInterval = 1.0f;   // Tiempo entre la aparición de cada enemigo
    public float timeBeforeWave = 3.0f;  // Tiempo antes de iniciar la siguiente ronda
}