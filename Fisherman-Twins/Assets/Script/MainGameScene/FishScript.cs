/*
 * FishScript.cs
 * 물고기의 행동을 관리하는 스크립트
 * 각 물고기 프리팹에 부여
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    #region PARAM

    public int fishIdx; // 물고기 인덱스
    public bool isBad; // 나쁜 물고기인지 여부

    [SerializeField]

    public int price; // 물고기 가격
    public float weight; // 물고기 무게
    public float width; // 물고기 너비
    public float speedZ; // 물고기의 z방향 속도

    public AudioClip getSound; // 물고기를 잡았을 때의 소리
    Transform net; // 그물 위치
    public GameObject obstacle; // 물고기에 귀속된 장애물 (ex. 전기뱀장어의 전기공격)
    #endregion PARAM

    // fishIdx에 따른 물고기 특성 초기화
    public void Initialize()
    {
        Dictionary<int, Fish> fishList = GameController.GetInstance().fishData.FishList;
        var fishData = fishList[fishIdx];

        isBad = fishData.IsBad;

        if (!isBad) // 일반 물고기
        {
            price = fishData.Price;
            weight = fishData.Weight;
            width = fishData.Width;
            speedZ = fishData.SpeedZ;
        }
        else
        {
            width = fishData.Width;
            speedZ = fishData.SpeedZ;
        }

        if (fishIdx == 28) // 전기뱀장어
        {
            isBad = true; // 작살을 맞추기 전까지는 나쁜 물고기 취급
            obstacle.GetComponent<ObstacleScript>().func = () => { Destroy(this.gameObject); }; // 전기 공격에 닿을 시 물고기도 함게 사라지도록 처리
        }

        transform.localScale *= width;

        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            // 애니메이션 시작 위치 랜덤화
            string animationName = anim.runtimeAnimatorController.animationClips[0].name;
            anim.Play(animationName, 0, Random.Range(0.0f, 1.0f)); // 시작 시간을 0과 1 사이의 랜덤한 값으로 설정
        }

        originalScale = transform.localScale.x; // 원래의 스케일을 저장
    }

    // 물고기의 특수한 행동을 구현하는 함수
    void PerformBehavior(int idx)
    {
        switch (idx)
        {
            case 14: // 은어
                MoveTowardNet(10f, 10f); // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 15: // 송어
                AvoidNet(10f, 10f); // 그물을 기피 (그물에서 멀어짐)
                break;
            case 25: // 피라냐
                MoveTowardNet(10f, 10f); // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 27: // 복어
                InflateNearNet(10f, 3f, 2f);
                break;
            case 28: // 전기뱀장어
                if (isBad) { MoveTowardNet(10f, 10f); } // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 32: // 뼈 은어
                MoveTowardNet(10f, 10f); // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 35: // 뼈 피라냐
                MoveTowardNet(10f, 10f); // 그물을 향해 돌진 (그물 쪽으로 서서히 이동)
                break;
            case 37: // 뼈 복어
                InflateNearNet(10f, 3f, 2f);
                break;
                // 나머지 물고기도 여기에 추가...
        }
    }

    #region FEATURES

    // 그물 방향으로 이동
    // 시작거리 바깥에서는 작동 안함
    // 시작거리 안쪽에서는, 그물 범위 바깥에 있으면 그물 중심 방향으로 이동, 그물 범위 안쪽이면 직진 이동
    void MoveTowardNet(float startDistance, float speedMultiplier)
    {
        var xDistoNet = Mathf.Abs(transform.position.x - net.transform.position.x); //  물고기와 그물 사이의 수평방향(X) 거리
        var zDistoNet = Mathf.Abs(transform.position.z - net.transform.position.z); //  물고기와 그물 사이의 수직방향(Z) 거리

        var netWidth = GameController.GetInstance().player.netWidth; // 현재 그물의 길이

        if (zDistoNet < startDistance) // 일정 범위 안에서만 그물 감지
        {
            var a = 20;

            // 물고기와 그물 사이의 수평방향(X) 거리가 그물 길이의 0.5a% 이하일 경우 (그물 범위 가운데 a% 안쪽)
            if (xDistoNet <= 0.005* a * 0.5f * netWidth)
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

    // 그물 기피
    void AvoidNet(float startDistance, float speedMultiplier)
    {
        var xDistoNet = Mathf.Abs(transform.position.x - net.transform.position.x); //  물고기와 그물 사이의 수평방향(X) 거리
        var zDistoNet = Mathf.Abs(transform.position.z - net.transform.position.z); //  물고기와 그물 사이의 수직방향(Z) 거리

        var netWidth = GameController.GetInstance().player.netWidth; // 현재 그물의 길이

        if (zDistoNet < startDistance) // 일정 범위 안에서만 그물 감지
        {
            // 물고기와 그물 사이의 수평방향(X) 거리가 그물 길이의 25% 이하일 경우 (그물 범위 가운데 50% 안쪽)
            if (xDistoNet <= 0.5f * 0.5f * netWidth)
            {
                // 그물 바깥 방향으로 이동

                float directionToNet = Mathf.Sign(net.position.x - transform.position.x); // 그물의 x 좌표 방향을 계산
                transform.position += new Vector3(-directionToNet * speedMultiplier * Time.deltaTime, 0, 0); // 그물의 x 좌표 반대 방향으로 이동
            }
            else // 그물 범위 밖
            {
                // 그대로 직진 (X방향 이동 없음)
            }
        }

    }

    private bool hasStartedInflating = false;  // 부풀어오르기 시작한 적이 있는지를 나타내는 변수
    float originalScale; // 원래의 스케일을 저장할 변수

    void InflateNearNet(float inflateDistance, float inflateSpeed, float maxScaleRatio)
    {
        // 물고기와 그물 사이의 거리를 계산
        float distanceToNet = Vector3.Distance(transform.position, net.transform.position);

        // 거리가 inflateDistance 이하이거나 이미 부풀어오르기 시작한 적이 있다면
        if (distanceToNet <= inflateDistance || hasStartedInflating)
        {
            // 부풀어오르기 시작한 적이 있다는 것을 표시
            hasStartedInflating = true;

            // 부풀어 오르도록 스케일을 점차적으로 증가
            float newScale = Mathf.Min(transform.localScale.x + inflateSpeed * Time.deltaTime, originalScale * maxScaleRatio);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }

    #endregion FEATURES

    private void Start()
    {
        net = GameObject.FindGameObjectWithTag("net").transform;

        Initialize();
    }
    private void Update()
    {
        // -z방향으로 이동

        var newZ = transform.position.z - speedZ * GameController.GetInstance().player.speedZ * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 0, newZ);

        // 특성에 따른 행동 구현
        PerformBehavior(fishIdx);
    }
    void OnTriggerEnter(Collider other)
    {
        // "bullet" 태그를 가진 오브젝트와의 충돌 검사
        if (other.tag == "bullet")
        {
            switch (fishIdx)
            {
                case 28: // 전기뱀장어
                    Destroy(obstacle); // 전기 공격 삭제
                    speedZ = -0.2f; // 속도 줄이기
                    isBad = false; // 포
                    break;
                default:
                    // 총알과 물고기 파괴
                    Destroy(other.gameObject);
                    Destroy(gameObject);
                    break;
            }
        }
    }

}

