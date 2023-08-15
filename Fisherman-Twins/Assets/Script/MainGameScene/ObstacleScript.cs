/*
 * ObstacleScript.cs
 * 장애물 기믹을 관리하는 스크립트
 * 각 장애물 프리팹에 부여
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObstacleScript : MonoBehaviour
{
    public int obstacleIdx;

    public float width;

    public AudioClip hitSound;
    public Action func;

    // fishIdx에 따른 물고기 특성 초기화
    public void Initialize()
    {
        Dictionary<int, Obstacle> obstacleDataList = GameController.GetInstance().objectData.ObstacleList;
        var obstacleData = obstacleDataList[obstacleIdx];

        width = obstacleData.Width;
        transform.localScale *= width;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "net")
        {
            if(func != null) { func(); } // 장애물 쪽에서 특수한 조건하에 행동을 해야할 경우
        }
    }

}


