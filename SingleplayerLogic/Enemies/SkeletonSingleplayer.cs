using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonSingleplayer : MonoBehaviour
{
    private GameObject weapon;
    private EnemySingleplayer enemy;

    public AudioSource audioSpell1;

    void Awake()
    {
        enemy = GetComponent<EnemySingleplayer>();
        enemy.enabled = true;

        enemy.agent = GetComponent<NavMeshAgent>();

        enemy.myAnimator = GetComponent<Animator>();
        enemy.myAnimator.SetFloat("Health", enemy.GetHealth());
        enemy.myAnimator.SetBool("IsAlive", true);

        weapon = GetComponentInChildren<SkeletonWeapon>().gameObject;

        enemy.gameManagerGO = GameObject.Find("GameManager");
    }

    public EnemySingleplayer GetEnemy()
    {
        return this.enemy;
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
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
                    Attack();
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
        weapon.GetComponent<BoxCollider>().enabled = false;
        Destroy(this.gameObject, 4.0f);
    }

    public void Attack()
    {
        enemy.myAnimator.SetTrigger("Attack");
    }


    public void AttackActivateTrigger()
    {
        audioSpell1.Play();
        weapon.GetComponent<BoxCollider>().enabled = true;
    }

    public void AttackDeactivateTrigger()
    {
        weapon.GetComponent<BoxCollider>().enabled = false;
    }
}
