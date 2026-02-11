using UnityEngine;

//Script utilizado para executar a função "LeaveGame", chamada pelo botão "Exit" contido no Menu de Pausa
//Permite sair de uma sessão de jogo Multiplayer
public class LeaveGamePlayer : MonoBehaviour
{
    public void OnLeaveGameButtonClicked()
    {
        LeaveGameMultiplayer.LeaveGame();
    }
}
