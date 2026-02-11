using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Netcode;
using Cinemachine;


public class PlayerIndividualUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] public enum SINGLEPLAYER_CLASS { Mage, Thief, Warrior };
    public SINGLEPLAYER_CLASS className;


    public void UpdatePlayerName()
    {
        playerNameText.text = className.ToString();
    }


    void Start()
    {
        gameObject.SetActive(false);
        ActivateForLocalPlayer();
    }


    //Se for localPlayer - Mostra e atualiza a UI 
    public void ActivateForLocalPlayer()
    {
            UpdatePlayerName();
            gameObject.SetActive(true);
    }




}