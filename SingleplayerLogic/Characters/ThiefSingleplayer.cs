using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

//Classe semelhante a Thief. No entanto, deriva de MonoBehaviour, de modo a poder ser utilizada em sessões de jogo Singleplayer
public class ThiefSingleplayer : MonoBehaviour
{
    public bool stealthMovement = false;
    private BaseCharacterSingleplayer baseCharacter;
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
        baseCharacter = GetComponent<BaseCharacterSingleplayer>();
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
        if (baseCharacter.isAlive)
        {
            HandleInput();
            RegenerateStamina();
            AbilityCooldown(ref currentAbility1Cooldown, ability1Cooldown, ref isAbility1Cooldown, abilityImage1, abilityText1);
            AbilityCooldown(ref currentAbility2Cooldown, ability2Cooldown, ref isAbility2Cooldown, abilityImage2, abilityText2);
        }
    }

    private void HandleInput()
    {
        if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.C) && stealthMovement == false && !isAbility2Cooldown)
        {
                StartStealthMovement();
        }
        else if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.C) && stealthMovement == true)
        {
                StopStealthMovement();
        }

        if (isAbility2Unlocked && Input.GetKey(KeyCode.LeftShift) && stealthMovement == true)
        {
                ToggleStealthSpeed();
                DrainMana(20f);
        }
        else if (isAbility2Unlocked && !Input.GetKey(KeyCode.LeftShift) && stealthMovement == true)
        {
                ToggleNormalSpeed();
        }

        if (Input.GetKeyDown(KeyCode.E) && currentStamina >= 25f && !isAbility1Cooldown)
        {
            BackstabStart();
            currentStamina -= 25f;
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
                    StopStealthMovement();
            }
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


    public void ToggleStealthSpeed()
    {
        if (!baseCharacter.isAlive)
            return;
        baseCharacter.currentMovementSpeed = baseCharacter.stealthMovementSpeed;
    }


    public void ToggleNormalSpeed()
    {
        if (!baseCharacter.isAlive)
            return;
        baseCharacter.currentMovementSpeed = baseCharacter.baseMovementSpeed;
    }

    public void StartStealthMovement()
    {
        if (!baseCharacter.isAlive)
            return;
        stealthMovement = true;
        baseCharacter.myAnimator.SetBool("StealthMovement", true);
        audioAbility2.Play();
    }


    public void StopStealthMovement()
    {
        if (!baseCharacter.isAlive)
            return;
        stealthMovement = false;
        baseCharacter.myAnimator.SetBool("StealthMovement", false);
        isAbility2Cooldown = true;
        currentAbility2Cooldown = ability2Cooldown;
        audioAbility2.Stop();
    }

    public void BackstabStart()
    {
        audioAbility1.Play();
        baseCharacter.myAnimator.SetTrigger("Spell1_Backstab");
    }

    public void BackstabTriggerStart()
    {
        weapon.GetComponent<BoxCollider>().isTrigger = true;
    }

    public void BackstabTriggerStop()
    {
        weapon.GetComponent<BoxCollider>().isTrigger = false;
    }

}