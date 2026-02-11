using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class GhoulSingleplayer : MonoBehaviour
{
    private GameObject hand;
    private EnemySingleplayer enemy;

    //public AudioSource audioSpell1;

    public EnemySingleplayer GetEnemy()
    {
        return this.enemy;
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
    }

    void Awake()
    {
        enemy = GetComponent<EnemySingleplayer>();

        enemy.agent = GetComponent<NavMeshAgent>();

        enemy.myAnimator = GetComponent<Animator>();
        enemy.myAnimator.SetFloat("Health", enemy.GetHealth());
        enemy.myAnimator.SetBool("IsAlive", true);

        hand = GetComponentInChildren<GhoulHand>().gameObject;

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

            if (enemy.myAnimator.GetFloat("DistanceToPlayer") <= 1.5f)
            {

                    GhoulAttack();
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
        hand.GetComponent<BoxCollider>().enabled = false;
        Destroy(this.gameObject, 4.0f);
    }


    public void GhoulAttack()
    {
        enemy.myAnimator.SetTrigger("Attack");
    }


    public void GhoulAttackActivateTrigger()
    {
        //PlayAttackAudioClientRPC();
        hand.GetComponent<BoxCollider>().enabled = true;
    }


    public void GhoulAttackDeactivateTrigger()
    {
        hand.GetComponent<BoxCollider>().enabled = false;
    }
}
