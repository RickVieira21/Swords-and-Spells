using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class BossSingleplayer : MonoBehaviour
{
    [SerializeField] private float attackBias;
    private GameObject bossHead;
    private GameObject bossHand;

    private EnemySingleplayer enemy;

    private float defaultAgentSpeed = 5f;
    private float headbuttAgentSpeed = 11.0f;

    private float attackTimer = 0.0f;
    private float maxAttackDelay = 5f;

    [SerializeField] private bool doingAttack1 = false;
    [SerializeField] private bool doingAttack2 = false;

    public AudioSource gameEndedAudio;
    private MusicZone musicZone;

    public EnemySingleplayer GetEnemy()
    {
        return this.enemy;
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
    }

    private void Awake()
    {
        enemy = GetComponent<EnemySingleplayer>();
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
                    Attack1();
                }
                //Começar o Ataque 2
                else if (atkProbability >= attackBias)
                {
                    Attack2();
                }
            }

            if (doingAttack1 && !doingAttack2 && enemy.myAnimator.GetFloat("DistanceToPlayer") < 4f)
            {
                Attack1SetTrigger();
            }

            if (doingAttack2 && !doingAttack1 && enemy.myAnimator.GetFloat("DistanceToPlayer") < 3.5f)
            {
                Attack2SetTrigger();
            }

            if (enemy.myAnimator.GetFloat("Health") <= 0f)
            {
                Die();
            }
        }
    }

    //*************************************************************************
    //Morte
    public void Die()
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

        BaseCharacterSingleplayer[] characters = FindObjectsOfType<BaseCharacterSingleplayer>();
        foreach (BaseCharacterSingleplayer character in characters)
        {
            character.ShowGameEndedMenu();
        }
        if (gameEndedAudio != null)
        {
            musicZone.StopAllAudioAndPlayNew(gameEndedAudio);
        }
    }

    //*********************************************************************
    //Ataque 1 (Head)

    public void Attack1()
    {
        enemy.myAnimator.ResetTrigger("ATK1Trigger");
        enemy.myAnimator.SetTrigger("Attack1");
        enemy.agent.speed = headbuttAgentSpeed;
        doingAttack1 = true;
    }

    public void Attack1SetTrigger()
    {
        enemy.myAnimator.SetTrigger("ATK1Trigger");
    }

    public void Attack1ActivateTrigger()
    {
        bossHead.GetComponent<BoxCollider>().enabled = true;
    }

    public void Attack1DeactivateTrigger()
    {
        bossHead.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0f;
        doingAttack1 = false;
        enemy.myAnimator.ResetTrigger("ATK1Trigger");
        enemy.agent.speed = defaultAgentSpeed;
    }

    public void StopMovement()
    {
        enemy.agent.speed = 0f;
    }

    public void ResumeMovement()
    {
        enemy.agent.speed = headbuttAgentSpeed;
    }

    //*********************************************************************
    //Ataque 2 (Melee)

    public void Attack2()
    {
        enemy.myAnimator.SetTrigger("Attack2");
        //agent.speed = headbuttAgentSpeed;
        doingAttack2 = true;
        //agent.isStopped = true;
    }

    public void Attack2SetTrigger()
    {
        enemy.myAnimator.SetTrigger("ATK2Trigger");
    }

    public void Attack2ActivateTrigger()
    {
        enemy.myAnimator.SetFloat("DistanceToPlayer", enemy.GetDistanceToPlayer());
        bossHand.GetComponent<BoxCollider>().enabled = true;
    }

    public void Attack2DeactivateTrigger()
    {
        bossHand.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0f;
        doingAttack2 = false;
    }
}
