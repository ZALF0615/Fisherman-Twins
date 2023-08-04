using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    public int fishIdx;
    public bool isBad;


    [SerializeField]
    public string fishName;
    public int price;
    public float weight;

    public float speedZ;

    public AudioClip getSound;
    Transform net;

    // fishIdx에 따른 물고기 특성 초기화
    public void Initialize()
    {
        switch (fishIdx)
        {
            case 11: // 멸치
                fishName = "멸치";
                weight = 0.2f;
                price = 3;
                speedZ = -1.5f;
                break;
            case 12: // 날치
                fishName = "날치";
                weight = 0.5f;
                price = 7;
                speedZ = -1.25f;
                break;
            case 13: // 가재
                fishName = "가재";
                weight = 2.0f;
                price = 18;
                speedZ = -0.7f;
                break;
            case 14: // 은어
                fishName = "은어";
                weight = 4.0f;
                price = 20;
                speedZ = -1.2f;
                break;
            case 15: // 송어
                fishName = "송어";
                weight = 7.0f;
                price = 56;
                speedZ = -0.9f;
                break;
            case 16: // 연어
                fishName = "연어";
                weight = 10.0f;
                price = 80;
                speedZ = -0.6f;
                break;
                // 나머지 물고기도 여기에 추가...
        }

    }

    void PerformBehavior(int idx)
    {
        switch (idx)
        {
            case 11: // 멸치
                break;
            case 12: // 날치
                // 멸치의 행동 구현
                break;
            case 13: // 가재
                break;
            case 14: // 은어
                MoveTowardNet(10f, 3f); // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 15: // 송어
                MoveTowardNet(10f, -1.5f); // 그물을 기피 (그물에서 멀어짐)
                break;
            case 16: // 연어
                // 연어의 행동 구현
                break;
                // 나머지 물고기도 여기에 추가...
        }
    }

    // Features

    void MoveTowardNet(float startDistance, float speedMultiplier)
    {
        
        var xDistoNet = Mathf.Abs(transform.position.x - net.transform.position.x); //  물고기와 그물 사이의 수평방향(X) 거리
        var zDistoNet = Mathf.Abs(transform.position.z - net.transform.position.z); //  물고기와 그물 사이의 수직방향(Z) 거리

        var netWidth = GameController.GetInstance().player.netWidth; // 현재 그물의 길이

        if(zDistoNet < startDistance) // 일정 범위 안에서만 그물 감지
        {
            // 물고기와 그물 사이의 수평방향(X) 거리가 그물 길이의 25% 이하일 경우 (그물 범위 가운데 50% 안쪽)
            if (xDistoNet <= 0.5f * 0.5f *netWidth)
            {   
                // 그대로 직진 (X방향 이동 없음)
            }
            else // 그물 범위 밖
            {
                float directionToNet = Mathf.Sign(net.position.x - transform.position.x); // 그물의 x 좌표 방향을 계산
                transform.position += new Vector3(directionToNet * speedMultiplier * Time.deltaTime, 0, 0); // 그물의 x 좌표 방향으로 이동
            }
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
        // -z방향으로 이동

        var newZ = transform.position.z + speedZ * GameController.GetInstance().player.speedZ *Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 0, newZ);

        // 특성에 따른 행동 구현
        PerformBehavior(fishIdx);
    }
}
