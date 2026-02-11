using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Netcode;
using Cinemachine;

//Script que vai buscar as informações do player para serem usadas na UI
public class PlayerUI : NetworkBehaviour {

     public MultiplayerLogic testLobby; // Script que contém o nome e classe do jogador
    [SerializeField] private TextMeshProUGUI playerNameText; 


    public void UpdatePlayerName()
    {      
        string playerName = MultiplayerLogic.Instance.playerName;
        playerNameText.text = playerName;
    }

    
    void Start()
    {
       gameObject.SetActive(false);
       ActivateForLocalPlayer();
    }


    //Se for localPlayer - Mostra e atualiza a UI 
    public void ActivateForLocalPlayer()
    {
        
        if (IsLocalPlayer)
        {
            UpdatePlayerName(); 
            gameObject.SetActive(true); 
        }
    }




}