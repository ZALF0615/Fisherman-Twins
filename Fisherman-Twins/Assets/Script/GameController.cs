using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class GameController : MonoBehaviour
{

    #region INSTANCE
    public PlayerController playerController;
    public UIScript uiScript;
    static GameController instance;
    public static GameController GetInstance()
    {
        if (instance == null)
        {
            return null;
        }

        return instance;
    }
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion INSTANCE
    
    #region PARAM

    public bool isGameOngoing;

    public GameObject player;

    public AudioSource BGM;
    public AudioSource SE;

    #endregion PARAM

    // 게임 진행 관련 처리
    public void GameOver()
    {
        isGameOngoing = false;
        uiScript.GameOver();
    }



    public void PlaySE(AudioClip clip)
    {
        SE.PlayOneShot(clip);
    }


    void Start()
    {
        isGameOngoing = true;
    }
}
