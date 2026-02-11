using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe associada à mão do Boss, responsável por causar dano ao jogador quando o mesmo se aproxima o suficiente do Boss
public class BossHand : MonoBehaviour
{
    private readonly float damage = 15f;
    private Boss me;
    public HealthBar healthBar;
    private Enemy myEnemyInfo;
    private EnemySingleplayer mySingleplayerInfo;

    private void Awake()
    {
        me = transform.root.GetComponent<Boss>();
        myEnemyInfo = transform.root.GetComponent<Enemy>();
        mySingleplayerInfo = transform.root.GetComponent<EnemySingleplayer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && myEnemyInfo.GetHealth() > 0.0f)
        {
            BaseCharacter playerHit = other.gameObject.GetComponent<BaseCharacter>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("The Boss Has Hit a Player with its left Arm");
        }
        else if (other.gameObject.CompareTag("SinglePlayer") && mySingleplayerInfo.GetHealth() > 0.0f)
        {
            BaseCharacterSingleplayer playerHit = other.gameObject.GetComponent<BaseCharacterSingleplayer>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("Boss Hit a Player with its left arm");
        }
    }
}
