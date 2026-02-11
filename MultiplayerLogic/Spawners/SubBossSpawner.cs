using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Script associado ao Spawner responsável por spawnar os inimigos da terceira área, os Sub-Bosses
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class SubBossSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject subBossPrefab;
    [SerializeField] private Transform topPositionTransform;
    [SerializeField] private Transform bottomPositionTransform;
    [SerializeField] private int numOfSubBosses;
    public static bool playerEnteredSubBossArea = false;

    [SerializeField] private ParticleSystem fog;
    [SerializeField] private BoxCollider barrier;


    private void Update()
    {
        if (playerEnteredSubBossArea)
        {
            Debug.Log("SubBoss List Size: " + GameObject.FindGameObjectsWithTag("SubBoss").Length);
            if (GameObject.FindGameObjectsWithTag("SubBoss").Length <= 0)
            {
                if (IsServer)
                {
                    DeactivateBarrier();
                }
                else if (IsClient)
                {
                    DeactivateBarrierServerRPC();
                }
            }

        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredSubBossArea && other.CompareTag("Player") && IsServer)
        {
            SpawnSubBosses(topPositionTransform.position, numOfSubBosses);
            SpawnSubBosses(bottomPositionTransform.position, numOfSubBosses);
            playerEnteredSubBossArea = true;
            if (IsServer)
            {
                ActivateBarrier();
            }
            else if (IsClient)
            {
                ActivateBarrierServerRPC();
            }
        }
    }

    private void SpawnSubBosses(Vector3 spawnPos, int numOfSubBosses)
    {

        for (int i = 0; i < numOfSubBosses; i++)
        {
            var subBossInstance = Instantiate(subBossPrefab, spawnPos, Quaternion.identity);
            var subBossNetworkObject = subBossInstance.GetComponent<NetworkObject>();

            if (IsServer)
            {
                subBossNetworkObject.SpawnWithOwnership(OwnerClientId);
            }
            else
            {
                Destroy(subBossInstance.gameObject);
            }
        }
    }

    [ServerRpc]
    public void DeactivateBarrierServerRPC()
    {
        DeactivateBarrier();
    }

    public void DeactivateBarrier()
    {
        fog.Stop();
        barrier.enabled = false;
        DeactivateBarrierClientRPC();
    }

    [ClientRpc]
    public void DeactivateBarrierClientRPC()
    {
        fog.Stop();
        barrier.enabled = false;
    }

    // ----------------------------------------- Ativar Barreira

    [ServerRpc]
    public void ActivateBarrierServerRPC()
    {
        ActivateBarrier();
    }

    public void ActivateBarrier()
    {
        fog.gameObject.SetActive(true);
        fog.Play();
        ActivateBarrierClientRPC();
    }

    [ClientRpc]
    public void ActivateBarrierClientRPC()
    {
        fog.gameObject.SetActive(true);
        fog.Play();
    }
}
