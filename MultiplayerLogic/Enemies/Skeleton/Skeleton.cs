using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

//Classe que representa o Esqueleto, o inimigo presente na primeira área do jogo
public class Skeleton : NetworkBehaviour
{
    private GameObject weapon;
    private Enemy enemy;

    public AudioSource audioSpell1;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemy.enabled = true;

        enemy.agent = GetComponent<NavMeshAgent>();

        enemy.myAnimator = GetComponent<Animator>();
        enemy.myAnimator.SetFloat("Health", enemy.GetHealth());
        enemy.myAnimator.SetBool("IsAlive", true);

        weapon = GetComponentInChildren<SkeletonWeapon>().gameObject;

        enemy.gameManagerGO = GameObject.Find("GameManager");
    }

    public Enemy GetEnemy()
    {
        return this.enemy;
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
            //atacar o jogador se o mesmo estiver próximo o suficiente
            if (enemy.myAnimator.GetFloat("DistanceToPlayer") <= 1.5f)
            {
                if (IsServer)
                {
                    Attack();
                }
                else if (IsClient)
                {
                    AttackServerRPC();
                }
            }
            //morrer, se a sua vida for equivalente ou menor que 0
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

    //Executar a animação de morte, e desativar o Box Collider associado à arma, para que o jogador não sofra danos depois do Esqueleto morrer
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
        weapon.GetComponent<BoxCollider>().enabled = false;
        Destroy(skeletonNetworkObject.gameObject, 4.0f);
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
        weapon.GetComponent<BoxCollider>().enabled = false;
        StartCoroutine(DestroyEnemyInClient());
    }

    [ServerRpc]
    public void DieServerRPC()
    {
        Die();
        DieClientRPC();
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
    }

    [ServerRpc]
    public void MoveAgentServerRPC()
    {
        MoveAgent();
    }

    public void Attack()
    {
        AttackClientRPC();
        enemy.myAnimator.SetTrigger("Attack");
    }

    [ServerRpc]
    public void AttackServerRPC()
    {
        Attack();
    }

    public void AttackActivateTrigger()
    {
        PlayAttackAudioClientRPC(); //Som do ataque do Esqueleto
        AttackActivateTriggerClientRPC();
        weapon.GetComponent<BoxCollider>().enabled = true;
    }

    [ServerRpc]
    public void AttackActivateTriggerServerRPC()
    {
        AttackActivateTrigger();
    }

    public void AttackDeactivateTrigger()
    {
        AttackDectivateTriggerClientRPC();
        weapon.GetComponent<BoxCollider>().enabled = false;
    }

    [ServerRpc]
    public void AttackDeactivateTriggerServerRPC()
    {
        AttackDeactivateTrigger();
    }

    [ClientRpc]
    public void AttackActivateTriggerClientRPC()
    {
        weapon.GetComponent<BoxCollider>().enabled = true;
    }

    [ClientRpc]
    public void AttackDectivateTriggerClientRPC()
    {
        weapon.GetComponent<BoxCollider>().enabled = false;
    }

    private IEnumerator DestroyEnemyInClient()
    {
        yield return new WaitForSeconds(4);
        var skeletonNetworkObject = this.GetComponent<NetworkObject>();
        skeletonNetworkObject.Despawn(true);
    }

    [ClientRpc]
    public void AttackClientRPC()
    {
        enemy.myAnimator.SetTrigger("Attack");
    }

    [ClientRpc]
    public void PlayAttackAudioClientRPC()
    {
        audioSpell1.Play();
    }
}
