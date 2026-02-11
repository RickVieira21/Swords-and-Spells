using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe que representa a arma do Guerreiro
public class WarriorWeapon : MonoBehaviour
{
    //danos diferentes, dependendo do modo de jogo que o jogador está a jogar
    private float damageMultiplayer;
    private float damageSingleplayer;

    public void SetDamageMultiplayer(float newDamage)
    {
        this.damageMultiplayer = newDamage;
    }

    public void SetDamageSingleplayer(float newDamage)
    {
        this.damageSingleplayer = newDamage;
    }

    //causar dano ao inimigo, se a arma entrar em contacto com o mesmo
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("Warrior atingiu um Inimigo");
            Enemy enemy = c.gameObject.GetComponent<Enemy>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageMultiplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
            }
        }
        else if (c.gameObject.CompareTag("SEnemy"))
        {
            Debug.LogWarning("Warrior atingiu um Inimigo");
            EnemySingleplayer enemy = c.gameObject.GetComponent<EnemySingleplayer>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageSingleplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
            }
        }
    }
}
