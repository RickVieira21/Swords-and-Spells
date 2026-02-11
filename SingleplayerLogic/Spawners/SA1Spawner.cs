using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


//Script que representa o Spawner que instancia Esqueletos, no Modo Singleplayer
/*
 * Todos os scripts que representam spawners apresentam uma estrutura de código semelhante,
 * visto que o único componente diferente entre todos estes scripts consiste no nome do método utilizado
 * para, de facto, spawnar o inimigo (SpawnSkeletonGroup/SpawnGhoulGroup/SpawnSubBosses/SpawnBoss), assim
 * como o argumento introduzido nesse mesmo método, sendo este o Prefab que representa o inimigo correspondente
 * (Skeleton/Ghoul/Sub-Boss/Boss).
 */
public class SA1Spawner : MonoBehaviour
{
    [SerializeField] private GameObject skeletonPrefab;
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
            Debug.Log("Area1 Skeleton List Size: " + GameObject.FindGameObjectsWithTag("SEnemy").Length);
            if (GameObject.FindGameObjectsWithTag("SEnemy").Length <= 0)
            {
                    DeactivateBarrier();
            }
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!playerEnteredArea1 && other.CompareTag("SinglePlayer"))
        {
            SpawnSkeletonGroup(topPositionTransform.position, numOfSkeletons);
            SpawnSkeletonGroup(bottomPositionTransform.position, numOfSkeletons);
            playerEnteredArea1 = true;
            ActivateBarrier();
        }
    }

    private void SpawnSkeletonGroup(Vector3 spawnPos, int numOfSkeletons)
    {

        for (int i = 0; i < numOfSkeletons; i++)
        {
            var skeletonInstance = Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
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
