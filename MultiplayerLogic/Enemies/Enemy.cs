using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

//Classe que mantém todas as características e estatísticas comuns a todos os inimigos desenvolvidos (Movimento, HP, velocidade, ...)
//Classe que desempenha uma função semelhante a BaseCharacter, mas no contexto dos inimigos.
public class Enemy : NetworkBehaviour
{
    [SerializeField] private float health = 20f;

    public BaseCharacter playerToFollow; //jogador a seguir
    private Vector3 playerPosition; //posição do jogador a seguir
    public NavMeshAgent agent; //referência ao componente NavMeshAgent contido no inimigo

    public Animator myAnimator;
    private float distanceToPlayer; //distância ao jogador

    public GameObject gameManagerGO;
    public GameManager gameManager;

    private bool navMeshReady = false;
    public bool isAlive = true;

    public bool isProvoked = false;

    //tipos de inimigos, assim como o tipo de inimigo ao qual este script se encontra associado
    public enum ENEMY_TYPE { Skeleton, Ghoul, SubBoss, Boss };
    public ENEMY_TYPE enemyType;

    private Skeleton skeleton;
    private Ghoul ghoul;
    private SubBoss subBoss;
    private Boss boss;

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

    //definir o alvo do agente
    public void SetTarget(Transform target)
    {
        if (agent != null && target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    //definir o jogador que o inimigo irá perseguir
    public void SetPlayerToFollow(BaseCharacter player)
    {
        this.playerToFollow = player;
    }

    //indicar se o inimigo está a ser provocado por um Guerreiro ou não
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
                skeleton = GetComponent<Skeleton>();
                break;
            case ENEMY_TYPE.Ghoul:
                ghoul = GetComponent<Ghoul>();
                break;
            case ENEMY_TYPE.SubBoss:
                subBoss = GetComponent<SubBoss>();
                break;
            case ENEMY_TYPE.Boss:
                boss = GetComponent<Boss>();
                break;
        }

        if (gameManagerGO != null)
        {
            GameManager gameManager = gameManagerGO.GetComponent<GameManager>();

            if (gameManager != null)
            {
                //adicionar o inimigo à respetiva lista de inimigos mantida pelo GameManager, dependendo do tipo
                if (skeleton != null) gameManager.skeletonList.Add(this.skeleton);
                else if (ghoul != null) gameManager.ghoulList.Add(this.ghoul);
                else if (subBoss != null) gameManager.subBossList.Add(this.subBoss);
                else if (boss != null) gameManager.bossList.Add(this.boss);
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

            if(isProvoked == false) playerToFollow = FindClosestPlayer();
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

    //Encontrar o jogador mais próximo, e persegui-lo
    private BaseCharacter FindClosestPlayer()
    {
        BaseCharacter closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var character in gameManager.allCharactersList)
        {
            if (character != null)
            {
                BaseCharacter baseCharacter = character.GetComponent<BaseCharacter>();
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

    [ServerRpc]
    public void MoveAgentServerRPC()
    {
        MoveAgent();
    }

}