using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region INSTANCE
    public static GameManager instance;
    public static GameManager GetInstance()
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

        DontDestroyOnLoad(gameObject);
    }
    #endregion INSTANCE

    public static GameScene currentScene;
    public static int currentStageIdx;

    public static void LoadScene(GameScene scene)
    {
        string sceneName = string.Empty;

        switch (scene)
        {
            case GameScene.Title:
                sceneName = "TitleScene";
                break;
            case GameScene.MainLobby:
                sceneName = "MainLobbyScene";
                break;
            case GameScene.AdventureLobby:
                sceneName = "AdventureLobbyScene";
                break;
            case GameScene.GameScene_Adventure:
                sceneName = "GameScene";
                break;
            case GameScene.GameScene_Infinity:
                sceneName = "GameScene";
                break;
        }

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch
        {
            Debug.LogError("정의되지 않은 게임 신입니다.");
        }

    }

}

public enum GameScene
{
    Title,
    MainLobby,
    AdventureLobby,
    GameScene_Adventure,
    GameScene_Infinity,
}