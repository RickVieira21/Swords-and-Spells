using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;


//Representa cada playerUI no contexto da lobby
public class LobbyPlayerSingleUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button kickPlayerButton;

    private Player player;


    private void Awake() {
        kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void SetKickPlayerButtonVisible(bool visible) {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    // Atualiza a template do jogador na lobby:
    // Mostra nome do jogador e imagem do seu character na lobby
    public void UpdatePlayer(Player player) {
        this.player = player;
        playerNameText.text = player.Data[MultiplayerLogic.KEY_PLAYER_NAME].Value;
        MultiplayerLogic.PlayerCharacter playerCharacter = 
            System.Enum.Parse<MultiplayerLogic.PlayerCharacter>(player.Data[MultiplayerLogic.KEY_PLAYER_CHARACTER].Value);
        characterImage.sprite = LobbyAssets.Instance.GetSprite(playerCharacter);
    }

    private void KickPlayer() {
        if (player != null) {
            MultiplayerLogic.Instance.KickPlayer(player.Id);
        }
    }


}