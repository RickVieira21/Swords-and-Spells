using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//Classe que representa o gestor da ligação do Cliente a uma alocação Relay
//Associado ao GameObject "ClientConnectionHandler", na cena "GameScene"

public class ClientConnectionHandler : NetworkBehaviour
{
    //Lista com todos os Prefabs com os quais o Cliente pode "spawnar" numa sessão de jogo Multiplayer
    public PlayerPrefabList AlternatePlayerPrefabs;

    //Definição do Prefab (Classe de Personagem) com o qual o Cliente irá jogar, numa sessão de jogo Multiplayer
    public void SetClientPlayerPrefab(int index)
    {
        //Verificar se o valor de índice introduzido excede o número de elementos presentes na lista "AlternatePlayerPrefabs"
        if (index >= AlternatePlayerPrefabs.alternatePlayerPrefabHashes.Count)
        {
            Debug.LogError($"Trying to assign player Prefab index of {index} when there are only {AlternatePlayerPrefabs.alternatePlayerPrefabHashes.Count} entries!");
            return;
        }
        if (NetworkManager.IsListening || IsSpawned)
        {
            Debug.LogError("This needs to be set before connecting!");
            return;
        }
        NetworkManager.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(index);
    }

    //Método executado quando um determinado Network Object é spawnado numa sessão Multiplayer
    public override void OnNetworkSpawn()
    {
        //Se a máquina atual apresentar características de um servidor(Servidor/Host), definir o "Connection Approval Callback" do Network Manager do jogo
        if (IsServer)
        {
            NetworkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
        }
    }

    //Método responsável por Spawnar o Cliente numa sessão de jogo Multiplayer
    //Este método é evocado pelo cliente quando uma sessão de jogo é iniciada, onde o mesmo indica o índice do Player Prefab, armazenado na lista "AlternatePlayerPrefabs",
    //que o mesmo deseja utilizar durante o jogo.
    public void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Lógica de aprovação
        response.Approved = true; //quando "response.Approved = true", a conexão do cliente foi aprovada
        response.CreatePlayerObject = true; //quando = true, o Servidor/Host irá spawnar um Player Prefab, e associá-lo ao Cliente que está a conectar-se à sessão de jogo.

        var playerPrefabIndex = System.BitConverter.ToInt32(request.Payload);
        //Se o índice de Player Prefab selecionado pelo Cliente existe, definir o valor "Hash" do Player Prefab que será associado ao Cliente como o valor que se encontra
        //no índice "playerPrefabIndex", na lista AlternatePlayerPrefabs, que mantém uma lista com todos os valores Hash de todos os Player Prefabs que podem representar o Cliente 
        if (playerPrefabIndex >= 0 && playerPrefabIndex < AlternatePlayerPrefabs.alternatePlayerPrefabHashes.Count)
        {
            response.PlayerPrefabHash = AlternatePlayerPrefabs.alternatePlayerPrefabHashes[playerPrefabIndex];
        }
        else
        {
            Debug.LogError($"Client provided player Prefab index of {playerPrefabIndex} when there are only {AlternatePlayerPrefabs.alternatePlayerPrefabHashes.Count} entries!");
            return;
        }

        // Posição para spawnar o Player Prefab que representa o Cliente
        response.Position = new Vector3(0f, 21f, 0f);//AREA1 0, 21, 0 //AREA2 30f, 21f, 100f //SUB-BOSS - 15f, 21f, 226f //BOSS 10f, 21f, 440f
        response.Rotation = Quaternion.identity; // Fazer "reset" da rotação, para garantir que o Host spawna com a rotação e orientação padrão

        //Mensagem que explica a razão de "response.Approved" ser igual a "false", caso aconteça
        response.Reason = "Some reason for not approving the client";

        //Deve ser igual a 'true' se existirem passos adicionais durante a aprovação.
        //Assim que for igual a 'false', a resposta da aprovação da ligação à sessão de jogo é imediatamente processada.
        response.Pending = false;
    }
}