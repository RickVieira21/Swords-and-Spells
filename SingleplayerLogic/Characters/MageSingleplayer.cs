using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UI;

//Classe semelhante a Mage. No entanto, deriva de MonoBehaviour, de modo a poder ser utilizada em sessões de jogo Singleplayer
public class MageSingleplayer : MonoBehaviour
{
    private BaseCharacterSingleplayer baseCharacter;

    public Rigidbody spell1;
    public Transform spell1Pos;
    private Rigidbody spell1_throw;
    public float spell1Force = 600f;
    [SerializeField] float spell1Cooldown = 5f;
    private bool canUseSpell1 = true;

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
        baseCharacter = GetComponent<BaseCharacterSingleplayer>();
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
        if (baseCharacter.isAlive)
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
        else if(!baseCharacter.isAlive)
        {
            if (spell2beingUsed)
            {
                Debug.LogWarning("Não estou vivo, vou parar de usar o spell 2");
                TriggerSpell2(false);
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentMana >= 25f && !isAbility1Cooldown)
        {
            TriggerSpell1();
            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;
        }

        if (isAbility2Unlocked && Input.GetKeyDown(KeyCode.E) && baseCharacter.myAnimator.GetBool("Spell2") == false 
            && !spell2Used && currentMana > 0f && !isAbility2Cooldown && baseCharacter.isAlive)
        {
            TriggerSpell2(true);
            spell2Used = true;
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

    //Tira mana enquanto o spell 2 está ativo
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
                    TriggerSpell2(false);
                    spell2Used = false;
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


    public void UseSpell1()
    {
        Debug.Log("Casting Spell 1: Fireball");
        spell1_throw = Instantiate(spell1, spell1Pos.position, spell1Pos.rotation);
        spell1_throw.AddForce(spell1Pos.forward * spell1Force);
        spell1_throw.gameObject.AddComponent<Spell1>();
        Destroy(spell1_throw.gameObject, 3.0f);
        currentMana -= 25f;
        manaBar.SetHealth(currentMana);
        baseCharacter.SetCanMove(true);
    }

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

    public void TriggerSpell1()
    {
        if (!baseCharacter.isAlive)
            return;
        baseCharacter.myAnimator.SetTrigger("Spell1");
        baseCharacter.SetCanMove(false);
    }

    public void TriggerSpell2(bool value)
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
}
