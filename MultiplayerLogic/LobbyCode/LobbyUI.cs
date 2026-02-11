using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

//Usado dentro da própria lobby para escolher classe e atualizar as informações
public class LobbyUI : MonoBehaviour {

    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;

    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button changeMageButton;
    [SerializeField] private Button changeThiefButton;
    [SerializeField] private Button changeWarriorButton;

    [SerializeField] private ClientConnectionHandler characterHandler;

    private void Awake() {
        Instance = this;

        //Default character (mage)
        characterHandler.SetClientPlayerPrefab(0);

        playerSingleTemplate.gameObject.SetActive(false);

        leaveLobbyButton.onClick.AddListener(() => {
            MultiplayerLogic.Instance.LeaveLobby();
        });


        changeMageButton.onClick.AddListener(() =>
        {
            characterHandler.SetClientPlayerPrefab(0);
            MultiplayerLogic.Instance.UpdatePlayerCharacter(MultiplayerLogic.PlayerCharacter.Mage);
            //Debug.Log("changed to mage");
        });


        changeThiefButton.onClick.AddListener(() =>
        {
            characterHandler.SetClientPlayerPrefab(1);
            MultiplayerLogic.Instance.UpdatePlayerCharacter(MultiplayerLogic.PlayerCharacter.Thief);
            //Debug.Log("changed to thief");
        });

        changeWarriorButton.onClick.AddListener(() =>
        {
            characterHandler.SetClientPlayerPrefab(2);
            MultiplayerLogic.Instance.UpdatePlayerCharacter(MultiplayerLogic.PlayerCharacter.Warrior);
            //Debug.Log("changed to warrior");
        });

    }

    private void Start() {
        MultiplayerLogic.Instance.OnJoinedLobby += UpdateLobby_Event;
        MultiplayerLogic.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        MultiplayerLogic.Instance.OnLeftLobby += TestLobby_OnLeftLobby;
        MultiplayerLogic.Instance.OnKickedFromLobby += TestLobby_OnLeftLobby;
        //    Hide();
    }

    private void TestLobby_OnLeftLobby(object sender, System.EventArgs e) {
        ClearLobby();
     //   Hide();
    }

    private void UpdateLobby_Event(object sender, MultiplayerLogic.LobbyEventArgs e) {
        UpdateLobby();
    }

    private void UpdateLobby() {
        UpdateLobby(MultiplayerLogic.Instance.GetJoinedLobby());
    }


    // Método para atualizar o lobby:
    // 1 - Limpa player templates antigas
    // 2 - Cria novas templates para novos players
    // 3 - Atualiza info da lobby
    private void UpdateLobby(Lobby lobby) {
        ClearLobby();

        foreach (Player player in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                MultiplayerLogic.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        
        Debug.Log("lobbyname: " + lobbyNameText.text);
        Debug.Log("playerCount: " + playerCountText.text);

        Show();
    }

    //Limpa player single templates antigas
    private void ClearLobby() {
        foreach (Transform child in container) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}