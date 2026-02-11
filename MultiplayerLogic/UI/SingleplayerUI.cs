using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


//Lógica para iniciar um jogo Singleplayer - Script tem de ser MonoBehaviour 
public class SingleplayerUI : MonoBehaviour
{
    private bool pickedMage = true;
    private bool pickedThief = false;
    private bool pickedWarrior = false;

    [SerializeField] private MageSingleplayer mage;
    [SerializeField] private ThiefSingleplayer thief;
    [SerializeField] private WarriorSingleplayer warrior;

    [SerializeField] private Button leaveLobbyButton;

    [SerializeField] private Button changeMageButton;
    [SerializeField] private Button changeThiefButton;
    [SerializeField] private Button changeWarriorButton;
    [SerializeField] private Sprite mageImage;
    [SerializeField] private Sprite thiefImage;
    [SerializeField] private Sprite warriorImage;
    [SerializeField] private Image playerImage;

    [SerializeField] private BasicEnemySpawner a1spawner;
    [SerializeField] private Area2Spawner a2spawner;
    [SerializeField] private SubBossSpawner a3spawner;
    [SerializeField] private BossSpawner a4spawner;

    [SerializeField] private SA1Spawner sa1spawner;
    [SerializeField] private SA2Spawner sa2spawner;
    [SerializeField] private SA3Spawner sa3spawner;
    [SerializeField] private SA4Spawner sa4spawner;

    [SerializeField] private FinalZone fzMultiplayer;
    [SerializeField] private FinalZoneSingleplayer fzSingleplayer;

    public AudioController audioController; //música menu inicial

    //Escolhe character e muda imagem 
    private void Awake()
    {
        changeMageButton.onClick.AddListener(() =>
        {
            pickedMage = true;
            pickedThief = false;
            pickedWarrior = false;
            playerImage.sprite = mageImage;
            Debug.LogWarning("picked mage");
        });


        changeThiefButton.onClick.AddListener(() =>
        {
            pickedMage = false;
            pickedThief = true;
            pickedWarrior = false;
            playerImage.sprite = thiefImage;
            Debug.LogWarning("picked thief");
        });

        changeWarriorButton.onClick.AddListener(() =>
        {
            pickedMage = false;
            pickedThief = false;
            pickedWarrior = true;
            playerImage.sprite = warriorImage;
            Debug.LogWarning("picked warrior");
        });

        playerImage.sprite = mageImage;
    }


    //Começa sessão de jogo
    public void StartSingleplayerGame()
    {
        if (pickedMage)
        {
            Instantiate(mage, new Vector3(0f, 21f, 0f), Quaternion.identity);
        }
        else if (pickedThief)
        {
            Instantiate(thief, new Vector3(0f, 21f, 0f), Quaternion.identity);
        }
        else if (pickedWarrior)
        {
            Instantiate(warrior, new Vector3(0, 21f, 0f), Quaternion.identity);
        }
        this.gameObject.SetActive(false);
        a1spawner.gameObject.SetActive(false);
        a2spawner.gameObject.SetActive(false);
        a3spawner.gameObject.SetActive(false);
        a4spawner.gameObject.SetActive(false);

        sa1spawner.gameObject.SetActive(true);
        sa2spawner.gameObject.SetActive(true);
        sa3spawner.gameObject.SetActive(true);
        sa4spawner.gameObject.SetActive(true);

        fzMultiplayer.gameObject.SetActive(false);
        fzSingleplayer.gameObject.SetActive(true);

        Debug.Log("Parar o audio do menu principal");
        audioController.StopAudio();
        audioController.DisableAudioListener();

    }

}
