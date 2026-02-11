using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemySingleplayer : MonoBehaviour
{
    [SerializeField] private float health = 20f;

    public BaseCharacterSingleplayer playerToFollow;
    private Vector3 playerPosition;
    public NavMeshAgent agent;

    public Animator myAnimator;
    private float distanceToPlayer;

    public GameObject gameManagerGO;
    public GameManager gameManager;

    private bool navMeshReady = false;
    public bool isAlive = true;

    public bool isProvoked = false;

    public enum ENEMY_TYPE { Skeleton, Ghoul, SubBoss, Boss };
    public ENEMY_TYPE enemyType;

    private SkeletonSingleplayer skeleton;
    private GhoulSingleplayer ghoul;
    private SubBossSingleplayer subBoss;
    private BossSingleplayer boss;

    public float GetDistanceToPlayer()
    {
        return distanceToPlayer;
    }

    public Vector3 GetPlayerPosition()
    {
        return playerPosition;
    }

    public Animator GetAnimator()
    {
        return myAnimator;
    }

    public void SetMovementSpeed(float newValue)
    {
        this.agent.speed = newValue;
    }

    public void SetTarget(Transform target)
    {
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    public void SetPlayerToFollow(BaseCharacterSingleplayer player)
    {
        this.playerToFollow = player;
    }

    public void SetProvokedStatus(bool value)
    {
        this.isProvoked = value;
    }

    public float GetHealth()
    {
        return this.health;
    }

    public void SetHealth(float newHealth)
    {
        this.health = newHealth;
    }


    private void Awake()
    {
        myAnimator = GetComponent<Animator>();

        GameObject gameManagerGO = GameObject.Find("GameManager");

        switch (enemyType)
        {
            case ENEMY_TYPE.Skeleton:
                skeleton = GetComponent<SkeletonSingleplayer>();
                break;
            case ENEMY_TYPE.Ghoul:
                ghoul = GetComponent<GhoulSingleplayer>();
                break;
            case ENEMY_TYPE.SubBoss:
                subBoss = GetComponent<SubBossSingleplayer>();
                break;
            case ENEMY_TYPE.Boss:
                boss = GetComponent<BossSingleplayer>();
                break;
        }

        if (gameManagerGO != null)
        {
            GameManager gameManager = gameManagerGO.GetComponent<GameManager>();

            if (gameManager != null)
            {
                if (skeleton != null) gameManager.sList.Add(this.skeleton);
                else if (ghoul != null) gameManager.gList.Add(this.ghoul);
                else if (subBoss != null) gameManager.sbList.Add(this.subBoss);
                else if (boss != null) gameManager.bList.Add(this.boss);
            }
            else
            {
                Debug.LogWarning("GameManager not found");
            }
        }
        else
        {
            Debug.LogWarning("GameManager GameObject not found");
        }
        myAnimator.SetBool("IsAlive", true);
        SetHealth(health);
        myAnimator.SetFloat("Health", health);
    }

    private void Start()
    {
    }

    //tratar dos inputs realizados por cada jogador constantemente
    private void Update()
    {
        if (isAlive)
        {
            if (!navMeshReady) // Verifica se o NavMesh ainda não está pronto
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    navMeshReady = true; // Define a flag para verdadeira quando o NavMesh estiver pronto
                }
                else
                {
                    return; // Se o NavMesh ainda não estiver pronto, retorna e tenta novamente no próximo quadro
                }
            }

            if (isProvoked == false) playerToFollow = FindClosestPlayer();

            if (playerToFollow == null) return; // Se não encontrar um jogador, retorna

            playerPosition = playerToFollow.transform.position;
            distanceToPlayer = Vector3.Distance(playerPosition, this.transform.position);
            myAnimator.SetFloat("DistanceToPlayer", distanceToPlayer);
            if (agent.isActiveAndEnabled)
            {
                MoveAgent();
            }
        }
    }

    private BaseCharacterSingleplayer FindClosestPlayer()
    {
        BaseCharacterSingleplayer closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var character in gameManager.allSingleplayerCharactersList)
        {
            if (character != null)
            {
                BaseCharacterSingleplayer baseCharacter = character.GetComponent<BaseCharacterSingleplayer>();
                if (baseCharacter.isAlive)
                {
                    float distance = Vector3.Distance(this.transform.position, baseCharacter.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = baseCharacter;
                    }
                }
            }
        }

        return closestPlayer;
    }

    public void MoveAgent()
    {
        agent.SetDestination(playerPosition);
    }

}