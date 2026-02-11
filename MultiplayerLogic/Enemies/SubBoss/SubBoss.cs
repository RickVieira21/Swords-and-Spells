using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

//Classe que representa o Sub-Boss, um inimigo que os jogadores enfrentam na terceira área do jogo
public class SubBoss : NetworkBehaviour
{
    private Enemy enemy;

    private float attackTimer = 0.0f;
    private readonly float maxAttackDelay = 5f; //Intervalo de tempo entre ataques (excluindo o ataque à curta distância)

    //Ataque 1 - Bola de Fogo; Longa Distância
    public Rigidbody attack1; //Rigidbody do projétil do Ataque 1
    public Transform attack1SpawnPosition; //Posição de Spawn do Ataque 1
    private Rigidbody attack1Instance; //Instância a ser spawnada pelo sub-boss, quando o mesmo decide atacar
    public float attack1Force; //Força aplicada no projétil criado pelo Sub-Boss

    private SubBossLeftArm leftArm; //braço esquerdo
    private SubBossRightArm rightArm; //braço direito

    public Enemy GetEnemy()
    {
        return this.enemy;
    }

    public void MoveAgent()
    {
        enemy.agent.SetDestination(enemy.GetPlayerPosition());
    }

    void Awake()
    {
        enemy = GetComponent<Enemy>();
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
            //Atacar o jogador à longa distância, com uma Bola de Fogo, se o mesmo estiver longe o suficiente do jogador alvo
            if (attackTimer >= maxAttackDelay && enemy.myAnimator.GetFloat("DistanceToPlayer") > 1.3f)
            {
                attackTimer = 0.0f;
                if (IsServer)
                {
                    Attack1();
                }
                else if (IsClient)
                {
                    Attack1ServerRPC();
                }
            }
            //Atcar o jogador à curta distância, se o mesmo estiver próximo o suficiente do seu jogador alvo
            else if (enemy.myAnimator.GetFloat("DistanceToPlayer") <= 1.3f)
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
        var subBossNetworkObject = this.GetComponent<NetworkObject>();
        if (enemy.agent.isActiveAndEnabled)
        {
            enemy.agent.isStopped = true;
            enemy.agent.enabled = false;
        }
        Destroy(subBossNetworkObject.gameObject, 8.0f);
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
        StartCoroutine(DestroyEnemyInClient());
    }

    private IEnumerator DestroyEnemyInClient()
    {
        yield return new WaitForSeconds(4);
        var subBossNetworkObject = this.GetComponent<NetworkObject>();
        subBossNetworkObject.Despawn(true);
    }

    //********************** Ataque 1 - Conjurar uma Bola de Fogo; Ataque de Longa Distância

    public void Attack1()
    {
        Attack1ClientRPC();
        enemy.myAnimator.SetTrigger("Attack1");
    }

    [ServerRpc]
    public void Attack1ServerRPC()
    {
        Attack1();
    }

    [ClientRpc]
    public void Attack1ClientRPC()
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



    //********************* Ataque 2 - Melee; Curta Distância; o Sub-Boss ataca o jogador com ambos os seus braços

    public void Attack2()
    {
        Attack2ClientRPC();
        enemy.myAnimator.SetTrigger("Attack2");
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
    }


    public void SubBossATK2On()
    {
        SubBossATK2OnClientRPC();
        leftArm.GetComponent<BoxCollider>().enabled = true;
        rightArm.GetComponent<BoxCollider>().enabled = true;
    }

    [ServerRpc]
    public void SubBossATK2OnServerRPC()
    {
        SubBossATK2On();
    }

    public void SubBossATK2Off()
    {
        SubBossATK2OffClientRPC();
        leftArm.GetComponent<BoxCollider>().enabled = false;
        rightArm.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0.0f;
    }

    [ServerRpc]
    public void SubBossATK2OffServerRPC()
    {
        SubBossATK2Off();
    }

    [ClientRpc]
    public void SubBossATK2OnClientRPC()
    {
        leftArm.GetComponent<BoxCollider>().enabled = true;
        rightArm.GetComponent<BoxCollider>().enabled = true;
    }

    [ClientRpc]
    public void SubBossATK2OffClientRPC()
    {
        leftArm.GetComponent<BoxCollider>().enabled = false;
        rightArm.GetComponent<BoxCollider>().enabled = false;
        attackTimer = 0.0f;
    }
}