using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class PlayerController : MonoBehaviour
{
    #region PARAM
    GameController GC;

    public Transform playerParent;
    public GameObject boat1;
    public GameObject boat2;

    Animator anim_boat1;
    Animator anim_boat2;

    public float speedX = 15;
    public float speedZ = 20;

    public int gold_total, gold_net;
    public int weight;
    public int net_left;

    public float distance;

    float recoverTime = 0.0f;
    public float raiseTime1p, raiseTime2p = 0.0f;
    public bool up1p, up2p;

    #endregion PARAM

    #region PLAYER CONTROLL
    bool IsStun()
    {
        return false;
    }
    void BoatMove()
    {
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
    void RaiseNet()
    {
        gold_total += gold_net;
        gold_net = 0;

        weight = 0;

        print("Raise Net");
    }

    #endregion PLAYER

    #region NET
    void LocateNet()
    {
        var x1 = boat1.transform.position.x;
        var x2 = boat2.transform.position.x;

        var netWidth = (x2 - x1);
        var netX = (x1 + x2) / 2.0f;

        transform.localScale = new Vector3(netWidth, transform.localScale.y, transform.localScale.z);
        transform.position = new Vector3(netX, transform.position.y, transform.position.z);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsStun()) { return; }

        if(other.gameObject.tag == "Fish")
        {
            var fish = other.GetComponent<FishScript>();
            GetFish(fish.price, fish.weight);

            var getSound = fish.getSound;
            GC.PlaySE(getSound);

            Destroy(other.gameObject);
        }
        else if(other.gameObject.tag == "Obstacle")
        {
            var obs = other.GetComponent<ObstacleScript>();
            Obstacle(obs.obstacleIdx);

            var hitSound = obs.hitSound;
            GC.PlaySE(hitSound);

            // Destroy(other.gameObject);
        }
    }

    #endregion NET

    #region EVENT

    bool damageOngoing;

    public void GetFish(int p, int w)
    {
        gold_net += p;
        weight += w;

        if (weight > MAX_NET_WEIGHT) { Damage(); }
    }
    public void Obstacle(int idx)
    {
        switch (idx)
        {
            case 0: // pile
                // print("obstacle: pile");
                Damage();
                break;
        }
    }
    public void Damage()
    {
        gold_net = 0;
        weight = 0;
        net_left--;

        StartCoroutine(DamageAnim());

        if (net_left == 0) { GC.GameOver(); }
    }

    IEnumerator DamageAnim()
    {
        damageOngoing = true;
        speedZ /= 5f;

        yield return new WaitForSeconds(1);

        damageOngoing = false;
        speedZ *= 5f;
    }
    #endregion EVENT

    #region RUNTIME
    void Start()
    {
        GC = GameController.GetInstance();

        net_left = DEFAULT_NET_NUM;

        anim_boat1 = boat1.GetComponent<Animator>();
        anim_boat2 = boat2.GetComponent<Animator>();
    }
    void Update()
    {
        if (IsStun())
        {
            recoverTime -= Time.deltaTime;
        }
        else
        {
            // 플레이어 z방향으로 전진
            var newZ = playerParent.transform.position.z + speedZ * Time.deltaTime;
            playerParent.transform.position = new Vector3(0, 0, newZ);

            // 플레이어 x 방향 이동
            BoatMove();

            // 그물 위치 조정
            LocateNet();

            // 그물 들어올리는 판정

            if (up1p)
            {
                raiseTime1p -= Time.deltaTime;
                if(raiseTime1p <= 0) { up1p = false; raiseTime1p = 0; }
            }
            if (up2p)
            {
                raiseTime2p -= Time.deltaTime;
                if (raiseTime2p <= 0) { up2p = false; raiseTime2p = 0; }
            }

            if (up1p) { raiseTime1p -= Time.deltaTime; up1p = raiseTime1p > 0; }
            if (up2p) { raiseTime2p -= Time.deltaTime; up2p = raiseTime2p > 0; }

            if (Input.GetButtonDown("Player1Up")) { up1p = true; raiseTime1p = RAISE_DIFF; }
            if (Input.GetButtonDown("Player2Up")) { up2p = true; raiseTime2p = RAISE_DIFF; }

            if (up1p && up2p)
            {
                RaiseNet();
               
                up1p = up2p = false;
                raiseTime1p = raiseTime2p = 0;
            }
        }

        distance = transform.position.z / 20f;
    }
    #endregion RUNTIME
}
