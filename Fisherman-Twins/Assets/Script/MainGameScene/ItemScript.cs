/*
 * ItemScript.cs
 * 아이템을 관리하는 스크립트
 * 각 아이템 프리팹에 부여
 * 아이템 효과는 이 곳에서 처리
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemScript : MonoBehaviour
{
    private GameController GC;

    public AudioClip getSound;
    public int itemIdx; // 프리팹의 인스펙터 창에서 직접 지정

    void GetItem()
    {
        switch (itemIdx)
        {
            case 0: // 테스트 용 아이템. 먹으면 즉시 10000골드 획득.
                GC.player.gold_total += 10000;
                break;
        }

        Destroy(this.gameObject);
    }

    private void Start()
    {
        GC = GameController.GetInstance();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Boat")
        {
            GameController.GetInstance().PlaySE(getSound); // 아이템 얻은 효과음 재생
            GetItem();
        }
    }


}


