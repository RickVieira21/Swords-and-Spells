using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;

public class MultiplayerLogic : MonoBehaviour
{
    public static MultiplayerLogic Instance { get; private set; }

    public Lobby hostLobby; //lobby criado pelo jogador
    public Lobby joinedLobby; //armazena o lobby ao qual o jogador se juntou

    private float refreshLobbyListTimer = 5f; //Temporizador para atualizar a lista de lobbies automaticamente 
    private float lobbyUpdateTimer;  //Temporizador para enviar updates ao Lobby, e atualizar as suas informaçoes para todas os jogadores.
    private float heartbeatTimer; //Intervalo de segundos em que informacao keepalive é enviada para o lobby

    public string playerName; //nome do jogador

    bool startedGame;  //indica o estado do jogo
    public GameObject startGameButton;
    public GameObject multiplayerMenuPanel;
    public GameObject lobbyPanel;
    public TMPro.TMP_InputField lobbyCodeInputField;

    //texto no menu do lobby para exibir o Lobby Code e o seu ID do lobby
    public TMPro.TextMeshProUGUI lobbyCodeText;
    public TMPro.TextMeshProUGUI lobbyIdText;

    public ClientConnectionHandler clientConnectionHandler; //tratar da conexao por parte dos clientes
    public PlayerPrefabList AlternatePlayerPrefabs; //prefabs de todos os tipos de jogadores/classes

    public AudioController audioController; //música menu inicial

    public enum PlayerCharacter
    {
        Mage,
        Thief,
        Warrior
    }
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_CHARACTER = "Character";

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        //Inicializar os servi�os do Unity (função assíncrona)
        await UnityServices.InitializeAsync();

        //Verificar/validar que o jogador, de facto, fez "sign in".
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Jogador registou-se(signed in), Player ID = " + AuthenticationService.Instance.PlayerId);
        };

        //Registo anónimo automático
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //Atribuir nome aleatório ao jogador
        playerName = "Jogador" + UnityEngine.Random.Range(1, 1000);
        Debug.Log("Nome do jogador: " + playerName);

        await Task.Delay(1000);
    }


    // ****************************************************************** LOBBY ************************************************************************

    //permite ao jogador juntar-se a um lobby privado, atraves do Lobby Code
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            //Obter o Lobby ao qual o jogador se juntou
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Juntou-se ao Lobby com o c�digo " + lobbyCode);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //listar/filtrar os lobbies ativos
    public async void ListLobbies()
    {
        try
        {
            //opcoes de filtragem dos lobbies
            QueryLobbiesOptions queryLobbiesOptions
                 = new QueryLobbiesOptions
                 {
                     Count = 25,
                         //lista de filtros
                         Filters = new List<QueryFilter>
                     {
                             //obter todos os lobbies com uma quantidade maior do que 0 jogadores (excetuando 0)
                             new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                     },
                         //organizar os resultados
                         Order = new List<QueryOrder>
                     {
                             //Ordenar por ordem ascendente da idade em que os lobbies foram criados (os mais novos aparecem primeiro)
                             new QueryOrder(false, QueryOrder.FieldOptions.Created)
                     }
                 };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies encontrados: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Nome do Lobby: " + lobby.Name + "; N�mero m�ximo de jogadores: " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //enviar heartbeats para manter o lobby vivo (keepalive) a cada X segundos, para garantir que o Lobby continua ativo
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby == null)
            return;

        //se o host do lobby existir, comecar o contador
        heartbeatTimer -= Time.deltaTime;
        //quando chegar a 0,
        if (heartbeatTimer < 0f)
        {
            float heartbeatTimerMax = 10f;
            //resetar o contador
            heartbeatTimer = heartbeatTimerMax;
            //enviar informa��o para o Lobby de 10 em 10 segundos
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            Debug.Log("Lobby atualizado");
            PrintPlayers();
        }
    }

    //realizar solicitações de atualizações das informações do Lobby (nº jogadores, classe de cada jogador, ...) num determinado intervalo de tempo
    //limite mínimo de 1 vez por segundo
    private async void HandleLobbyPollForUpdates()
    {

        // Return imediato se o jogador não estiver num Lobby ou o jogo ainda não tiver começado.
        if (joinedLobby == null || startedGame)
        {
            return;
        }
        bool isHost = IsLobbyHost();
        if (isHost)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyPollTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyPollTimerMax;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                // Jogador foi kickado do Lobby
                if (!IsPlayerInLobby())
                {
                    Debug.Log("Kicked from Lobby!");
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                    joinedLobby = null;
                }
            }
            // Se o jogo já começou, o jogador encontra-se no Lobby, e não é um Host, juntar-se à sessão de jogo (Relay) criada pelo Host
            if (joinedLobby != null && joinedLobby.Data["StartGame"].Value != "0")
            {
                if (hostLobby == null)
                {
                    UpdatedJoinRelay(joinedLobby.Data["StartGame"].Value);
                }
                startedGame = true;
            }
        }
    }

    // Atualizar a lista de lobbies automaticamente passado x tempo (refresh automático)
    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f)
            {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;
                RefreshLobbyList();
            }
        }
    }

    //Indica se o jogador está num Lobby
    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Exibir informações de todos os jogadores que estão no Lobby (nome, ID, etc...)
    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name + ":");
        foreach (Player player in lobby.Players)
        {
            //Informação obtida através do Dicionário <string, PlayerDataObject> com informação genérica, designado por Data, que permite definir e obter informações personalizadas
            Debug.Log("ID= " + player.Id + "; nome= " + player.Data[KEY_PLAYER_NAME].Value + ", character= " + player.Data[KEY_PLAYER_CHARACTER].Value);
        }
    }

    //Outra vers�o do m�todo PrintPlayers que permite obter as informa��es do Lobby ao qual o jogador se juntou.
    public void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    //Obter as informações do jogador
    private Player GetPlayer()
    {
        return new Player
        {
            //Definir informações personalizadas em cada jogador através do dicionário genérico "Data" contido na classe Player
            Data = new Dictionary<string, PlayerDataObject>
                        {
                            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                            { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerCharacter.Mage.ToString()) }
                        }
        };
    }

    //Atualizar o nome do jogador no Lobby
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                    {
                        {KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
                    }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //Atualizar a Imagem que representa a classe do jogador, no Lobby
    public async void UpdatePlayerCharacter(PlayerCharacter playerCharacter)
    {
        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions();
                options.Data = new Dictionary<string, PlayerDataObject>() {
                        {
                            KEY_PLAYER_CHARACTER, new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: playerCharacter.ToString())
                        }
                    };

                string playerId = AuthenticationService.Instance.PlayerId;
                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    //Sair do Lobby, se o jogador estiver, de facto, num Lobby.
    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            bool wasHost = IsLobbyHost();
            if (wasHost)
            {
                // Delegar um novo jogador para Host antes de sair do Lobby, se este jogador for Host
                bool success = await PromoteNewHost();
                if (!success)
                {
                    Debug.LogError("Falha ao promover um novo host");
                    return;
                }
            }
            /*
            //Se o Jogador tentar sair do Lobby enquanto Host, e for o único jogador do Lobby, o mesmo deve ser destruído
            if (hostLobby != null && joinedLobby.Players.Count <= 1)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                    await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                    Debug.LogWarning("Lobby " + joinedLobby.Name + " Destruído. ");
                    hostLobby = null;
                    joinedLobby = null;
                    OnLeftLobby?.Invoke(this, EventArgs.Empty);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
            */
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    //Promover um novo Host, quando o Host atual sai do Lobby
    private async Task<bool> PromoteNewHost()
    {
        if (joinedLobby == null || joinedLobby.Players == null || joinedLobby.Players.Count == 0)
        {
            return false;
        }
        // Seleciona o próximo jogador para ser o novo Host
        var newHost = joinedLobby.Players.FirstOrDefault(player => player.Id != AuthenticationService.Instance.PlayerId);
        //Caso exista pelo menos um jogador no Lobby que não seja o Host atual, indicá-lo como o novo Host
        if (newHost != null)
        {
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    HostId = newHost.Id
                });
                Debug.Log("Novo host promovido: " + newHost.Id);
                hostLobby = null;
                //joinedLobby = null;
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                return false;
            }
        }
        //Caso apenas exista um jogador (o próprio Host), não será promovido um novo Host, e o Lobby será destruído.
        else if (newHost == null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                Debug.LogWarning("Lobby " + joinedLobby.Name + " Destruído. ");
                hostLobby = null;
                //joinedLobby = null;
                return false;
                //OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        return false;
    }

    private void Update()
    {
        HandleRefreshLobbyList();
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    // ********************************************** Funções do Menu do Lobby ************************************************
    public event EventHandler OnLeftLobby; //Evento Trigger quando se sai do Lobby
    public event EventHandler<LobbyEventArgs> OnJoinedLobby; //Evento Trigger quando se cria ou se sai de um Lobby
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate; //Evento Trigger quando se está num Lobby
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby; //Evento Trigger quando se é "kickado" de um Lobby

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    //Criar um Lobby, com as settings definidas pelo host
    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        try
        {
            Player player = GetPlayer();
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject> { { "StartGame", new DataObject(DataObject.VisibilityOptions.Member, "0") } }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            Debug.Log("Created Lobby " + lobby.Name);
            Debug.Log("Lobby Code: " + lobby.LobbyCode);
            Debug.Log("Lobby ID: " + lobby.Id);

            lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;
            lobbyIdText.text = "Lobby ID: " + lobby.Id;

            PrintPlayers();
            startGameButton.SetActive(true);
        }
        catch (LobbyServiceException lse)
        {
            Debug.Log(lse);
        }
    }

    //Atualizar a lista de Lobbies existentes
    public async void RefreshLobbyList()
    {
        //ListLobbies();
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Aplicar filtros apenas para Lobbies com espaços livres
            options.Filters = new List<QueryFilter> {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0")
                };

            // Ordenar por Lobbies mais recentes primeiro
            options.Order = new List<QueryOrder> {
                    new QueryOrder(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created)
                };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //Juntar a um Lobby público presente na lista de Lobbies, através do seu Lobby ID
    public async void JoinPublicLobby(Lobby lobby)
    {
        Debug.Log("Join Public Lobby by ID");
        Player player = GetPlayer();
        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions { Player = player });
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        Debug.Log("Entrei no lobby público" + lobby.Name + ", com o c�digo " + lobby.LobbyCode);
    }

    //Juntar a um lobby privado presente na lista de Lobbies, através do seu Lobby Code
    public async void JoinPrivateLobby()
    {
        try
        {
            Player player = GetPlayer();
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCodeInputField.text, new JoinLobbyByCodeOptions
            {
                Player = player
            });
            joinedLobby = lobby;
            multiplayerMenuPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            Debug.Log("Entrei no lobby privado" + lobby.Name + ", com o c�digo " + lobby.LobbyCode);
            PrintPlayers();
        }
        catch (LobbyServiceException lse)
        {
            Debug.Log(lse);
        }
        //OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    //Se o jogador for o Host do Lobby, é capaz de kickar outros jogadores
    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    // *********************************************** RELAY **********************************************************

    //Criação de uma Alocação Relay (Sessão de Jogo)
    public async Task<string> UpdatedCreateRelay()
    {
        string joinCode = "";
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            //Geração de um "Join Code", para que os outros jogadores sejam capazes de se juntar a esta Alocação.
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls"); //pode ser DTLS ou UDP
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck; //Lógica da inicialização do Host na sessão de jogo
            Debug.LogWarning("VOU COMEÇAR O JOGO");
            NetworkManager.Singleton.StartHost(); //Inicialização da sessão de jogo como Host

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            Debug.Log("Erro na criação do Relay.");
        }
        Debug.Log("Parar o audio do menu principal");
        audioController.StopAudio();
        audioController.DisableAudioListener();
        return joinCode;
    }

    //Juntar a uma Alocação Relay criada por um Host
    async void UpdatedJoinRelay(string joinCode)
    {
        Debug.Log("joinCode: " + joinCode);
        try
        {
            Debug.Log("A juntar-se a um relay com o c�digo " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = clientConnectionHandler.ConnectionApprovalCallback; //Lógica da inicialização do cliente na sessão de jogo
            NetworkManager.Singleton.StartClient(); //Inicialização da sessão de jogo como Cliente
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            Debug.Log("Erro durante a ligação ao Relay com o Join Code indicado. ");
        }

        lobbyPanel.SetActive(false);
        Debug.Log("Parar o audio do menu principal");
        audioController.StopAudio();
        audioController.DisableAudioListener();
    }

    //Método chamado pelo botão de Start Game presente no Lobby
    public async void UpdatedStartGame()
    {
        string relayCode = await UpdatedCreateRelay(); //Criação de uma Alocação Relay
                                                       //Atualizar as informações do Lobby, indicando que a sessão de jogo já foi inicializada
        Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        { Data = new Dictionary<string, DataObject> { { "StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode) } } });
        joinedLobby = lobby;
        lobbyPanel.SetActive(false);
    }

    // ***************************** Gestão da Introdução do Host na Sessão de Jogo ***********************

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Identificador de cliente a ser autenticado
        var clientId = request.ClientNetworkId;
        // Informação de ligação adicional personalizada
        var connectionData = request.Payload;
        response.Approved = true;
        response.CreatePlayerObject = true;

        var playerPrefabIndex = System.BitConverter.ToInt32(request.Payload);
        if (playerPrefabIndex >= 0 && playerPrefabIndex < AlternatePlayerPrefabs.alternatePlayerPrefabHashes.Count)
        {
            response.PlayerPrefabHash = AlternatePlayerPrefabs.alternatePlayerPrefabHashes[playerPrefabIndex];
        }
        else
        {
            Debug.LogError($"Client provided player Prefab index of {playerPrefabIndex} when there are only {AlternatePlayerPrefabs.alternatePlayerPrefabHashes.Count} entries!");
            return;
        }
        // Posição para spawnar o Player Prefab que representa o Host
        response.Position = new Vector3(0f, 21f, 0f); //AREA1 0, 21, 0 //AREA2 30f, 21f, 100f //SUB-BOSS - 15f, 21f, 226f //BOSS 10f, 21f, 440f
                                                     
        response.Rotation = Quaternion.identity;  // Fazer "reset" da rotação, para garantir que o Host spawna com a rotação e orientação padrão
        //Mensagem que explica a razão de "response.Approved" ser igual a "false", caso aconteça
        response.Reason = "Some reason for not approving the client";
        //Deve ser igual a 'true' se existirem passos adicionais durante a aprovação.
        //Assim que for igual a 'false', a resposta da aprovação da ligação à sessão de jogo é imediatamente processada.
        response.Pending = false;
    }
}