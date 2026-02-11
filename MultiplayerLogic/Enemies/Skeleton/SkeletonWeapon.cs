using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe associada à arma do Esqueleto, de modo a que esta seja capaz de causar dano a jogadores que entrem contacto com a mesma.
public class SkeletonWeapon : MonoBehaviour
{
    private readonly float damage = 2f;
    private Skeleton me;
    private Enemy myEnemyInfo;
    public HealthBar healthBar;

    private SkeletonSingleplayer me_s;
    private EnemySingleplayer mySingleplayerInfo;

    private void Awake()
    {
        me = transform.root.GetComponent<Skeleton>();
        myEnemyInfo = transform.root.GetComponent<Enemy>();

        me_s = transform.root.GetComponent<SkeletonSingleplayer>();
        mySingleplayerInfo = transform.root.GetComponent<EnemySingleplayer>();
    }

    //Causar dano ao jogador, quando o mesmo entra em contacto com o Box Collider presente na arma do Esqueleto, e que atua como um Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && myEnemyInfo.GetHealth() > 0.0f)
        {
            BaseCharacter playerHit = other.gameObject.GetComponent<BaseCharacter>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("I Hit a Player with a Skeleton Attack");
        }
        else if (other.gameObject.CompareTag("SinglePlayer") && mySingleplayerInfo.GetHealth() > 0.0f)
        {
            BaseCharacterSingleplayer playerHit = other.gameObject.GetComponent<BaseCharacterSingleplayer>();
            playerHit.SetHealth(playerHit.GetHealth() - damage);
            playerHit.myAnimator.SetFloat("health", playerHit.GetHealth());
            playerHit.healthBar.SetHealth(playerHit.GetHealth() - damage);
            Debug.LogWarning("I Hit a Player with a Skeleton Attack");
        }
    }
}
