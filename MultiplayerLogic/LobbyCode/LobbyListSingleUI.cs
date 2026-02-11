using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;


//Representa uma SingleUI na lista de lobbies
public class LobbyListSingleUI : MonoBehaviour {
    
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;


    private Lobby lobby;


    //Botão de Join
    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            MultiplayerLogic.Instance.JoinPublicLobby(lobby);
        });
    }


    //Info. da lobby (nome e players)
    public void UpdateLobby(Lobby lobby) {
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }


}