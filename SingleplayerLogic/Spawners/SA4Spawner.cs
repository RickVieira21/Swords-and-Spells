using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//Script que representa o Spawner que instancia o Boss, no Modo Singleplayer
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class SA4Spawner : MonoBehaviour
{
    [SerializeField] private GameObject BossPrefab;
    [SerializeField] private Transform bossSpawnPosition;
    public static bool playerEnteredBossArea = false;


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredBossArea && other.CompareTag("SinglePlayer"))
        {
            SpawnBoss(bossSpawnPosition.position);
            playerEnteredBossArea = true;
        }
    }

    private void SpawnBoss(Vector3 spawnPos)
    {
        Instantiate(BossPrefab, spawnPos, Quaternion.identity);
    }
}
