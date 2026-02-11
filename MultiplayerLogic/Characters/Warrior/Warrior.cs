using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

//Classe que representa o Guerreiro
public class Warrior : NetworkBehaviour
{
    private BaseCharacter baseCharacter;

    private WarriorWeapon weapon;
    private float slashDamage = 6f;

    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    public HealthBar staminaBar;

    //COOLDOWNS UI

    [Header("Ability 1")]
    public Image abilityImage1;
    public Text abilityText1;
    public float ability1Cooldown = 3.0f;

    [Header("Ability 2")]
    public Image abilityImage2;
    public Text abilityText2;
    public float ability2Cooldown = 10.0f;

    private bool isAbility1Cooldown = false;
    private bool isAbility2Cooldown = false;
    private bool isProvoking = false;

    private float currentAbility1Cooldown;
    private float currentAbility2Cooldown;

    public bool isAbility2Unlocked = false;
    public Image ImageSkill2;

    public AudioSource audioAbility1;
    public AudioSource audioAbility2;

    public Image FadeImage;

    private void Awake()
    {
        baseCharacter = GetComponent<BaseCharacter>();
        weapon = GetComponentInChildren<WarriorWeapon>();
        weapon.SetDamageMultiplayer(slashDamage);
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
            RegenerateStamina();
            AbilityCooldown(ref currentAbility1Cooldown, ability1Cooldown, ref isAbility1Cooldown, abilityImage1, abilityText1);
            AbilityCooldown(ref currentAbility2Cooldown, ability2Cooldown, ref isAbility2Cooldown, abilityImage2, abilityText2);
        }
    }

    //gestão de inputs das habilidades
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentStamina >= 25f && !isAbility1Cooldown)
        {
            if (IsServer)
            {
                SlashStart();
            }
            else if (IsClient)
            {
                SlashStartServerRPC();
            }

            currentStamina -= 25f;
            staminaBar.SetHealth(currentStamina);
            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;
        }

        if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.E) && currentStamina >= 50f && !isAbility2Cooldown && !isProvoking)
        {
            if (IsServer)
            {
                ProvokeStart();
            }
            else if (IsClient)
            {
                ProvokeStartServerRPC();
            }
            currentStamina -= 50f;
            staminaBar.SetHealth(currentStamina);
            isAbility2Cooldown = true;
            currentAbility2Cooldown = ability2Cooldown;
        }
    }

    //Regenera stamina automaticamente com o tempo
    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += 5f * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            staminaBar.SetHealth(currentStamina);
        }
    }

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

    // ************************ Habilidade 1 -> Slash

    public void SlashStart()
    {
        SlashStartClientRPC();
        PlayAbility1AudioClientRPC();
        baseCharacter.myAnimator.SetTrigger("Slash");
    }


    [ClientRpc]
    public void SlashStartClientRPC()
    {
        baseCharacter.myAnimator.SetTrigger("Slash");
    }

    [ServerRpc]
    public void SlashStartServerRPC()
    {
        SlashStart();
    }

    public void SlashTriggerStart()
    {
        SlashTriggerStartClientRPC();
        weapon.gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    [ServerRpc]
    public void SlashTriggerStartServerRPC()
    {
        SlashTriggerStart();
    }

    [ClientRpc]
    public void SlashTriggerStartClientRPC()
    {
        weapon.gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    public void SlashTriggerStop()
    {
        SlashTriggerStopClientRPC();
        weapon.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    [ServerRpc]
    public void SlashTriggerStopServerRPC()
    {
        SlashTriggerStop();
    }

    [ClientRpc]
    public void SlashTriggerStopClientRPC()
    {
        weapon.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    // ********************************************** Habilidade 2 -PROVOKE
    public void ProvokeStart()
    {
        ProvokeStartClientRPC();
        PlayAbility2AudioClientRPC();
        baseCharacter.myAnimator.SetTrigger("Provoke");
    }


    [ClientRpc]
    public void ProvokeStartClientRPC()
    {
        baseCharacter.myAnimator.SetTrigger("Provoke");
    }

    [ServerRpc]
    public void ProvokeStartServerRPC()
    {
        ProvokeStart();
    }

    public void ProvokeTriggerStart()
    {
        ProvokeTriggerStartClientRPC();
        baseCharacter.SetCanMove(false);
        StartCoroutine(AttractMonsters());
    }

    //Provocar todos os inimigos presentes no mapa, durante uma determinada quantidade de tempo
    private IEnumerator AttractMonsters()
    {
        float provokeDuration = 5f;
        float elapsedTime = 0f;

        while (elapsedTime < provokeDuration && baseCharacter.isAlive)
        {
            elapsedTime += Time.deltaTime;
            foreach (var skeleton in baseCharacter.GetGameManager().skeletonList)
            {
                if (skeleton != null && skeleton.GetEnemy().isAlive)
                {
                    skeleton.GetEnemy().SetTarget(this.transform);
                    skeleton.GetEnemy().SetProvokedStatus(true);
                    skeleton.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    skeleton.MoveAgent();
                }
            }
            foreach (var ghoul in baseCharacter.GetGameManager().ghoulList)
            {
                if (ghoul != null && ghoul.GetEnemy().isAlive)
                {
                    ghoul.GetEnemy().SetTarget(this.transform);
                    ghoul.GetEnemy().SetProvokedStatus(true);
                    ghoul.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    ghoul.MoveAgent();
                }
            }
            foreach (var subBoss in baseCharacter.GetGameManager().subBossList)
            {
                if (subBoss != null && subBoss.GetEnemy().isAlive)
                {
                    subBoss.GetEnemy().SetTarget(this.transform);
                    subBoss.GetEnemy().SetProvokedStatus(true);
                    subBoss.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    subBoss.MoveAgent();
                }
            }
            foreach (var boss in baseCharacter.GetGameManager().bossList)
            {
                if (boss != null && boss.GetEnemy().isAlive)
                {
                    boss.GetEnemy().SetTarget(this.transform);
                    boss.GetEnemy().SetProvokedStatus(true);
                    boss.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    boss.MoveAgent();
                }
            }
            yield return null;
        }
        StopProvoking();
        isProvoking = false;
    }

    //Cancelar a provocação, fazendo com que os inimigos persigam os respetivos alvos que se enontrem mais próximos novamente
    public void StopProvoking()
    {
        StopCoroutine(AttractMonsters());
        isProvoking = false;
        foreach (var skeleton in baseCharacter.GetGameManager().skeletonList)
        {
            if (skeleton != null && skeleton.GetEnemy().isAlive)
            {
                skeleton.GetEnemy().SetProvokedStatus(false);
            }
        }
        foreach (var ghoul in baseCharacter.GetGameManager().ghoulList)
        {
            if (ghoul != null && ghoul.GetEnemy().isAlive)
            {
                ghoul.GetEnemy().SetProvokedStatus(false);
            }
        }
        foreach (var subBoss in baseCharacter.GetGameManager().subBossList)
        {
            if (subBoss != null && subBoss.GetEnemy().isAlive)
            {
                subBoss.GetEnemy().SetProvokedStatus(false);
            }
        }
        foreach (var boss in baseCharacter.GetGameManager().bossList)
        {
            if (boss != null && boss.GetEnemy().isAlive)
            {
                boss.GetEnemy().SetProvokedStatus(false);
            }
        }
    }

    [ServerRpc]
    public void ProvokeTriggerStartServerRPC()
    {
        ProvokeTriggerStart();
    }

    [ClientRpc]
    public void ProvokeTriggerStartClientRPC()
    {
        baseCharacter.SetCanMove(false);
        StartCoroutine(AttractMonsters());
    }

    public void ProvokeTriggerStop()
    {
        ProvokeTriggerStopClientRPC();
        baseCharacter.SetCanMove(true);
    }

    [ServerRpc]
    public void ProvokeTriggerStopServerRPC()
    {
        ProvokeTriggerStop();
    }

    [ClientRpc]
    public void ProvokeTriggerStopClientRPC()
    {
        baseCharacter.SetCanMove(true);
    }

    // ********************************************************************************************************
    // RPCs do lado do cliente; Executados pelo Servidor, e recebidos pelo Cliente
    // Garantir que as animações acontecem do lado do Cliente e, por conseguinte, sincronizadas em ambas as máquinas (Host e Clientes)

    [ClientRpc]
    public void RpcSetWalkingAnimationClientRPC(bool walking)
    {
        baseCharacter.myAnimator.SetBool("walking", walking);
    }

    [ClientRpc]
    public void PlayAbility1AudioClientRPC()
    {
        audioAbility1.Play();
    }

    [ClientRpc]
    public void PlayAbility2AudioClientRPC()
    {
        audioAbility2.Play();
    }

}