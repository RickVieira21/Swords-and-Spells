using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script associado à mão do Ghoul; responsável por causar dano aos jogadores, quando o Ghoul realiza a sua animação de ataque e o seu braço entra em contacto com jogadores
public class GhoulHand : MonoBehaviour
{
    private readonly float damage = 3f;
    private Ghoul me;
    public HealthBar healthBar;
    private Enemy myEnemyInfo;

    private GhoulSingleplayer me_s;
    private EnemySingleplayer mySingleplayerInfo;

    private void Awake()
    {
        me = transform.root.GetComponent<Ghoul>();
        myEnemyInfo = transform.root.GetComponent<Enemy>();
        mySingleplayerInfo = transform.root.GetComponent<EnemySingleplayer>();
    }

    //Detetar se o Box Collider associado à mão do Ghoul, que atua como Trigger, entra em contacto com um jogador
    //Se sim, causar dano ao jogador
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && myEnemyInfo.GetHealth() > 0.0f)
        {
            BaseCharacter playerHit = other.gameObject.GetComponent<BaseCharacter>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("I Hit a Player with a Ghoul Attack");
        }
        else if (other.gameObject.CompareTag("SinglePlayer") && mySingleplayerInfo.GetHealth() > 0.0f)
        {
            BaseCharacterSingleplayer playerHit = other.gameObject.GetComponent<BaseCharacterSingleplayer>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("I Hit a Player with a Ghoul Attack");
        }
    }
}
