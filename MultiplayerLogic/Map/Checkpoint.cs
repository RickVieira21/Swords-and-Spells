using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private string playerTag = "Player";
    private string singleplayerTag = "SinglePlayer";
    public AudioSource audioSource;
    public bool unlockLevel1 = false;
    public bool unlockLevel2 = false;
    public bool unlockLevel3 = false;
    public bool unlockLevel4 = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            BaseCharacter baseCharacter = other.GetComponent<BaseCharacter>();

            if (baseCharacter != null && unlockLevel1 && baseCharacter.level == 1)
            {
                Debug.Log("Checkpoint ativado - Level 2");
                audioSource.Play();
                baseCharacter.level++; 
                RegeneratePlayerStats(baseCharacter);
            }

            if (baseCharacter != null && unlockLevel2 && baseCharacter.level == 2)
            {
                Debug.Log("Checkpoint ativado - Level 3");
                audioSource.Play();
                baseCharacter.level++; 
                RegeneratePlayerStats(baseCharacter);
                UnlockAbility2(baseCharacter);
            }

            if (baseCharacter != null && unlockLevel3 && baseCharacter.level == 3)
            {
                Debug.Log("Checkpoint ativado - Level 4");
                audioSource.Play();
                baseCharacter.level++; 
                RegeneratePlayerStats(baseCharacter);
            }

            if (baseCharacter != null && unlockLevel4 && baseCharacter.level == 4)
            {
                Debug.Log("Checkpoint ativado - Level 5");
                audioSource.Play();
                baseCharacter.level++; 
                RegeneratePlayerStats(baseCharacter);
            }

        }
        else if (other.CompareTag(singleplayerTag))
        {
            BaseCharacterSingleplayer baseCharacter = other.GetComponent<BaseCharacterSingleplayer>();

            if (baseCharacter != null && unlockLevel1 && baseCharacter.level == 1)
            {
                Debug.Log("Checkpoint ativado - Level 2");
                audioSource.Play();
                baseCharacter.level++;
                RegenerateSingleplayerStats(baseCharacter);
            }

            if (baseCharacter != null && unlockLevel2 && baseCharacter.level == 2)
            {
                Debug.Log("Checkpoint ativado - Level 3");
                audioSource.Play();
                baseCharacter.level++;
                RegenerateSingleplayerStats(baseCharacter);
                UnlockSingleplayerAbility2(baseCharacter);
            }

            if (baseCharacter != null && unlockLevel3 && baseCharacter.level == 3)
            {
                Debug.Log("Checkpoint ativado - Level 4");
                audioSource.Play();
                baseCharacter.level++;
                RegenerateSingleplayerStats(baseCharacter);
            }

            if (baseCharacter != null && unlockLevel4 && baseCharacter.level == 4)
            {
                Debug.Log("Checkpoint ativado - Level 5");
                audioSource.Play();
                baseCharacter.level++;
                RegenerateSingleplayerStats(baseCharacter);
            }

        }
    }

    private void RegeneratePlayerStats(BaseCharacter baseCharacter)
    {
        baseCharacter.RegenerateHealth();
    }

    private void RegenerateSingleplayerStats(BaseCharacterSingleplayer baseCharacter)
    {
        baseCharacter.RegenerateHealth();
    }

    //Desbloquear imagem do skill2
    private void UnlockAbility2(BaseCharacter baseCharacter)
    {
        Mage mage = baseCharacter.GetComponent<Mage>();
        Thief thief = baseCharacter.GetComponent<Thief>();
        Warrior warrior = baseCharacter.GetComponent<Warrior>();

        if (mage != null)
        {
            mage.isAbility2Unlocked = true;
            mage.ImageSkill2.enabled = false; 
        }
        else if (thief != null)
        {
            thief.isAbility2Unlocked = true;
            thief.ImageSkill2.enabled = false; 
        }
        else if (warrior != null)
        {
            warrior.isAbility2Unlocked = true;
            warrior.ImageSkill2.enabled = false; 
        }
    }

    private void UnlockSingleplayerAbility2(BaseCharacterSingleplayer baseCharacter)
    {
        MageSingleplayer mage = baseCharacter.GetComponent<MageSingleplayer>();
        ThiefSingleplayer thief = baseCharacter.GetComponent<ThiefSingleplayer>();
        WarriorSingleplayer warrior = baseCharacter.GetComponent<WarriorSingleplayer>();

        if (mage != null)
        {
            mage.isAbility2Unlocked = true;
            mage.ImageSkill2.enabled = false;
        }
        else if (thief != null)
        {
            thief.isAbility2Unlocked = true;
            thief.ImageSkill2.enabled = false;
        }
        else if (warrior != null)
        {
            warrior.isAbility2Unlocked = true;
            warrior.ImageSkill2.enabled = false;
        }
    }
}
