using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe que representa a Bola de Fogo lançada pelo Mago, na sua primeira habilidade
public class Spell1 : MonoBehaviour
{
    //A habilidade causa dano diferente, dependendo do modo de jogo que o jogador está a jogar
    private float damageMultiplayer = 10f;
    private float damageSingleplayer = 15f;

    //Se a Bola de Fogo entrar em contacto com um inimigo, causar dano correspondente ao dano da própria Bola de Fogo
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("Bola de fogo atingiu um Inimigo");
            Enemy enemy = c.gameObject.GetComponent<Enemy>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageMultiplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
            }
        }
        else if (c.gameObject.CompareTag("SEnemy"))
        {
            Debug.LogWarning("Bola de fogo atingiu um Inimigo");
            EnemySingleplayer e = c.gameObject.GetComponent<EnemySingleplayer>();
            if (e.gameObject.activeSelf && e != null)
            {
                e.SetHealth(e.GetHealth() - damageSingleplayer);
                e.GetAnimator().SetFloat("Health", e.GetHealth());
            }
        }
    }

}
