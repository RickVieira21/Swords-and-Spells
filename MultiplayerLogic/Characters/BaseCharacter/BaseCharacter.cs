using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

//Script que representa uma personagem genérica
//possui todas as características de uma personagem genérica, tais como estatísticas (HP, velocidade,...), movimento, UI, entre outros.
public class BaseCharacter : NetworkBehaviour
{
    private MultiplayerLogic testLobby;

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

    private Mage mage;
    private Thief thief;
    private Warrior warrior;

    public bool isAlive = true;
    public HealthBar healthBar;

    private bool isPaused = false;
    private bool isHelping = false;
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
        testLobby = FindObjectOfType<MultiplayerLogic>();

        myAnimator = GetComponent<Animator>();
        playerInput = new MyPlayerInput();
        playerInput.Enable();

        GameObject gameManagerGO = GameObject.Find("GameManager");

        if (gameManagerGO != null)
        {
            gameManager = gameManagerGO.GetComponent<GameManager>();

            if (gameManager != null)
            {
                gameManager.allCharactersList.Add(this);
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

        //Definir o tipo de personagem ao qual este script está associado, e instanciar o objeto da respetiva Classe
        switch (characterType)
        {
            case CHARACTER_TYPE.Mage:
                mage = GetComponent<Mage>();
                break;
            case CHARACTER_TYPE.Thief:
                thief = GetComponent<Thief>();
                break;
            case CHARACTER_TYPE.Warrior:
                warrior = GetComponent<Warrior>();
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
        // Sincronizar vida quando o jogador é instanciado
        if (IsOwner)
        {
            listener.enabled = true;
            vc.Priority = 1;
        }
        else
        {
            vc.Priority = 0;
        }


        //Ativar menu de help localmente
        if(IsLocalPlayer && helpMenuUI != null)
        {
            helpMenuUI.SetActive(true);
            isHelping = true;
        }
    }

    //tratar dos inputs realizados por cada jogador constantemente
    private void Update()
    {  
        if (IsLocalPlayer)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePauseMenu();
            }
            
            if (isAlive)
            {
                if (Keyboard.current.hKey.wasPressedThisFrame)
                {
                    ToggleHelpMenu();
                }
                HandleInput();
            }
        }
    }

    // gestão de inputs que funcionam de modo semelhante em todas as personagens, tais como movimento e morte.
    private void HandleInput()
    {
        if (isAlive)
        {
            inputKey = new Vector3(-playerInput.Player.Movement.ReadValue<Vector2>().y, 0, playerInput.Player.Movement.ReadValue<Vector2>().x);

            if (health <= 0.0f)
            {
                if (IsServer)
                {
                    Die();
                    if(helpMenuUI.activeSelf == true && IsLocalPlayer)
                    {
                        helpMenuUI.SetActive(false);
                    }
                    deadMenuUI.SetActive(true);
                }
                else if (IsClient)
                {
                    DieServerRPC();
                    if (helpMenuUI.activeSelf == true && IsLocalPlayer)
                    {
                        helpMenuUI.SetActive(false);
                    }
                    deadMenuUI.SetActive(true);
                }
            }
            
            //verificar se o jogador está a premir algum botão utilizado para movimento
            if (inputKey.magnitude >= 0.1f && canMove)
            {
                if (IsServer)
                {
                    Move(inputKey);
                }
                else if (IsClient)
                {
                    MoveServerRPC(inputKey);
                }
            }
            else
            {
                if (IsServer)
                {
                    StopMoving();
                }
                else if (IsClient)
                {
                    StopMovingServerRPC();
                }
            }
        }
    }

    //Menu de pausa do jogo
    private void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (pauseMenuImage != null)
        {
            pauseMenuImage.SetActive(isPaused);
        }
    }

    //Menu de Ajuda do jogo
    private void ToggleHelpMenu()
    {
        isHelping = !isHelping;
        if (helpMenuUI != null)
        {
            helpMenuUI.SetActive(isHelping);
        }
    }

    //Exibir a UI de fim do jogo
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
        // Apenas permitir definir a vida se o jogador for o dono da instância
        if (IsOwner)
        {
            this.health = newHealth;
        }
    }

    //Regenerar vida (utilizado quando o jogador alcança um Checkpoint)
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
        RpcSetWalkingAnimationClientRPC(true);
        myAnimator.SetBool("walking", true);
        float angle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg;
        float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref currentVelocity, 0.1f);
        transform.rotation = Quaternion.Euler(0, smooth, 0);
    }

    [ServerRpc]
    private void MoveServerRPC(Vector3 input)
    {
        Move(input);
    }

    private void StopMoving()
    {
        RpcSetWalkingAnimationClientRPC(false);
        myAnimator.SetBool("walking", false);
    }

    [ServerRpc]
    private void StopMovingServerRPC()
    {
        StopMoving();
    }

    //Executar a animação de morrer, quando o jogador possui <= 0 HP
    public void Die()
    {
        DieClientRPC();
        myAnimator.SetBool("isAlive", false);
        isAlive = false;
        rb.velocity = Vector3.zero; // Impede qualquer movimento
        rb.angularVelocity = Vector3.zero; // Impede qualquer rotação
        playerInput.Disable(); // Desativa os inputs do jogador  
    }

    [ServerRpc]
    public void DieServerRPC()
    {
        Die();
    }

    // **********************************************************************
    // RPCs do lado do cliente
    // garantir que as animações acontecem do lado do cliente; sincronização entre Cliente e Host

    [ClientRpc]
    private void DieClientRPC()
    {
        myAnimator.SetBool("isAlive", false);
        isAlive = false;
        rb.velocity = Vector3.zero; // Impede qualquer movimento
        rb.angularVelocity = Vector3.zero; // Impede qualquer rotação
        playerInput.Disable(); // Desativa os inputs do jogador
    }

    [ClientRpc]
    private void RpcSetWalkingAnimationClientRPC(bool walking)
    {
        myAnimator.SetBool("walking", walking);
    }

    [ClientRpc]
    private void SyncHealthClientRPC(float newHealth)
    {
        // Sincronizar a vida nos clientes
        this.health = newHealth;
    }

    public void SetMovementSpeed(float newValue)
    {
        this.baseMovementSpeed = newValue;
    }

    //Método executado pelo Host, quando o mesmo deseja sair de uma sessão de jogo Multiplayer
    public void RequestLeaveGame()
    {
        if (IsServer)
        {
            LeaveGameClientRPC();
        }
    }

    // Método executado pelo Cliente, quando o mesmo deseja sair do jogo
    [ClientRpc]
    private void LeaveGameClientRPC()
    {
        LeaveGameMultiplayer.LeaveGame();
    }
}
