using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class GameController : MonoBehaviour
{

    #region INSTANCE
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

    public PlayerController player;
    public RiverGenerator riverGenerator;

    public AudioSource BGM;
    public AudioSource SE;

    #endregion PARAM

    #region ADVENTURE_MODE

    public bool isAdventureMode;
    public GameObject adventureObj;

    #endregion ADVENTURE_MODE

    public void GameStart()
    {
        isGameOngoing = true;

        uiScript.SetUIActive(true);
        player.GameStart();
    }

    // 霸烙 柳青 包访 贸府
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
        if(GameManager.currentScene == GameScene.GameScene_Adventure)
        {
            isAdventureMode = true;

            uiScript.SetUIActive(false);
            adventureObj.gameObject.SetActive(true);

            adventureObj.GetComponent<AdventureModeManager>().Init();
        }
        else
        {
            Destroy(adventureObj);
            GameStart();
        }
    }
}
