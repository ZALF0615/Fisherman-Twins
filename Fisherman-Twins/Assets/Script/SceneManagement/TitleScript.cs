using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour
{
    public AudioSource BGMPlayer;
    public AudioSource SEPlayer;



    private void Update()
    {
        if (Input.anyKeyDown)
        {
            SEPlayer.Play();
            GameManager.LoadScene(GameScene.MainLobby);
        }
    }
}
