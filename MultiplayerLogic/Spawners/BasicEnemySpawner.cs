using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Script associado ao Spawner responsável por spawnar os inimigos da primeira fase, os Esqueletos.
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class BasicEnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject skeletonPrefab;
    [SerializeField] private Transform topPositionTransform;
    [SerializeField] private Transform bottomPositionTransform;
    [SerializeField] private int numOfSkeletons;
    public static bool playerEnteredArea1 = false;

    [SerializeField] private ParticleSystem fog;
    [SerializeField] private BoxCollider barrier;


    private void Update()
    {
        if (playerEnteredArea1)
        {
            Debug.Log("Area1 Skeleton List Size: " + GameObject.FindGameObjectsWithTag("Enemy").Length);
            if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
            {
                //deactivate the barrier if all Area1 skeletons are dead
                if (IsServer)
                {
                    DeactivateBarrier();
                } else if (IsClient)
                {
                    DeactivateBarrierServerRPC();
                }
            }
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredArea1 && other.CompareTag("Player") && IsServer)
        {
            SpawnSkeletonGroup(topPositionTransform.position, numOfSkeletons);
            SpawnSkeletonGroup(bottomPositionTransform.position, numOfSkeletons);
            playerEnteredArea1 = true;
            if (IsServer)
            {
                ActivateBarrier();
            } else if (IsClient)
            {
                ActivateBarrierServerRPC();
            }
        }
    }

    private void SpawnSkeletonGroup(Vector3 spawnPos, int numOfSkeletons)
    {

        for (int i = 0; i < numOfSkeletons; i++)
        {
            var skeletonInstance = Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
            var skeletonNetworkObject = skeletonInstance.GetComponent<NetworkObject>();

            // Verifica se este é o Servidor/Host. Se sim, o Objeto será spawnado
            if (IsServer)
            {
                skeletonNetworkObject.SpawnWithOwnership(OwnerClientId);
            }
            else
            {
                // Se for Cliente, destrói o GameObject não autorizado (Os Clientes não podem spawnar GameObjects, apenas o Servidor/Host pode)
                Destroy(skeletonInstance.gameObject);
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
