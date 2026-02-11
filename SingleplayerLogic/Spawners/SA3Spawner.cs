using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Script que representa o Spawner que instancia Sub-Bosses, no Modo Singleplayer
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class SA3Spawner : MonoBehaviour
{
    [SerializeField] private GameObject subBossPrefab;
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
            Debug.Log("SubBoss List Size: " + GameObject.FindGameObjectsWithTag("SEnemy").Length);
            if (GameObject.FindGameObjectsWithTag("SEnemy").Length <= 0)
            {
                DeactivateBarrier();
            }
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredSubBossArea && other.CompareTag("SinglePlayer"))
        {
            SpawnSubBosses(topPositionTransform.position, numOfSubBosses);
            SpawnSubBosses(bottomPositionTransform.position, numOfSubBosses);
            playerEnteredSubBossArea = true;
            ActivateBarrier();
        }
    }

    private void SpawnSubBosses(Vector3 spawnPos, int numOfSubBosses)
    {
        for (int i = 0; i < numOfSubBosses; i++)
        {
            Instantiate(subBossPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void DeactivateBarrier()
    {
        fog.Stop();
        barrier.enabled = false;
    }

    public void ActivateBarrier()
    {
        fog.gameObject.SetActive(true);
        fog.Play();
    }

}