using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class SubBossSingleplayer : MonoBehaviour
{
    private EnemySingleplayer enemy;

    private float attackTimer = 0.0f;
    private readonly float maxAttackDelay = 5f;

    public Rigidbody attack1; //Rigidbody do projétil do Ataque 1
    public Transform attack1SpawnPosition; //Posição de Spawn do Ataque 1
    private Rigidbody attack1Instance; //Instância a ser spawnada pelo sub-boss, quando o mesmo decide atacar
    public float attack1Force;

    private SubBossLeftArm leftArm;
    private SubBossRightArm rightArm;

    public EnemySingleplayer GetEnemy()
    {
        return this.enemy;
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
    }

    // Start is called before the first frame update
    void Awake()
    {
        enemy = GetComponent<EnemySingleplayer>();
        enemy.enabled = true;

        enemy.agent = GetComponent<NavMeshAgent>();

        enemy.myAnimator = GetComponent<Animator>();
        enemy.myAnimator.SetFloat("Health", enemy.GetHealth());
        enemy.myAnimator.SetBool("IsAlive", true);

        leftArm = GetComponentInChildren<SubBossLeftArm>();
        rightArm = GetComponentInChildren<SubBossRightArm>();

        enemy.gameManagerGO = GameObject.Find("GameManager");
    }

    void Start()
    {
        if (enemy.gameManagerGO != null)
        {
            enemy.gameManager = enemy.gameManagerGO.GetComponent<GameManager>();
        }
    }

    private void Update()
    {
        if (enemy.isAlive)
        {

            attackTimer += Time.deltaTime;
            if (attackTimer >= maxAttackDelay && enemy.myAnimator.GetFloat("DistanceToPlayer") > 1.3f)
            {
                attackTimer = 0.0f;
                Attack1();
            }
            else if (enemy.myAnimator.GetFloat("DistanceToPlayer") <= 1.3f)
            {
                 Attack2();
            }

            if (enemy.myAnimator.GetFloat("Health") <= 0f)
            {
                    Die();
            }
        }
    }

    public void Die()
    {
        enemy.isAlive = false;
        enemy.myAnimator.SetBool("IsAlive", false);
        if (enemy.agent.isActiveAndEnabled)
        {
            enemy.agent.isStopped = true;
            enemy.agent.enabled = false;
        }
        Destroy(this.gameObject, 8.0f);
    }

    //********************** ATTACK 1

    public void Attack1()
    {
        enemy.myAnimator.SetTrigger("Attack1");
    }

    public void UseAttack1()
    {
        Debug.Log("SubBoss is Casting Attack 1");
        attack1Instance = Instantiate(attack1, attack1SpawnPosition.position, attack1SpawnPosition.rotation);
        attack1Instance.AddForce(attack1SpawnPosition.forward * attack1Force);
        attack1Instance.gameObject.AddComponent<SBAttack1>();
        attackTimer = 0f;
        Destroy(attack1Instance.gameObject, 6.0f);
    }


    //********************* ATTACK 2

    public void Attack2()
    {
        enemy.myAnimator.SetTrigger("Attack2");
    }

    public void SubBossATK2On()
    {
        leftArm.GetComponent<BoxCollider>().enabled = true;
        rightArm.GetComponent<BoxCollider>().enabled = true;
    }


    public void SubBossATK2Off()
    {
        leftArm.GetComponent<BoxCollider>().enabled = false;
        rightArm.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0.0f;
    }
}
