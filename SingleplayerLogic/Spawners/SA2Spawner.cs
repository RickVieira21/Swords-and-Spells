using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Script que representa o Spawner que instancia Ghouls, no Modo Singleplayer
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class SA2Spawner : MonoBehaviour
{
    [SerializeField] private GameObject ghoulPrefab;
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
            Debug.Log("Area2 Enemies List Size: " + GameObject.FindGameObjectsWithTag("SEnemy").Length);
            if (GameObject.FindGameObjectsWithTag("SEnemy").Length <= 0)
            {
                    DeactivateBarrier();
            }
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredArea2 && other.CompareTag("SinglePlayer"))
        {
            SpawnGhoulGroup(topPositionTransform.position, numOfGhouls);
            SpawnGhoulGroup(bottomPositionTransform.position, numOfGhouls);
            playerEnteredArea2 = true;
            ActivateBarrier();
        }
    }

    private void SpawnGhoulGroup(Vector3 spawnPos, int numGhouls)
    {

        for (int i = 0; i < numGhouls; i++)
        {
            Instantiate(ghoulPrefab, spawnPos, Quaternion.identity);
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
