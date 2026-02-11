using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


//Script para criar a lobby com as definições escolhidas
public class LobbyCreateUI : MonoBehaviour
{
    public static LobbyCreateUI Instance { get; private set; }

    [SerializeField] private Button createLobbyButton;

    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle[] privacyToggles;
    [SerializeField] private Toggle[] playerCountToggles;

    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;

    private void Awake()
    {
        Instance = this;

        // Desmarcar todos os toggles de privacidade
        foreach (Toggle toggle in privacyToggles)
        {
            toggle.isOn = false;
        }

        // Desmarcar todos os toggles de número de jogadores
        foreach (Toggle toggle in playerCountToggles)
        {
            toggle.isOn = false;
        }

        // Adiciona os listeners para os eventos de mudança nos toggles
        foreach (Toggle toggle in privacyToggles)
        {
            toggle.onValueChanged.AddListener(delegate { PrivacyToggleValueChanged(toggle); });
        }

        foreach (Toggle toggle in playerCountToggles)
        {
            toggle.onValueChanged.AddListener(delegate { PlayerCountToggleValueChanged(toggle); });
        }

        lobbyNameInputField.onValueChanged.AddListener(delegate { CheckButtonInteractable(); });
    }


    //Privacidade
    void PrivacyToggleValueChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            foreach (Toggle toggle in privacyToggles)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
            isPrivate = changedToggle == privacyToggles[0];
            Debug.LogWarning("is the lobby private: " + isPrivate);
            CheckButtonInteractable(); // Atualiza a interatividade do botão quando o toggle é alterado
        }
    }


    //PlayerCount
    void PlayerCountToggleValueChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            foreach (Toggle toggle in playerCountToggles)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
            maxPlayers = int.Parse(changedToggle.GetComponentInChildren<Text>().text); // Assuming text component holds player count
            Debug.LogWarning("max players: " + maxPlayers);
            CheckButtonInteractable(); // Atualiza a interatividade do botão quando o toggle é alterado
        }
    }

    void CheckButtonInteractable()
    {
        // Verifica se o campo de entrada do nome do lobby não está vazio (diferente do texto do placeholder)
        bool isNameFilled = !string.IsNullOrEmpty(lobbyNameInputField.text) && lobbyNameInputField.text != lobbyNameInputField.placeholder.GetComponent<TextMeshProUGUI>().text;

        // Verifica se pelo menos um toggle de privacidade está selecionado
        bool isPrivacySelected = false;
        foreach (Toggle toggle in privacyToggles)
        {
            if (toggle.isOn)
            {
                isPrivacySelected = true;
                break;
            }
        }

        // Verifica se pelo menos um toggle de número máximo de jogadores está selecionado
        bool isPlayerCountSelected = false;
        foreach (Toggle toggle in playerCountToggles)
        {
            if (toggle.isOn)
            {
                isPlayerCountSelected = true;
                break;
            }
        }

        // Atualiza a interatividade do botão com base nas condições
        createLobbyButton.interactable = isNameFilled && isPrivacySelected && isPlayerCountSelected;
    }


    public void CreateLobby()
    {
        if (createLobbyButton.interactable)
        {
            Debug.LogWarning("You can create the lobby " + lobbyNameInputField.text + " with isPrivate==" + isPrivate + " and with " + maxPlayers + " max players");

            MultiplayerLogic.Instance.CreateLobby(
                lobbyNameInputField.text,
                maxPlayers,
                isPrivate
            );
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

}