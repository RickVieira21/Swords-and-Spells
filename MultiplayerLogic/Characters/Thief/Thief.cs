using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

//Classe que representa a classe de personagem Thief
public class Thief : NetworkBehaviour
{
    public bool stealthMovement = false;
    private BaseCharacter baseCharacter;
    private ThiefWeapon weapon;

    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    public HealthBar staminaBar;

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

    public AudioSource audioAbility1;
    public AudioSource audioAbility2;

    public Image FadeImage;


    private void Awake()
    {
        baseCharacter = GetComponent<BaseCharacter>();
        weapon = GetComponentInChildren<ThiefWeapon>();
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
            HandleInput(); //gestão de inputs das habilidades
            RegenerateStamina(); //Regenerar Stamina ao longo do tempo
            //Definir cooldowns para as habilidades
            AbilityCooldown(ref currentAbility1Cooldown, ability1Cooldown, ref isAbility1Cooldown, abilityImage1, abilityText1);
            AbilityCooldown(ref currentAbility2Cooldown, ability2Cooldown, ref isAbility2Cooldown, abilityImage2, abilityText2);
        }
    }

    //Gestão dos inputs utilizados para executar as habilidades
    private void HandleInput()
    {
        //iniciar a segunda Habilidade - Crouch
        if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.C) && stealthMovement == false && !isAbility2Cooldown)
        {
            if (IsServer)
            {
                StartStealthMovement();
            }
            else if (IsClient)
            {
                StartStealthMovementServerRPC();
            }
        }
        //cancelar a segunda Habilidade
        else if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.C) && stealthMovement == true)
        {
            if (IsServer)
            {
                StopStealthMovement();
            }
            else if (IsClient)
            {
                StopStealthMovementServerRPC();
            }
        }
        //Aumentar a velocidade, enquanto no modo Stealth, ou seja, durante a Segunda Habilidade
        if (isAbility2Unlocked && Input.GetKey(KeyCode.LeftShift) && stealthMovement == true)
        {
            if (IsServer)
            {
                ToggleStealthSpeed();
                DrainMana(20f);
            }
            else if (IsClient)
            {
                ToggleStealthSpeedServerRPC();
                DrainMana(20f);
            }
        }
        //Definir a velocidade do Thief equivalente à sua velocidade padrão, uma vez que não está a manter a tecla LEFT SHIFT premida
        else if (isAbility2Unlocked && !Input.GetKey(KeyCode.LeftShift) && stealthMovement == true)
        {
            if (IsServer)
            {
                ToggleNormalSpeed();
            }
            else if (IsClient)
            {
                ToggleNormalSpeedServerRPC();
            }
        }
        //Primeira Habilidade - Backstab
        if (Input.GetKeyDown(KeyCode.E) && currentStamina >= 10f && !isAbility1Cooldown)
        {
            if (IsServer)
            {
                BackstabStart();             
            }
            else if (IsClient)
            {
                BackstabStartServerRPC();
            }
            currentStamina -= 10f;
            staminaBar.SetHealth(currentStamina);  
            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;
        }
    }

    //Regenera mana automaticamente com o tempo
    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += 5f * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            staminaBar.SetHealth(currentStamina); 
        }
    }

    //Drenar Mana; Método utilizado sempre que uma habilidade é utilizada; No caso do Thief é Stamina, mas o método também possui o nome DrainMana
     private void DrainMana(float amount)
    {
        if (currentStamina > 0)
        {
            currentStamina -= amount * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            staminaBar.SetHealth(currentStamina);

            // Verifica se a stamina acabou e desativa stealthMovement
            if (currentStamina <= 0)
            {
                if (IsServer)
                {
                    StopStealthMovement();
                }
                else if (IsClient)
                {
                    StopStealthMovementServerRPC();                   
                }
            }
        }
    }


    private void AbilityCooldown(ref float currentCooldown, float maxCooldown, ref bool isCooldown, Image skillImage, Text skillText)
    {
        if(isCooldown)
        {
            currentCooldown -= Time.deltaTime;

            if(currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;

                if(skillImage != null)
                {
                    skillImage.fillAmount = 0f;
                }
                if(skillText != null)
                {
                    skillText.text = "";
                }
            }
            else
            {
                if(skillImage != null)
                {
                    skillImage.fillAmount = currentCooldown / maxCooldown;
                }
                if(skillText != null)
                {
                    skillText.text = Mathf.Ceil(currentCooldown).ToString();
                }
            }
        }
    }

    
    public void ToggleStealthSpeed()
    {
        if (!baseCharacter.isAlive)
            return;
        baseCharacter.currentMovementSpeed = baseCharacter.stealthMovementSpeed;
        ToggleStealthSpeedClientRPC();
    }

    [ServerRpc]
    public void ToggleStealthSpeedServerRPC()
    {
        ToggleStealthSpeed();
    }

    public void ToggleNormalSpeed()
    {
        if (!baseCharacter.isAlive)
            return;
        baseCharacter.currentMovementSpeed = baseCharacter.baseMovementSpeed;
        ToggleNormalSpeedClientRPC();
    }

    [ServerRpc]
    public void ToggleNormalSpeedServerRPC()
    {
        ToggleNormalSpeed();
    }

    public void StartStealthMovement()
    {
        if (!baseCharacter.isAlive)
            return;
        stealthMovement = true;
        baseCharacter.myAnimator.SetBool("StealthMovement", true);
        StartStealthMovementClientRPC();
        PlayAbility2AudioClientRPC(); 
    }

    [ServerRpc]
    public void StartStealthMovementServerRPC()
    {
        StartStealthMovement();
    }

    public void StopStealthMovement()
    {
        if (!baseCharacter.isAlive)
            return;
        stealthMovement = false;
        baseCharacter.myAnimator.SetBool("StealthMovement", false);
        StopStealthMovementClientRPC();
        isAbility2Cooldown = true;
        currentAbility2Cooldown = ability2Cooldown;
        StopAbility2AudioClientRPC();
    }

    [ServerRpc]
    public void StopStealthMovementServerRPC()
    {
        StopStealthMovement();
    }

    public void BackstabStart()
    {
        BackstabStartClientRPC();
        PlayAbility1AudioClientRPC();
        baseCharacter.myAnimator.SetTrigger("Spell1_Backstab");
    }

    [ServerRpc]
    public void BackstabStartServerRPC()
    {
        BackstabStart();
    }

    public void BackstabTriggerStart()
    {
        BackstabTriggerStartClientRPC();
        weapon.GetComponent<BoxCollider>().isTrigger = true;
    }

    [ServerRpc]
    public void BackstabTriggerStartServerRPC()
    {
        BackstabTriggerStart();
    }

    public void BackstabTriggerStop()
    {
        BackstabTriggerStopClientRPC();
        weapon.GetComponent<BoxCollider>().isTrigger = false;
    }

    [ServerRpc]
    public void BackstabTriggerStopServerRPC()
    {
        BackstabTriggerStop();
    }

    // ********************************************************************************************************
    // RPCs do lado do cliente; chamada remota de execução de código do Servidor, para o Cliente
    // Garantir que as animações acontecem do lado do Cliente

    [ClientRpc]
    public void RpcSetWalkingAnimationClientRPC(bool walking)
    {
        baseCharacter.myAnimator.SetBool("walking", walking);
    }

    [ClientRpc]
    public void StartStealthMovementClientRPC()
    {
        stealthMovement = true;
        baseCharacter.myAnimator.SetBool("StealthMovement", true);
    }

    [ClientRpc]
    private void StopStealthMovementClientRPC()
    {
        stealthMovement = false;
        baseCharacter.myAnimator.SetBool("StealthMovement", false);
        isAbility2Cooldown = true;
        currentAbility2Cooldown = ability2Cooldown;
    }

    [ClientRpc]
    public void ToggleStealthSpeedClientRPC()
    {
        baseCharacter.currentMovementSpeed = baseCharacter.stealthMovementSpeed;
    }

    [ClientRpc]
    public void ToggleNormalSpeedClientRPC()
    {
        baseCharacter.currentMovementSpeed = baseCharacter.baseMovementSpeed;
    }

    [ClientRpc]
    public void BackstabStartClientRPC()
    {
        baseCharacter.myAnimator.SetTrigger("Spell1_Backstab");
    }

    [ClientRpc]
    public void BackstabTriggerStartClientRPC()
    {
        weapon.GetComponent<BoxCollider>().isTrigger = true;
    }

    [ClientRpc]
    public void BackstabTriggerStopClientRPC()
    {
        weapon.GetComponent<BoxCollider>().isTrigger = false;
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

    [ClientRpc]
    public void StopAbility2AudioClientRPC()
    {
        audioAbility2.Stop();
    }

}