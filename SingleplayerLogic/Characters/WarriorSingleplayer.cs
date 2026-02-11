using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

//Classe semelhante a Warrior. No entanto, deriva de MonoBehaviour, de modo a poder ser utilizada em sessões de jogo Singleplayer
public class WarriorSingleplayer : MonoBehaviour
{
    private BaseCharacterSingleplayer baseCharacter;

    private WarriorWeapon weapon;
    private float slashDamage = 14f;

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

    //private float provokeTimer = 0.0f;
    //private float maxProvokeDuration = 5.0f;


    private void Awake()
    {
        baseCharacter = GetComponent<BaseCharacterSingleplayer>();
        weapon = GetComponentInChildren<WarriorWeapon>();
        weapon.SetDamageSingleplayer(slashDamage);
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
        if (Input.GetKeyDown(KeyCode.Q) && currentStamina >= 25f && !isAbility1Cooldown)
        {
            SlashStart();
            currentStamina -= 25f;
            staminaBar.SetHealth(currentStamina);
            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;
        }

        if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.E) && currentStamina >= 50f && !isAbility2Cooldown && !isProvoking)
        {
            ProvokeStart();
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

    // ************************ Attack 1 -> Slash

    public void SlashStart()
    {
        audioAbility1.Play();
        baseCharacter.myAnimator.SetTrigger("Slash");
    }

    public void SlashTriggerStart()
    {
        weapon.gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    public void SlashTriggerStop()
    {
        weapon.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    // ********************************************** PROVOKE
    public void ProvokeStart()
    {
        audioAbility2.Play();
        baseCharacter.myAnimator.SetTrigger("Provoke");
    }

    public void ProvokeTriggerStart()
    {
        baseCharacter.SetCanMove(false);
        StartCoroutine(AttractMonsters());
    }

    private IEnumerator AttractMonsters()
    {
        float provokeDuration = 5f;
        float elapsedTime = 0f;

        while (elapsedTime < provokeDuration)
        {
            elapsedTime += Time.deltaTime;

            foreach (var skeleton in baseCharacter.GetGameManager().sList)
            {
                if (skeleton != null && skeleton.GetEnemy().isAlive)
                {
                    skeleton.GetEnemy().SetTarget(this.transform);
                    skeleton.GetEnemy().SetProvokedStatus(true);
                    skeleton.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    skeleton.MoveAgent();
                }
            }
            foreach (var ghoul in baseCharacter.GetGameManager().gList)
            {
                if (ghoul != null && ghoul.GetEnemy().isAlive)
                {
                    ghoul.GetEnemy().SetTarget(this.transform);
                    ghoul.GetEnemy().SetProvokedStatus(true);
                    ghoul.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    ghoul.MoveAgent();
                }
            }
            foreach (var subBoss in baseCharacter.GetGameManager().sbList)
            {
                if (subBoss != null && subBoss.GetEnemy().isAlive)
                {
                    subBoss.GetEnemy().SetTarget(this.transform);
                    subBoss.GetEnemy().SetProvokedStatus(true);
                    subBoss.GetEnemy().SetPlayerToFollow(this.baseCharacter);
                    subBoss.MoveAgent();
                }
            }
            foreach (var boss in baseCharacter.GetGameManager().bList)
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

    public void StopProvoking()
    {
        StopCoroutine(AttractMonsters());
        isProvoking = false;
        foreach (var skeleton in baseCharacter.GetGameManager().sList)
        {
            if (skeleton != null && skeleton.GetEnemy().isAlive)
            {
                skeleton.GetEnemy().SetProvokedStatus(false);
            }
        }
    }

    public void ProvokeTriggerStop()
    {
        baseCharacter.SetCanMove(true);
    }

}