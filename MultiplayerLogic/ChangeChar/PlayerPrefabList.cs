using System.Collections.Generic;
using UnityEngine;

//Classe responsável por manter uma lista de valores inteiros "unsigned" que representam os valores GlobalObjectIdHash de todos os Prefabs que herdam de NetworkBehaviour,
//ou seja, todos os Prefabs que podem ser atribuídos ao Host/Clientes numma sessão de jogo Multiplayer, designados por "Player Prefabs"
//Estes valores encontram-se definidos em todos os Prefabs que possuem um Componente "Network Object", mais concretamente o valor do atributo "GlobalObjectIdHash" desse mesmo Componente.

[CreateAssetMenu(fileName = "PlayerPrefabList", menuName = "Player Prefab List", order = 1)]
public class PlayerPrefabList : ScriptableObject
{
    public List<uint> alternatePlayerPrefabHashes;
}
