using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

//Classe que representa o Boss, o último inimigo que o jogador enfrenta, situado na quinta e última área do jogo
public class Boss : NetworkBehaviour
{
    [SerializeField] private float attackBias; //frequência com que o Boss utiliza o primeiro e segundo ataque
    private GameObject bossHead;
    private GameObject bossHand;

    private Enemy enemy;

    private float defaultAgentSpeed = 5f; //Velocidade padrão do Boss
    private float headbuttAgentSpeed = 11.0f; //Velocidade do Boss, durante o Ataque "Headbutt"

    private float attackTimer = 0.0f;
    private float maxAttackDelay = 5f; //Intervalo de tempo entre ataques

    [SerializeField] private bool doingAttack1 = false;
    [SerializeField] private bool doingAttack2 = false;

    public AudioSource gameEndedAudio;
    private MusicZone musicZone;

    public Enemy GetEnemy()
    {
        return this.enemy;
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
    }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemy.enabled = true;

        enemy.agent = GetComponent<NavMeshAgent>();

        enemy.myAnimator = GetComponent<Animator>();
        enemy.myAnimator.SetFloat("Health", enemy.GetHealth());
        enemy.myAnimator.SetBool("IsAlive", true);

        bossHand = GetComponentInChildren<BossHand>().gameObject;
        bossHead = GetComponentInChildren<BossHead>().gameObject;

        enemy.gameManagerGO = GameObject.Find("GameManager");

    }

    // Start is called before the first frame update
    void Start()
    {
        if (enemy.gameManagerGO != null)
        {
            enemy.gameManager = enemy.gameManagerGO.GetComponent<GameManager>();
        }

        musicZone = FindObjectOfType<MusicZone>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.isAlive)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= maxAttackDelay && !doingAttack1 && !doingAttack2)
            {
                float atkProbability = Random.value;
                //Começar o Ataque 1
                if (atkProbability < attackBias)
                {
                    if (IsServer)
                    {
                        Attack1();
                    }
                    else if (IsClient)
                    {
                        Attack1ServerRPC();
                    }
                } 
                //Começar o Ataque 2
                else if (atkProbability >= attackBias)
                {
                    if (IsServer)
                    {
                        Attack2();
                    }
                    else if (IsClient)
                    {
                        Attack2ServerRPC();
                    }
                }
            }

            if (doingAttack1 && !doingAttack2 && enemy.myAnimator.GetFloat("DistanceToPlayer") < 4f)
            {
                if (IsServer)
                {
                    Attack1SetTrigger();
                }
                else if (IsClient)
                {
                    Attack1SetTriggerServerRPC();
                }
            }

            if (doingAttack2 && !doingAttack1 && enemy.myAnimator.GetFloat("DistanceToPlayer") < 3.5f)
            {
                if (IsServer)
                {
                    Attack2SetTrigger();
                }
                else if (IsClient)
                {
                    Attack2SetTriggerServerRPC();
                }
            }

            if (enemy.myAnimator.GetFloat("Health") <= 0f)
            {
                if (IsServer)
                {
                    Die();
                }
                else if (IsClient)
                {
                    DieServerRPC();
                }
            }
        }
    }

    //*************************************************************************
    //Morte
    public void Die()
    {
        //if (!isAlive) return;
        DieClientRPC();
        enemy.isAlive = false;
        enemy.myAnimator.SetBool("IsAlive", false);
        var skeletonNetworkObject = this.GetComponent<NetworkObject>();
        if (enemy.agent.isActiveAndEnabled)
        {
            enemy.agent.isStopped = true;
            enemy.agent.enabled = false;
        }
        bossHand.GetComponent<BoxCollider>().enabled = false;
        bossHead.GetComponent<BoxCollider>().enabled = false;
        //Destroy(skeletonNetworkObject.gameObject, 4.0f);

        BaseCharacter[] characters = FindObjectsOfType<BaseCharacter>();
        foreach (BaseCharacter character in characters)
        {
            character.ShowGameEndedMenu();
        }

        if (gameEndedAudio != null)
        {
            musicZone.StopAllAudioAndPlayNew(gameEndedAudio);
        }
    }

    [ServerRpc]
    public void DieServerRPC()
    {
        Die();
        DieClientRPC();
    }

    [ClientRpc]
    public void DieClientRPC()
    {
        enemy.isAlive = false;
        enemy.myAnimator.SetBool("IsAlive", false);
        if (enemy.agent.isActiveAndEnabled)
        {
            enemy.agent.isStopped = true;
            enemy.agent.enabled = false;
        }
        bossHand.GetComponent<BoxCollider>().enabled = false;
        bossHead.GetComponent<BoxCollider>().enabled = false;
        //StartCoroutine(DestroyEnemyInClient());

        BaseCharacter[] characters = FindObjectsOfType<BaseCharacter>();
        foreach (BaseCharacter character in characters)
        {
            character.ShowGameEndedMenu();
        }

        if (gameEndedAudio != null)
        {
            musicZone.StopAllAudioAndPlayNew(gameEndedAudio);
        }
    }

    private IEnumerator DestroyEnemyInClient()
    {
        yield return new WaitForSeconds(4);
        var skeletonNetworkObject = this.GetComponent<NetworkObject>();
        skeletonNetworkObject.Despawn(true);
    }

    //*********************************************************************
    //Ataque 1 (Head)

    public void Attack1()
    {
        Attack1ClientRPC();
        enemy.myAnimator.ResetTrigger("ATK1Trigger");
        enemy.myAnimator.SetTrigger("Attack1");
        enemy.agent.speed = headbuttAgentSpeed;
        doingAttack1 = true;
        //agent.isStopped = true;
    }

    [ServerRpc]
    public void Attack1ServerRPC()
    {
        Attack1();
    }

    [ClientRpc]
    public void Attack1ClientRPC()
    {
        enemy.myAnimator.ResetTrigger("ATK1Trigger");
        enemy.myAnimator.SetTrigger("Attack1");
        enemy.agent.speed = headbuttAgentSpeed;
        doingAttack1 = true;
    }

    public void Attack1SetTrigger()
    {
        Attack1SetTriggerClientRPC();
        enemy.myAnimator.SetTrigger("ATK1Trigger");
    }

    [ServerRpc]
    public void Attack1SetTriggerServerRPC()
    {
        Attack1SetTrigger();
    }

    [ClientRpc]
    public void Attack1SetTriggerClientRPC()
    {
        enemy.myAnimator.SetTrigger("ATK1Trigger");
    }

    public void Attack1ActivateTrigger()
    {
        Attack1ActivateTriggerClientRPC();
        bossHead.GetComponent<BoxCollider>().enabled = true;
    }

    [ServerRpc]
    public void Attack1ActivateTriggerServerRPC()
    {
        Attack1ActivateTrigger();
    }

    [ClientRpc]
    public void Attack1ActivateTriggerClientRPC()
    {
        bossHead.GetComponent<BoxCollider>().enabled = true;
    }

    public void Attack1DeactivateTrigger()
    {
        Attack1DectivateTriggerClientRPC();
        bossHead.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0f;
        doingAttack1 = false;
        enemy.myAnimator.ResetTrigger("ATK1Trigger");
        enemy.agent.speed = defaultAgentSpeed;
    }

    [ServerRpc]
    public void Attack1DeactivateTriggerServerRPC()
    {
        Attack1DeactivateTrigger();
    }

    [ClientRpc]
    public void Attack1DectivateTriggerClientRPC()
    {
        bossHand.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0f;
        doingAttack1 = false;
        enemy.myAnimator.ResetTrigger("ATK1Trigger");
        enemy.agent.speed = defaultAgentSpeed;
    }

    //Stop Agent Movement during Rage Animation

    public void StopMovement()
    {
        StopMovementClientRPC();
        enemy.agent.speed = 0f;
    }

    [ServerRpc]
    public void StopMovementServerRPC()
    {
        StopMovement();
    }

    [ClientRpc]
    public void StopMovementClientRPC()
    {
       enemy.agent.speed = 0f;
    }

    public void ResumeMovement()
    {
        ResumeMovementClientRPC();
        enemy.agent.speed = headbuttAgentSpeed;
    }

    [ServerRpc]
    public void ResumeMovementServerRPC()
    {
        ResumeMovement();
    }

    [ClientRpc]
    public void ResumeMovementClientRPC()
    {
        enemy.agent.speed = headbuttAgentSpeed;
    }

    //*********************************************************************
    //Ataque 2 (Melee)

    public void Attack2()
    {
        Attack2ClientRPC();
        enemy.myAnimator.SetTrigger("Attack2");
        //agent.speed = headbuttAgentSpeed;
        doingAttack2 = true;
        //agent.isStopped = true;
    }

    [ServerRpc]
    public void Attack2ServerRPC()
    {
        Attack2();
    }

    [ClientRpc]
    public void Attack2ClientRPC()
    {
        enemy.myAnimator.SetTrigger("Attack2");
        //agent.speed = headbuttAgentSpeed;
        doingAttack2 = true;
    }

    public void Attack2SetTrigger()
    {
        Attack2SetTriggerClientRPC();
        enemy.myAnimator.SetTrigger("ATK2Trigger");
    }

    [ServerRpc]
    public void Attack2SetTriggerServerRPC()
    {
        Attack2SetTrigger();
    }

    [ClientRpc]
    public void Attack2SetTriggerClientRPC()
    {
        enemy.myAnimator.SetTrigger("ATK2Trigger");
    }

    public void Attack2ActivateTrigger()
    {
        Attack2ActivateTriggerClientRPC();
        enemy.myAnimator.SetFloat("DistanceToPlayer", enemy.GetDistanceToPlayer());
        bossHand.GetComponent<BoxCollider>().enabled = true;
    }

    [ServerRpc]
    public void Attack2ActivateTriggerServerRPC()
    {
        Attack2ActivateTrigger();
    }

    [ClientRpc]
    public void Attack2ActivateTriggerClientRPC()
    {
        enemy.myAnimator.SetFloat("DistanceToPlayer", enemy.GetDistanceToPlayer());
        bossHand.GetComponent<BoxCollider>().enabled = true;
    }

    public void Attack2DeactivateTrigger()
    {
        Attack2DectivateTriggerClientRPC();
        bossHand.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0f;
        doingAttack2 = false;
    }

    [ServerRpc]
    public void Attack2DeactivateTriggerServerRPC()
    {
        Attack2DeactivateTrigger();
    }

    [ClientRpc]
    public void Attack2DectivateTriggerClientRPC()
    {
        bossHand.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0f;
        doingAttack2 = false;
    }
}
