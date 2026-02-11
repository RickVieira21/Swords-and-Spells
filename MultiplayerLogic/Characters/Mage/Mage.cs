using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UI;

//Classe que representa a Classe de Personagem "Mago"
public class Mage : NetworkBehaviour
{
    private BaseCharacter baseCharacter; //referência às informações e caracerísticas comuns a todas as classes de personagens

    //Informações associadas à primeira habilidade
    public Rigidbody spell1;
    public Transform spell1Pos;
    private Rigidbody spell1_throw;
    public float spell1Force = 600f;
    private bool canUseSpell1 = true;

    //Informações associadas à segunda habilidade
    public Rigidbody spell2;
    public Transform spell2Pos;
    private Rigidbody spell2Instance;
    public bool spell2Used = false;
    public bool spell2beingUsed = false;

    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana;
    public HealthBar manaBar;

    public AudioSource audioSpell1;
    public AudioSource audioSpell2;

    public Image FadeImage;

    //COOLDOWNS UI

    [Header("Ability 1")]
    public Image abilityImage1;
    public Text abilityText1;
    public float ability1Cooldown = 1.5f;

    [Header("Ability 2")]
    public Image abilityImage2;
    public Text abilityText2;
    public float ability2Cooldown = 2.0f;

    private bool isAbility1Cooldown = false;
    private bool isAbility2Cooldown = false;

    private float currentAbility1Cooldown;
    private float currentAbility2Cooldown;

    public bool isAbility2Unlocked = false;
    public Image ImageSkill2;


    private void Awake()
    {
        baseCharacter = GetComponent<BaseCharacter>();
        currentMana = maxMana;
        manaBar.SetMaxHealth(maxMana);
    }


    void Start()
    {
        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;

        abilityText1.text = "";
        abilityText2.text = "";
    }


    private void Update()
    {
        if (IsLocalPlayer && baseCharacter.isAlive)
        {
            HandleInput();
            RegenerateMana();
            AbilityCooldown(ref currentAbility1Cooldown, ability1Cooldown, ref isAbility1Cooldown, abilityImage1, abilityText1);
            AbilityCooldown(ref currentAbility2Cooldown, ability2Cooldown, ref isAbility2Cooldown, abilityImage2, abilityText2);
            if (spell2beingUsed)
            {
                DrainMana(20f);
            }
        }
        else if (IsLocalPlayer && !baseCharacter.isAlive)
        {
            if (spell2beingUsed)
            {
                if (IsServer)
                {
                    TriggerSpell2(false);
                }
                else if (IsClient)
                {
                    TriggerSpell2ServerRPC(false);
                }
            }
        }
    }

    //Gestão dos Inputs relacionados com as habilidades
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentMana >= 25f && !isAbility1Cooldown)
        {
            if (IsServer)
            {
                TriggerSpell1();
            }
            else if (IsClient)
            {
                TriggerSpell1ServerRPC();
            }
            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;
        }

        if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.E) && baseCharacter.myAnimator.GetBool("Spell2") == false &&
            !spell2Used && currentMana > 0f && !isAbility2Cooldown && baseCharacter.isAlive)
        {
            if (IsServer)
            {
                TriggerSpell2(true);
                spell2Used = true;
            }
            else if (IsClient)
            {
                TriggerSpell2ServerRPC(true);
                spell2Used = true;
            }

        }
    }

    //Regenera mana automaticamente com o tempo
    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += 5f * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            manaBar.SetHealth(currentMana);
        }
    }

    //Drena Mana enquanto o spell 2 está ativo
    private void DrainMana(float amount)
    {
        if (currentMana > 0)
        {
            currentMana -= amount * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            manaBar.SetHealth(currentMana);

            // Verifica se a mana acabou e desativa o feitiço 2
            if (currentMana <= 0)
            {
                if (IsServer)
                {
                    TriggerSpell2(false);
                    spell2Used = false;
                }
                else if (IsClient)
                {
                    TriggerSpell2ServerRPC(false);
                    spell2Used = false;
                }
            }
        }
    }

    //Aplicar Cooldown nas habilidades
    private void AbilityCooldown(ref float currentCooldown, float maxCooldown, ref bool isCooldown, Image skillImage, Text skillText)
    {
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;

                if (skillImage != null)
                {
                    skillImage.fillAmount = 0f;
                }
                if (skillText != null)
                {
                    skillText.text = "";
                }
            }
            else
            {
                if (skillImage != null)
                {
                    skillImage.fillAmount = currentCooldown / maxCooldown;
                }
                if (skillText != null)
                {
                    skillText.text = Mathf.Ceil(currentCooldown).ToString();
                }
            }
        }
    }

    //Utilizar a Habilidade 1; Instanciar o GameObject que representa a Bola de Fogo, aplicando-lhe uma força para que este seja projetado e, assim, atingir inimigos
    public void UseSpell1()
    {
        Debug.Log("Casting Spell 1: Fireball");
        spell1_throw = Instantiate(spell1, spell1Pos.position, spell1Pos.rotation);
        spell1_throw.AddForce(spell1Pos.forward * spell1Force);
        spell1_throw.gameObject.AddComponent<Spell1>();
        Destroy(spell1_throw.gameObject, 3.0f);
        currentMana -= 35f;
        manaBar.SetHealth(currentMana);
        baseCharacter.SetCanMove(true);
    }

    //Utilizar a Habilidade 2; Instanciar o GameObject que representa o Lança-Chamas
    public void UseSpell2()
    {
        Debug.Log("Casting Spell 2: Flamethrower");
        spell2beingUsed = true;
        spell2Instance = Instantiate(spell2, spell2Pos.position, spell2Pos.rotation);
        spell2Instance.transform.SetParent(transform);
        spell2Instance.gameObject.GetComponent<ParticleSystem>().Play();
        if (!spell2Instance.gameObject.GetComponent<Spell2>())
        {
            spell2Instance.gameObject.AddComponent<Spell2>();
        }
    }

    [ServerRpc]
    public void UseSpell1ServerRPC()
    {
        UseSpell1();
    }

    [ServerRpc]
    public void UseSpell2ServerRPC()
    {
        UseSpell2();
    }


    public void TriggerSpell1()
    {
        if (!baseCharacter.isAlive)
            return;

        TriggerSpell1ClientRPC();
        PlaySpell1AudioClientRPC();
        baseCharacter.myAnimator.SetTrigger("Spell1");
        baseCharacter.SetCanMove(false);
    }


    [ServerRpc]
    public void TriggerSpell1ServerRPC()
    {
        TriggerSpell1();
    }

    public void TriggerSpell2(bool value)
    {
        TriggerSpell2ClientRPC(value);
        baseCharacter.myAnimator.SetBool("Spell2", value);

        if (value == true)
        {
            PlaySpell2AudioClientRPC();
            baseCharacter.SetCanMove(false);
        }
        if (value == false)
        {
            spell2beingUsed = false;
            spell2Instance.gameObject.GetComponent<ParticleSystem>().Stop();
            Destroy(spell2Instance.gameObject, 2.0f);
            isAbility2Cooldown = true;
            currentAbility2Cooldown = ability2Cooldown;
            StopSpell2AudioClientRPC();
            baseCharacter.SetCanMove(true);
        }
    }

    [ServerRpc]
    public void TriggerSpell2ServerRPC(bool value)
    {
        TriggerSpell2(value);
    }

    // **********************************************************************
    // RPCs do lado do cliente
    // garantir que as animações acontecem do lado do cliente

    [ClientRpc]
    public void RpcSetWalkingAnimationClientRPC(bool walking)
    {
        baseCharacter.myAnimator.SetBool("walking", walking);
    }

    [ClientRpc]
    public void TriggerSpell1ClientRPC()
    {
        baseCharacter.myAnimator.SetTrigger("Spell1");
        baseCharacter.SetCanMove(false);
    }

    [ClientRpc]
    public void TriggerSpell2ClientRPC(bool value)
    {
        baseCharacter.myAnimator.SetBool("Spell2", value);
        if (value == true)
        {
            baseCharacter.SetCanMove(false);
        }
        if (value == false)
        {
            spell2beingUsed = false;
            spell2Instance.gameObject.GetComponent<ParticleSystem>().Stop();
            Destroy(spell2Instance.gameObject, 2.0f);
            isAbility2Cooldown = true;
            currentAbility2Cooldown = ability2Cooldown;
            baseCharacter.SetCanMove(true);
        }
    }

    [ClientRpc]
    public void UseSpell1AnimationClientRPC(bool canUse)
    {
        canUseSpell1 = canUse;
        baseCharacter.myAnimator.SetBool("canUseSpell1", canUse);
    }

    [ClientRpc]
    public void PlaySpell1AudioClientRPC()
    {
        audioSpell1.Play();
    }

    [ClientRpc]
    public void PlaySpell2AudioClientRPC()
    {
        audioSpell2.Play();
    }

    [ClientRpc]
    public void StopSpell2AudioClientRPC()
    {
        audioSpell2.Stop();
    }

}
