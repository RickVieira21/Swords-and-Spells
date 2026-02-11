using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Classe responsável por manter todas as características e estatísticas comuns a todas as classes de personagens (Modo Singleplayer)
public class BaseCharacterSingleplayer : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private AudioListener listener;
    [SerializeField] private GameObject pauseMenuImage;
    [SerializeField] private GameObject gameEndedUI;
    [SerializeField] private GameObject deadMenuUI;
    [SerializeField] private GameObject helpMenuUI;

    [SerializeField] private float health = 50f;
    [SerializeField] private float maxHealth = 50f;
    public float baseMovementSpeed = 5f;
    public float currentMovementSpeed = 5f;
    public float stealthMovementSpeed = 7f;
    public int level = 1;

    public Rigidbody rb;
    private MyPlayerInput playerInput;
    private Vector3 inputKey;
    private float currentVelocity;

    public Animator myAnimator;

    public enum CHARACTER_TYPE { Mage, Thief, Warrior };
    public CHARACTER_TYPE characterType;

    private MageSingleplayer mage;
    private ThiefSingleplayer thief;
    private WarriorSingleplayer warrior;

    public bool isAlive = true;
    public HealthBar healthBar;

    private bool isPaused = false;
    private bool canMove = true;

    private GameManager gameManager;


    public void SetCanMove(bool newValue)
    {
        this.canMove = newValue;
    }

    public GameManager GetGameManager()
    {
        return this.gameManager;
    }


    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        playerInput = new MyPlayerInput();
        playerInput.Enable();

        GameObject gameManagerGO = GameObject.Find("GameManager");

        if (gameManagerGO != null)
        {
            gameManager = gameManagerGO.GetComponent<GameManager>();


            if (gameManager != null)
            {
                gameManager.allSingleplayerCharactersList.Add(this);
            }
            else
            {
                Debug.LogWarning("GameManager not found");
            }

        }
        else
        {
            Debug.LogWarning("GameManager GameObject not found");
        }

        switch (characterType)
        {
            case CHARACTER_TYPE.Mage:
                mage = GetComponent<MageSingleplayer>();
                break;
            case CHARACTER_TYPE.Thief:
                thief = GetComponent<ThiefSingleplayer>();
                break;
            case CHARACTER_TYPE.Warrior:
                warrior = GetComponent<WarriorSingleplayer>();
                break;
        }
        myAnimator.SetBool("isAlive", true);
        SetHealth(health);
        myAnimator.SetFloat("health", health);
        healthBar.SetMaxHealth(health);

        if (pauseMenuImage != null)
        {
            pauseMenuImage.SetActive(false);
        }
    }

    private void Start()
    {
        listener.enabled = true;
        vc.Priority = 1;
    }

    //tratar dos inputs realizados por cada jogador constantemente
    private void Update()
    {
        if (isAlive)
        {
            HandleInput();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePauseMenu();
            }

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                ToggleHelpMenu();
            }
        }
    }

    // gestão de inputs
    private void HandleInput()
    {
        if (isAlive)
        {
            inputKey = new Vector3(-playerInput.Player.Movement.ReadValue<Vector2>().y, 0, playerInput.Player.Movement.ReadValue<Vector2>().x);
            if (health <= 0.0f)
            {
                Die();
                if (helpMenuUI.activeSelf == true)
                {
                    helpMenuUI.SetActive(false);
                }
                deadMenuUI.SetActive(true);
            }

            //verificar se o jogador está a premir algum botão utilizado para movimento
            if (inputKey.magnitude >= 0.1f && canMove)
            {
                Move(inputKey);
            }
            else
            {
                StopMoving();
            }
        }
    }

    //Menu de Pausa
    private void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (pauseMenuImage != null)
        {
            pauseMenuImage.SetActive(isPaused);
        }
    }

    //Menu de Ajuda

    private void ToggleHelpMenu()
    {
        if (helpMenuUI != null && !helpMenuUI.activeSelf)
        {
            helpMenuUI.SetActive(true);
        }
        else if (helpMenuUI != null && helpMenuUI.activeSelf)
        {
            helpMenuUI.SetActive(false);
        }
    }

    //Exibir o Menu de final de jogo
    public void ShowGameEndedMenu()
    {
        gameEndedUI.SetActive(true);
    }

    public float GetHealth()
    {
        return this.health;
    }

    public void SetHealth(float newHealth)
    {
        this.health = newHealth;
    }

    //Regenerar vida por segundo
    public void RegenerateHealth()
    {
        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        Debug.Log("Vida regenerada");
    }

    //Movimento
    private void Move(Vector3 input)
    {
        rb.MovePosition(transform.position + currentMovementSpeed * Time.deltaTime * input);
        myAnimator.SetBool("walking", true);
        float angle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg;
        // Check if L Key is held
        if (Keyboard.current.lKey.isPressed)
        {
            // Rotate to look in the opposite direction
            angle += 180f;
        }
        float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref currentVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0, smooth, 0);
    }


    private void StopMoving()
    {
        myAnimator.SetBool("walking", false);
    }

    //Fazer com que o jogador morra, ativando uma animação de morte e desabilitando os seus controlos quando a sua vida é igual ou menor a 0
    public void Die()
    {
        myAnimator.SetBool("isAlive", false);
        isAlive = false;
        rb.velocity = Vector3.zero; // Impede qualquer movimento
        rb.angularVelocity = Vector3.zero; // Impede qualquer rotação
        playerInput.Disable(); // Desativa os inputs do jogador
    }

    public void SetMovementSpeed(float newValue)
    {
        this.baseMovementSpeed = newValue;
    }

    //Método executado quando o jogador decide sair de uma sessão de jogo Singleplayer
    public void LeaveSingleplayerGame()
    {
        SA1Spawner.playerEnteredArea1 = false;
        SA2Spawner.playerEnteredArea2 = false;
        SA3Spawner.playerEnteredSubBossArea = false;
        SA4Spawner.playerEnteredBossArea = false;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}
