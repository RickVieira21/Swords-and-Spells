using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Script associado ao Spawner responsável por spawnar os inimigos da segunda área, os Ghouls
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class Area2Spawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject ghoulPrefab;
    [SerializeField] private Transform topPositionTransform;
    [SerializeField] private Transform bottomPositionTransform;
    [SerializeField] private int numOfGhouls;
    public static bool playerEnteredArea2 = false;

    [SerializeField] private ParticleSystem fog;
    [SerializeField] private BoxCollider barrier;


    private void Update()
    {
        if (playerEnteredArea2)
        {
            Debug.Log("Area2 Enemies List Size: " + GameObject.FindGameObjectsWithTag("Enemy").Length);
            if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
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
        if (!playerEnteredArea2 && other.CompareTag("Player") && IsServer)
        {
            SpawnGhoulGroup(topPositionTransform.position, numOfGhouls);
            SpawnGhoulGroup(bottomPositionTransform.position, numOfGhouls);
            playerEnteredArea2 = true;
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

    private void SpawnGhoulGroup(Vector3 spawnPos, int numOfSkeletons)
    {

        for (int i = 0; i < numOfSkeletons; i++)
        {
            var ghoulInstance = Instantiate(ghoulPrefab, spawnPos, Quaternion.identity);
            var ghoulNetworkObject = ghoulInstance.GetComponent<NetworkObject>();

            if (IsServer)
            {
                ghoulNetworkObject.SpawnWithOwnership(OwnerClientId);
            }
            else
            {
                Destroy(ghoulInstance.gameObject);
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
