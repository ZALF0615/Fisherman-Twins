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
    public AudioClip hitSound;
    public Action func;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "net")
        {
            switch (obstacleIdx)
            {
                case 28: // 전기뱀장어 전기공격
                    GameController.GetInstance().player.Damage();
                    break;
            }

            if(func != null) { func(); }
        }
    }

}


