using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbyScript : MonoBehaviour
{
    public void AdventureMode()
    {
        GameManager.LoadScene(GameScene.AdventureLobby);
    }

}
