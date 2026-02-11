using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Script para guardar as sprites das classes
public class LobbyAssets : MonoBehaviour {

    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite mageSprite;
    [SerializeField] private Sprite thiefSprite;
    [SerializeField] private Sprite warriorSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(MultiplayerLogic.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case MultiplayerLogic.PlayerCharacter.Mage:   return mageSprite;
            case MultiplayerLogic.PlayerCharacter.Thief:    return thiefSprite;
            case MultiplayerLogic.PlayerCharacter.Warrior:   return warriorSprite;
        }
    }

}