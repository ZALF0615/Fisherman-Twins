using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureModeManager : MonoBehaviour
{
    public int stageIdx;

    public void Init()
    {
        stageIdx = GameManager.currentStageIdx;
        this.GetComponent<MessageWindow>().Init(stageIdx);
    }

}
