using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    public int fishIdx;
    public bool isBad;

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
    Transform net;

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
            case 27: // 복어
                fishName = "복어";
                minWeight = 0f;
                maxWeight = 0f;
                priceMultiplier = 0f;
                break;
                // 나머지 물고기도 여기에 추가...
        }

        weight = Random.Range(minWeight, maxWeight);
        price = weight * priceMultiplier;

        // ... 나머지 필드 초기화 ...
    }

    void PerformBehavior(int idx)
    {
        switch (idx)
        {
            case 11: // 멸치
                // 멸치의 행동 구현
                break;
            case 13: // 은어
                MoveTowardNet(10f, 3f); // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 15: // 송어
                MoveTowardNet(10f, -1.5f); // 그물을 기피 (그물에서 멀어짐)
                break;
            case 16: // 연어
                // 연어의 행동 구현
                break;
            case 27: // 복어
                InflateNearNet(10f, 10f, 3.0f); // 그물 가까이에서 부풀어오름
                break;
                // 나머지 물고기도 여기에 추가...
        }
    }

    // Features

    void MoveTowardNet(float startDistance, float speedMultiplier)
    {
        // 물고기와 그물 사이의 거리를 계산
        float distanceToNet = Vector3.Distance(transform.position, net.transform.position);

        // 거리가 startDistance 이하일 경우
        if (distanceToNet <= startDistance)
        {
            // 그물의 x 좌표 방향을 계산
            float directionToNet = Mathf.Sign(net.position.x - transform.position.x);

            // 그물의 x 좌표 방향으로 이동
            transform.position += new Vector3(directionToNet * speedMultiplier * Time.deltaTime, 0, 0);
        }


    }

    private bool hasStartedInflating = false;  // 부풀어오르기 시작한 적이 있는지를 나타내는 변수
    void InflateNearNet(float inflateDistance, float inflateSpeed, float maxScale)
    {
        // 물고기와 그물 사이의 거리를 계산
        float distanceToNet = Vector3.Distance(transform.position, net.transform.position);

        // 거리가 inflateDistance 이하이거나 이미 부풀어오르기 시작한 적이 있다면
        if (distanceToNet <= inflateDistance || hasStartedInflating)
        {
            // 부풀어오르기 시작한 적이 있다는 것을 표시
            hasStartedInflating = true;

            // 부풀어 오르도록 스케일을 점차적으로 증가
            float newScale = Mathf.Min(transform.localScale.x + inflateSpeed * Time.deltaTime, maxScale);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }




    private void Start()
    {
        net = GameObject.FindGameObjectWithTag("net").transform;

        Initialize();
    }
    private void Update()
    {
        // 특성에 따른 행동 구현
        PerformBehavior(fishIdx);
    }
}
