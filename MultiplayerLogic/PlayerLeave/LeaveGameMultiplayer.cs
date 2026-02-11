using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;

public class LeaveGameMultiplayer : MonoBehaviour
{
    public static string menuSceneName = "GameScene";
    public static string gameObjectName = "MainMenuUI";
    // Nova variável de controle para evitar chamadas repetidas
    private static bool isLeavingGame = false;


    //Método que permite sair de uma sessão de jogo Multiplayer
    public static void LeaveGame()
    {
        if (isLeavingGame) return; // Se o jogador já está a sair da sessão, não fazer nada
        isLeavingGame = true; // Defrinir a variável com o valor "true" para evitar chamadas recursivas

        if (NetworkManager.Singleton.IsServer)
        {
            // Se for o Host que está a tentar sair, o mesmo irá forçar todos os Clientes a sairem da sessão primeiro.
            BaseCharacter[] allCharacters = FindObjectsOfType<BaseCharacter>();
            //Iterar pela lista de todos os jogadores na sessão, e forçar todos (menos o próprio Host) a solicitarem a saída da sessão de jogo
            foreach (BaseCharacter character in allCharacters)
            {
                if (!character.IsOwner)
                {
                    character.RequestLeaveGame();
                }
            }

            // Esperar um pouco (2 segundos), de modo a garantir que todos os Clientes sejam capazes de processar a solicitação corretamente
            NetworkManager.Singleton.StartCoroutine(LeaveAfterDelay());
        }
        else
        {
            //Se for o Cliente a solicitar uma saída da sessão de jogo, executar o método de saída de uma sessão de jogo instantanamente
            PerformLeaveGame();
        }
    }

    //Corrotina responsável por aguardar algum tempo, até que todos os clientes tenham saído do jogo
    private static IEnumerator LeaveAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        PerformLeaveGame();
    }

    //Função que é, de facto, por fazer com que o jogador saia da sessão de jogo Multiplayer, independentemente de ser Host ou Cliente
    public static void PerformLeaveGame()
    {
        // Executar apenas se não estivermos saindo (evitar reentrada)
        if (!isLeavingGame) return;

        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            // Desconectar do servidor
            NetworkManager.Singleton.Shutdown();
        }

        // Desconectar autenticação anterior
        AuthenticationService.Instance.SignOut();

        // Resetar status do jogo
        BossSpawner.playerEnteredBossArea = false;
        Area2Spawner.playerEnteredArea2 = false;
        BasicEnemySpawner.playerEnteredArea1 = false;
        SubBossSpawner.playerEnteredSubBossArea = false;

        // Carregar a cena do menu inicial
        SceneManager.LoadScene(menuSceneName);

        isLeavingGame = false; // Resetar a variável de controle ao finalizar
    }

}
