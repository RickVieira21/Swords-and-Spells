using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe associada à arma do Thief, de modo a que esta seja capaz de colidir com inimigos, e causar dano
public class ThiefWeapon : MonoBehaviour
{
    //A arma do Thief causa dano diferente, conforme o modo de jogo que o jogador está a jogar
    private readonly float damageMultiplayer = 8f;
    private float damageSingleplayer = 14f;

    //Se a arma entrar em contacto com um inimigo, causar-lhe dano correspondente ao dano da arma
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("Thief atingiu um Inimigo");
            Enemy enemy = c.gameObject.GetComponent<Enemy>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageMultiplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
            }
        }
        else if (c.gameObject.CompareTag("SEnemy"))
        {
            Debug.LogWarning("Thief atingiu um Inimigo");
            EnemySingleplayer enemy = c.gameObject.GetComponent<EnemySingleplayer>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageSingleplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
            }
        }
    }
}
