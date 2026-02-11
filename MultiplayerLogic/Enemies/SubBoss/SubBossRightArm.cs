using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script associado ao braço direito do Sub-Boss, de modo a que este seja capaz de causar dano a um jogador caso este se aproxime o suficiente
public class SubBossRightArm : MonoBehaviour
{
    private readonly float damage = 2f;
    private SubBoss me;
    public HealthBar healthBar;
    private Enemy myEnemyInfo;
    private EnemySingleplayer mySingleplayerInfo;

    private void Awake()
    {
        me = transform.root.GetComponent<SubBoss>();
        myEnemyInfo = transform.root.GetComponent<Enemy>();
        mySingleplayerInfo = transform.root.GetComponent<EnemySingleplayer>();
    }

    //Detetar se o Box Collider associado ao braço direito do Sub-Boss, que atua como Trigger, entra em contacto com um jogador
    //Se sim, causar dano ao jogador
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && myEnemyInfo.GetHealth() > 0.0f)
        {
            BaseCharacter playerHit = other.gameObject.GetComponent<BaseCharacter>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("SubBoss Hit a Player with its right arm");
        }
        else if (other.gameObject.CompareTag("SinglePlayer") && mySingleplayerInfo.GetHealth() > 0.0f)
        {
            BaseCharacterSingleplayer playerHit = other.gameObject.GetComponent<BaseCharacterSingleplayer>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("SubBoss Hit a Player with its left arm");
        }
    }
}
