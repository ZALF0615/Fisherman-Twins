/*
 * PlayerController.cs
 *
 * 플레이어의 조작을 관리하는 스크립트
 * 
 *  PLAYER CONTROLL: 플레이어 이동과 그물 들어올리기 등 조작에 관한 부분
 *  NET: 그물의 위치를 조정, 충돌 처리
 *  EVENT: 물고기를 잡거나, 장애물에 부딪혔을 때의 이벤트 처리
 *  RUNTIME: 런타임 관련
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class PlayerController : MonoBehaviour
{
    #region PARAM
    GameController GC; // 게임 컨트롤러 인스턴스 참조

    public Transform playerParent; // 플레이어 부모 오브젝트
    public GameObject boat1; // 플레이어 1의 보트
    public GameObject boat2; // 플레이어 2의 보트

    Animator anim_boat1; // 플레이어 1의 보트 애니메이션
    Animator anim_boat2; // 플레이어 2의 보트 애니메이션

    public float speedX = 15; // 플레이어의 x축 이동 속도
    public float speedZ = 6; // 플레이어의 z축 이동 속도

    public int gold_total; // 총 골드 양
    public int gold_net; // 현재 그물에 잡힌 골드 양
    public float weight; // 현재 그물에 잡힌 물고기의 무게

    public int net_left; // 남은 그물 수

    public float distance; // 이동한 거리
    public float distanceOffset = 0f; // 거리 오프셋 (어드벤처 모드에서 오프닝 대화를 하는 동안에도 배는 앞으로 가고 있으므로, 대화 끝나고 게임 시작할 때 시점의 위치를 저장)

    float recoverTime = 0.0f; // 회복 시간
    public float raiseTime1p, raiseTime2p = 0.0f; // 그물을 들어올리는 시간
    public bool up1p, up2p; // 그물을 들어올리는 중인지 여부

    #endregion PARAM

    #region PLAYER CONTROLL

    // 현재 플레이어가 스턴 상태인지 확인
    // (현재는 항상 false를 반환하도록 되어 있음, 추후 수정)
    bool IsStun() { return false; }

    // 플레이어의 보트 조작에 관한 함수
    void BoatMove()
    {
        if (damageOngoing) { return; } // 플레이어가 현재 피해 상태이면 조작 불가

        // 플레이어 1 (왼쪽 플레이어)
        float moveHorizontal = Input.GetAxis("Player1Horizontal");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, 0.0f);
        Vector3 newPosition = boat1.transform.localPosition + movement * speedX * Time.deltaTime;

        if (newPosition.x > boat2.transform.localPosition.x - BOAT_WIDTH) { newPosition.x = boat2.transform.localPosition.x - BOAT_WIDTH; } // 플레이어2보다 오른쪽으로 가지 못하게 함
        if (newPosition.x - BOAT_WIDTH / 2.0f < RIVER_WIDTH / -2.0f) { newPosition.x = (BOAT_WIDTH - RIVER_WIDTH) / 2.0f; } // 강 폭을 넘어가지 못하게 함

        boat1.transform.localPosition = newPosition;

        // 플레이어 2 (오른쪽 플레이어)
        moveHorizontal = Input.GetAxis("Player2Horizontal");

        movement = new Vector3(moveHorizontal, 0.0f, 0.0f);
        newPosition = boat2.transform.localPosition + movement * speedX * Time.deltaTime;

        if (newPosition.x < boat1.transform.localPosition.x + BOAT_WIDTH) { newPosition.x = boat1.transform.localPosition.x + BOAT_WIDTH; } // 플레이어1보다 왼쪽으로 가지 못하게 함
        if (newPosition.x + BOAT_WIDTH / 2.0f > RIVER_WIDTH / 2.0f) { newPosition.x = (RIVER_WIDTH - BOAT_WIDTH) / 2.0f; } // 강 폭을 넘어가지 못하게 함

        boat2.transform.localPosition = newPosition;
    }

    // 그물을 들어올리는 함수
    // 그물에 잡힌 골드와 무게를 초기화하고, 총 골드에 그물에 잡힌 골드를 더함
    void RaiseNet()
    {
        gold_total += gold_net; // 그물에 잡힌 골드를 총 골드에 더함

        gold_net = 0; // 그물에 잡힌 골드를 초기화
        weight = 0; // 그물의 무게를 초기화
    }

    #endregion PLAYER

    #region NET

    public float netWidth;  // 그물의 너비
    public float netX; // 그물 중심의 x 위치

    // 그물의 위치와 크기를 업데이트하는 함수
    // 보트 1과 보트 2 사이의 위치에 그물을 위치시키고, 두 보트 사이의 거리에 맞추어 그물의 너비를 조절
    void LocateNet()
    {
        var x1 = boat1.transform.position.x;
        var x2 = boat2.transform.position.x;

        netWidth = (x2 - x1);  // 보트 1과 보트 2 사이의 거리를 그물의 너비로 설정
        netX = (x1 + x2) / 2.0f;  // 보트 1과 보트 2의 중간 지점을 그물의 x 위치로 설정

        // 그물의 너비 업데이트
        transform.localScale = new Vector3(netWidth, transform.localScale.y, transform.localScale.z);
        // 그물의 위치 업데이트
        transform.position = new Vector3(netX, transform.position.y, transform.position.z);
    }

    // 그물에 다른 오브젝트가 충돌했을 때 호출되는 함수
    // 충돌한 오브젝트가 물고기일 경우, 물고기의 가격과 무게에 따라 처리
    // 충돌한 오브젝트가 장애물일 경우, 장애물의 인덱스에 따라 처리
    private void OnTriggerEnter(Collider other)
    {
        if (IsStun()) { return; }  // 플레이어가 스턴 상태인 경우 아무 처리도 하지 않음

        if (other.gameObject.tag == "Fish")  // 충돌한 오브젝트가 물고기인 경우
        {
            var fish = other.GetComponent<FishScript>();  // 물고기 스크립트를 가져옴

            AudioClip sound;

            if (!fish.isBad)  // 좋은 물고기인 경우
            {
                GetFish(fish.price, fish.weight);  // 물고기를 얻음
            }
            else  // 나쁜 물고기인 경우
            {
                GetBadFish(fish.fishIdx);  // 나쁜 물고기 얻음 처리
            }

            var getSound = fish.getSound;  // 물고기를 잡는 소리를 가져옴
            GC.PlaySE(getSound);  // 소리를 재생
            Destroy(other.gameObject);  // 물고기 오브젝트 삭제
        }
        else if (other.gameObject.tag == "Obstacle")  // 충돌한 오브젝트가 장애물인 경우
        {
            var obs = other.GetComponent<ObstacleScript>();  // 장애물 스크립트를 가져옴
            HitObstacle(obs.obstacleIdx);  // 장애물 충돌 처리

            var hitSound = obs.hitSound;  // 장애물에 부딪히는 소리를 가져옴
            GC.PlaySE(hitSound);  // 소리를 재생
        }

    }

    #endregion NET

    #region BULLET
    public GameObject bulletPrefab; // 총알 프리팹
    public Transform firePoint1; // 플레이어 1의 발사 위치
    public Transform firePoint2; // 플레이어 2의 발사 위치
    public float bulletSpeed = 50f; // 총알 속도

    // 총알 발사
    void FireBullet(Vector3 firePosition)
    {
        // 총알 생성 및 초기 위치 설정
        GameObject bullet = Instantiate(bulletPrefab, firePosition, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        // 총알 속도 설정
        bulletScript.speed = speedZ + bulletSpeed;
    }

    #endregion BULLET

    #region EVENT
    // 돈, 무게, 그물 개수 등 플레이어와 관련된 이벤트 처리

    bool damageOngoing; // 플레이어가 현재 피해를 입는 중인지 여부

    // 물고기를 얻었을 때 처리

    public void GetFish(int _price, float _weight)
    {
        gold_net += _price; // 그물에 물고기의 가격을 더함
        weight += _weight; // 그물의 무게를 증가시킴

        // 그물의 무게가 최대 무게를 초과하면 피해를 입음
        if (weight > MAX_NET_WEIGHT) { Damage(); }
    }

    // 나쁜 물고기를 얻었을 때
    public void GetBadFish(int idx)
    {
        if (idx == 26 || idx == 36) // 맹독물고기 or 뼈 맹독물고기
        {
            Poison();
        }
        else if(idx == 28) // 전기뱀장어
        {
            // 데미지 없음 (전기 공격 부분이 데미지 충돌 처리)
        }
        else // 나머지
        {
            Damage();
        }
    }

    // 장애물에 부딪혔을 때
    public void HitObstacle(int idx)
    {
        switch (idx)
        {
            case 0: // 바위
                Damage();
                break;
            case 2: // 소용돌이1
                Damage();
                break;
            case 4: // 벌레
                Damage();
                break;
            case 5: // 현무암바위
                Damage();
                break;
            case 9: // 쇼용돌이2
                Damage();
                break;
            case 28: // 전기뱀장어 전기공격
                Damage();
                break;
        }
    }

    // 피해를 입었을 때
    public void Damage()
    {
        gold_net = 0; // 그물에 있는 골드를 초기화
        weight = 0; // 그물의 무게를 초기화
        net_left--; // 남은 그물의 수를 감소

        StartCoroutine(DamageAnim()); // 피해 애니메이션 시작

        // 남은 그물의 수가 0 이하가 되면 게임 오버
        if (net_left <= 0) { GC.GameOver(); }
    }

    // 독 데미지
    public void Poison()
    {
        float poisonRatio = Random.Range(1f, 20f); // 1% ~ 20%
        
        int targetGold = (int)(gold_net * ((100 - poisonRatio) * 0.01f) + 0.5f); // 그물 안 물고기 가격 1~20% 감소
        gold_net = targetGold;

        StartCoroutine(DamageAnim()); // 피해 애니메이션 시작
    }

    // 피해를 입은 후의 애니메이션을 처리하는 코루틴
    IEnumerator DamageAnim()
    {
        damageOngoing = true; // 피해를 입은 상태로 표시
        speedZ /= 5f; // 속도를 감소

        yield return new WaitForSeconds(1); // 1초 동안 대기

        damageOngoing = false; // 피해 상태를 해제
        speedZ *= 5f; // 속도를 원래대로 복원
    }
    #endregion EVENT

    #region RUNTIME

    private void Awake() { Constants.OnConstantsLoaded += Init; }  // 게임이 시작될 때 Constants가 로드된 뒤에 Init 함수를 호출

    void Start()
    {
        GC = GameController.GetInstance();

        anim_boat1 = boat1.GetComponent<Animator>();
        anim_boat2 = boat2.GetComponent<Animator>();
    }

    private bool isInitialized = false;

    void Init()
    {
        if (isInitialized) { return; }

        net_left = DEFAULT_NET_NUM;

        // speedZ 조정

        int secondPerTile = 10; // 한 타일 당 몇 초가 걸리는지


        speedZ = Constants.BLOCK_SIZE / secondPerTile;

        isInitialized = true;
    }

    public void GameStart()
    {
        if (GameManager.currentScene == GameScene.GameScene_Adventure)
        {
            distanceOffset = distance;
        }

    }

    void Update()
    {

        // 플레이어 z방향으로 전진
        var newZ = playerParent.transform.position.z + speedZ * Time.deltaTime;
        playerParent.transform.position = new Vector3(0, 0, newZ);

        distance = (transform.position.z / 20f) - distanceOffset;

        if (!isInitialized) { return; }
        if (!GC.isGameOngoing) { return; }

        if (IsStun())
        {
            recoverTime -= Time.deltaTime;
        }
        else
        {

            // 플레이어 x 방향 이동
            BoatMove();

            // 그물 위치 조정
            LocateNet();

            // 그물 들어올리는 판정
            if (up1p) { raiseTime1p -= Time.deltaTime; up1p = raiseTime1p > 0; }
            if (up2p) { raiseTime2p -= Time.deltaTime; up2p = raiseTime2p > 0; }

            if (Input.GetButtonDown("Player1Raise")) { up1p = true; raiseTime1p = RAISE_DIFF; }
            if (Input.GetButtonDown("Player2Raise")) { up2p = true; raiseTime2p = RAISE_DIFF; }

            if (up1p && up2p)
            {
                RaiseNet();

                up1p = up2p = false;
                raiseTime1p = raiseTime2p = 0;
            }

            // 작살 발사
            if (Input.GetButtonDown("Player1Fire")) { FireBullet(firePoint1.position); }
            if (Input.GetButtonDown("Player2Fire")) { FireBullet(firePoint2.position); }
        }
    }
    #endregion RUNTIME
}
