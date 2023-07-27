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

    bool isGameOngoing;

    public GameObject player;

    public AudioSource BGM;
    public AudioSource SE;

    #endregion PARAM

    // 霸烙 柳青 包访 贸府
    public void GameOver()
    {
        isGameOngoing = false;
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
