using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

//Classe que representa o Esqueleto, o inimigo presente na segunda área do jogo
public class Ghoul : NetworkBehaviour
{
    private GameObject hand; //braço do Ghoul, responsável por causar dano aos jogadores
    private Enemy enemy;

    void Awake()
    {
        enemy = GetComponent<Enemy>();

        enemy.agent = GetComponent<NavMeshAgent>();

        enemy.myAnimator = GetComponent<Animator>();
        enemy.myAnimator.SetFloat("Health", enemy.GetHealth());
        enemy.myAnimator.SetBool("IsAlive", true);

        hand = GetComponentInChildren<GhoulHand>().gameObject;

        enemy.gameManagerGO = GameObject.Find("GameManager");
    }

    public Enemy GetEnemy()
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

            if (enemy.myAnimator.GetFloat("DistanceToPlayer") <= 2.0f)
            {
                if (IsServer)
                {
                    GhoulAttack();
                }
                else if (IsClient)
                {
                    GhoulAttackServerRPC();
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

    public void Die()
    {
        DieClientRPC();
        enemy.isAlive = false;
        enemy.myAnimator.SetBool("IsAlive", false);
        var skeletonNetworkObject = this.GetComponent<NetworkObject>();
        if (enemy.agent.isActiveAndEnabled)
        {
            enemy.agent.isStopped = true;
            enemy.agent.enabled = false;
        }
        hand.GetComponent<BoxCollider>().enabled = false;
        Destroy(skeletonNetworkObject.gameObject, 4.0f);
    }

    [ServerRpc]
    public void DieServerRPC()
    {
        Die();
        DieClientRPC();
    }

    public void GhoulAttack()
    {
        GhoulAttackClientRPC();
        enemy.myAnimator.SetTrigger("Attack");
    }

    [ServerRpc]
    public void GhoulAttackServerRPC()
    {
        GhoulAttack();
    }

    public void GhoulAttackActivateTrigger()
    {
        GhoulAttackActivateTriggerClientRPC();
        hand.GetComponent<BoxCollider>().enabled = true;
    }

    [ServerRpc]
    public void GhoulAttackActivateTriggerServerRPC()
    {
        GhoulAttackActivateTrigger();
    }

    public void GhoulAttackDeactivateTrigger()
    {
        GhoulAttackDectivateTriggerClientRPC();
        hand.GetComponent<BoxCollider>().enabled = false;
    }

    [ServerRpc]
    public void GhoulAttackDeactivateTriggerServerRPC()
    {
        GhoulAttackDeactivateTrigger();
    }

    [ClientRpc]
    public void GhoulAttackActivateTriggerClientRPC()
    {
        hand.GetComponent<BoxCollider>().enabled = true;
    }

    [ClientRpc]
    public void GhoulAttackDectivateTriggerClientRPC()
    {
        hand.GetComponent<BoxCollider>().enabled = false;
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
        hand.GetComponent<BoxCollider>().enabled = false;
        StartCoroutine(DestroyEnemyInClient());
    }

    private IEnumerator DestroyEnemyInClient()
    {
        yield return new WaitForSeconds(4);
        var skeletonNetworkObject = this.GetComponent<NetworkObject>();
        skeletonNetworkObject.Despawn(true);
    }

    [ClientRpc]
    public void GhoulAttackClientRPC()
    {
        enemy.myAnimator.SetTrigger("Attack");
    }
}