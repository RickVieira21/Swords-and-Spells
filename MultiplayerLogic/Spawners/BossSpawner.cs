using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//Script associado ao Spawner responsável por spawnar o inimigo da quinta e última fase, o Boss.
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class BossSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject BossPrefab;
    [SerializeField] private Transform bossSpawnPosition;
    public static bool playerEnteredBossArea = false;


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredBossArea && other.CompareTag("Player") && IsServer)
        {
            SpawnBoss(bossSpawnPosition.position);
            playerEnteredBossArea = true;
        }
    }

    private void SpawnBoss(Vector3 spawnPos)
    {
        var bossInstance = Instantiate(BossPrefab, spawnPos, Quaternion.identity);
        var bossNetworkObject = bossInstance.GetComponent<NetworkObject>();
        if (IsServer)
        {
            bossNetworkObject.SpawnWithOwnership(OwnerClientId);
            Debug.LogWarning("BOSS SPAWNED");
        }
        else
        {
            Destroy(bossInstance.gameObject);
        }

    }
}
