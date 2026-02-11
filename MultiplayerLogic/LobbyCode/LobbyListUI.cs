using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


//Representa a lista de lobbies que deve ser constantemente atualizada
public class LobbyListUI : MonoBehaviour {

    public static LobbyListUI Instance { get; private set; }

    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;


    private void Awake() {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);  

        refreshButton.onClick.AddListener(RefreshButtonClick);
    }

    private void Start() {
        MultiplayerLogic.Instance.OnLobbyListChanged += TestLobby_OnLobbyListChanged;
        MultiplayerLogic.Instance.OnJoinedLobby += TestLobby_OnJoinedLobby;
        MultiplayerLogic.Instance.OnLeftLobby += TestLobby_OnLeftLobby;
        MultiplayerLogic.Instance.OnKickedFromLobby += TestLobby_OnKickedFromLobby;
    }

    private void TestLobby_OnKickedFromLobby(object sender, MultiplayerLogic.LobbyEventArgs e) {
        Show();
    }

    private void TestLobby_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void TestLobby_OnJoinedLobby(object sender, MultiplayerLogic.LobbyEventArgs e) {
        Hide();
    }


    //Quando a lobbyList muda, tem de fazer update da lista
    private void TestLobby_OnLobbyListChanged(object sender, MultiplayerLogic.OnLobbyListChangedEventArgs e) {
        Debug.Log("TestLobby_OnLobbyListChanged");
        UpdateLobbyList(e.lobbyList);
        Show(); 
    }


    //Método para fazer update da lista de lobbies - destrói cada single template antiga e cria as novas necessárias
    private void UpdateLobbyList(List<Lobby> lobbyList) {

      int numberOfLobbies = lobbyList.Count;
      Debug.Log("Número de lobbies na lista: " + numberOfLobbies);

        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
           // Debug.Log("UpdateLobbyList: Terminado");
        }
    }


    private void RefreshButtonClick() {
        MultiplayerLogic.Instance.RefreshLobbyList();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}