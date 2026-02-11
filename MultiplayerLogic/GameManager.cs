using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script responsável por representar o GameManager
//No âmbito do projeto desenvolvido, o GameManager é responsável por manter listas com todos os tipos de inimigos desenvolvidos, assim como
//referências a todos os Spawners de inimigos, tanto em modo Singleplayer como em modo Multiplayer
public class GameManager : MonoBehaviour
{
    //Lista com todas as personagens presentes numa sessão de jogo Multiplayer
    public List<BaseCharacter> allCharactersList = new List<BaseCharacter>();
    //Lista que armazena o personagem do jogador, numa sessão de jogo Singleplayer
    public List<BaseCharacterSingleplayer> allSingleplayerCharactersList = new List<BaseCharacterSingleplayer>();

    //Spawners presentes no Modo Multiplayer
    [SerializeField] private BasicEnemySpawner basicEnemySpawner;
    [SerializeField] private Area2Spawner ghoulSpawner;
    [SerializeField] private SubBossSpawner subBossSpawner;
    [SerializeField] private BossSpawner bossSpawner;

    //Listas que armazenam todos os inimigos instanciados no modo Multiplayer
    public List<Skeleton> skeletonList;
    public List<Ghoul> ghoulList;
    public List<SubBoss> subBossList;
    public List<Boss> bossList;

    //Spawners presentes no Modo Singleplayer
    [SerializeField] private SA1Spawner spawnerArea1;
    [SerializeField] private SA2Spawner spawnerArea2;
    [SerializeField] private SA3Spawner spawnerArea3;
    [SerializeField] private SA4Spawner spawnerArea4;

    //Listas que armazenam todos os inimigos instanciados no modo Singleplayer
    public List<SkeletonSingleplayer> sList;
    public List<GhoulSingleplayer> gList;
    public List<SubBossSingleplayer> sbList;
    public List<BossSingleplayer> bList;

}
