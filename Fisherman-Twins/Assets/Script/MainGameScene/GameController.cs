/*
 * GameController.cs
 * 
 * 물고기를 잡는 메인 게임 씬의 로직 전체를 관리
 * 게임 시작 및 종료 처리, 사운드 이펙트 재생 기능
 * 어드벤처 모드의 활성화 및 비활성화 등
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class GameController : MonoBehaviour
{

    #region INSTANCE

    static GameController instance; // 게임 컨트롤러(자기 자신) 인스턴스
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
    public bool isGameOngoing; // 게임이 진행 중인지 여부

    public UIManager uiScript;
    public PlayerController player;
    public ItemController itemController;
    public TileGenerator tileGenerator;
    public FishData fishData;

    public AudioSource BGM;
    public AudioSource SE;

    #endregion PARAM

    #region ADVENTURE_MODE

    public bool isAdventureMode; // 현재 모드가 어드벤처 모드인지 여부
    public GameObject adventureObj; // 어드벤처 모드 오브젝트(어드벤처 모드에만 사용되는 요소를 모아둔 오브젝트)

    #endregion ADVENTURE_MODE

    // 게임 시작
    public void GameStart()
    {
        isGameOngoing = true; // 게임이 진행 중임을 표시
        uiScript.SetUIActive(true); // UI를 활성화 
        player.GameStart(); // 플레이어 컨트롤러에 게임 시작 신호
    }

    // 게임 종료
    public void GameOver()
    {
        isGameOngoing = false; // 게임이 진행 중이 아님을 표시
        uiScript.GameOver();
    }

    // 효과음 재생
    public void PlaySE(AudioClip clip)
    {
        SE.PlayOneShot(clip);
    }

    void Start()
    {
        if (GameManager.currentScene == GameScene.GameScene_Adventure) 
        {
            // 어드벤처 모드

            isAdventureMode = true; // 어드벤처 모드임을 표시

            uiScript.SetUIActive(false); // UI를 일단 비활성화
            adventureObj.gameObject.SetActive(true); // 어드벤처 모드 게임 오브젝트 활성화

            adventureObj.GetComponent<AdventureModeManager>().Init(); // 어드벤처 모드 매니저 초기화
        }
        else
        {
            // 무한 모드 등
            Destroy(adventureObj); // 어드벤처 모드 게임 오브젝트 삭제
            GameStart(); // 게임 바로 시작
        }
    }

}
