using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Classe que representa o Lança-Chamas, a segunda habilidade do Mago
public class Spell2 : MonoBehaviour
{
    private float damageMultiplayer = 1f;
    private float damageSingleplayer = 1.5f;

    private float damageInterval = 0.1f; // Intervalo de tempo entre cada aplicação de dano
    private float lastDamageTime; // Último momento em que o dano foi aplicado

    private bool singleplayerMode;
    private bool multiplayerMode;

    private BaseCharacter me;
    private Mage myClass;

    private BaseCharacterSingleplayer meSingleplayer;
    private MageSingleplayer myClassSingleplayer;

    private void Awake()
    {
        me = GetComponentInParent<BaseCharacter>();
        myClass = GetComponentInParent<Mage>();
        meSingleplayer = GetComponentInParent<BaseCharacterSingleplayer>();
        myClassSingleplayer = GetComponentInParent<MageSingleplayer>();
        if(me != null && myClass != null)
        {
            singleplayerMode = false;
            multiplayerMode = true;
        }
        else if (meSingleplayer != null && myClassSingleplayer != null)
        {
            singleplayerMode = true;
            multiplayerMode = false;
        }
    }

    //Desativar o Lança-Chamas se o Mago morrer ou libertar a tecla E
    private void Update()
    {
        if (multiplayerMode)
        {
            this.transform.position = myClass.spell2Pos.transform.position;

            if ((Input.GetKeyUp(KeyCode.E) && myClass.spell2Used) || !me.isAlive)
            {
                if (me.IsLocalPlayer)
                {
                    if (me.IsServer)
                    {
                        myClass.TriggerSpell2(false);
                        myClass.spell2Used = false;
                    }
                    else if (me.IsClient)
                    {
                        myClass.TriggerSpell2ServerRPC(false);
                        myClass.spell2Used = false;
                    }
                }
            }
        } 
        else if (singleplayerMode)
        {
            this.transform.position = myClassSingleplayer.spell2Pos.transform.position;

            if ((Input.GetKeyUp(KeyCode.E) && myClassSingleplayer.spell2Used) || !meSingleplayer.isAlive)
            {
               Debug.LogWarning("spell 2 desativado");
               myClassSingleplayer.TriggerSpell2(false);
               myClassSingleplayer.spell2Used = false;
            }
        }
    }

    //Retirar vida a todos os inimigos que entram em contacto com o Lança-Chamas a cada intervalo de tempo definido
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && Time.time > lastDamageTime + damageInterval)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageMultiplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
                lastDamageTime = Time.time;
            }
        }
        else if (other.CompareTag("SEnemy") && Time.time > lastDamageTime + damageInterval)
        {
            EnemySingleplayer enemy = other.gameObject.GetComponent<EnemySingleplayer>();
            if (enemy.gameObject.activeSelf && enemy != null)
            {
                enemy.SetHealth(enemy.GetHealth() - damageSingleplayer);
                enemy.GetAnimator().SetFloat("Health", enemy.GetHealth());
                lastDamageTime = Time.time;
            }
        }
    }

}
