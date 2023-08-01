using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    public int fishIdx;

    // 최종 값
    [SerializeField]
    public float price;
    public float weight;

    // 각 물고기 고유값
    public string fishName;
    float minWeight;
    float maxWeight;
    float priceMultiplier;

    public AudioClip getSound;

    // fishIdx에 따른 물고기 특성 초기화
    public void Initialize()
    {
        switch (fishIdx)
        {
            case 11: // 멸치
                fishName = "멸치";
                minWeight = 0.005f;
                maxWeight = 0.015f;
                priceMultiplier = 1f;
                break;
            case 13: // 은어
                fishName = "은어";
                minWeight = 0.5f;
                maxWeight = 0.7f;
                priceMultiplier = 0.2f;
                break;
            case 15: // 송어
                fishName = "송어";
                minWeight = 3f;
                maxWeight = 5f;
                priceMultiplier = 0.3f;
                break;
            case 16: // 연어
                fishName = "연어";
                minWeight = 5f;
                maxWeight = 10f;
                priceMultiplier = 0.4f;
                break;
                // 나머지 물고기도 여기에 추가...
        }

        weight = Random.Range(minWeight, maxWeight);
        price = weight * priceMultiplier;

        // ... 나머지 필드 초기화 ...
    }

    private void Start()
    {
        Initialize();
    }
}
