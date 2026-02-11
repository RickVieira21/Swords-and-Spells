using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script que é associado ao projétil gerado pelo Sub-Boss, durante o seu ataque à longa distância
public class SBAttack1 : MonoBehaviour
{
    private readonly float attack1Damage = 5f; //dano do projétil

    //Detetar se o projétil entra em contacto com um jogador. Se sim, causar dano.
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            Debug.LogWarning("Attack1 hit a player!");
            BaseCharacter enemy = c.gameObject.GetComponent<BaseCharacter>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - attack1Damage);
                enemy.myAnimator.SetFloat("health", enemy.GetHealth());
                enemy.healthBar.SetHealth(enemy.GetHealth() - attack1Damage);
            }
        }
        else if (c.gameObject.CompareTag("SinglePlayer"))
        {
            Debug.LogWarning("Attack1 hit a player!");
            BaseCharacterSingleplayer enemy = c.gameObject.GetComponent<BaseCharacterSingleplayer>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - attack1Damage);
                enemy.myAnimator.SetFloat("health", enemy.GetHealth());
                enemy.healthBar.SetHealth(enemy.GetHealth() - attack1Damage);
            }
        }
    }
}
