using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdventureLobbyScript : MonoBehaviour
{
    public void BackToMainLobby()
    {
        SceneManager.LoadScene("MainLobbyScene");
    }
}
