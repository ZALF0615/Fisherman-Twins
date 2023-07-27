using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class RiverGenerator : MonoBehaviour
{
    public Transform player;
    public Transform blockParent;

    public GameObject[] blockPrefabs;
    public GameObject[] fishPrefabs;

    public int startBlockIdx;
    public int preInstantiateNum;

    List<GameObject> generatedBlockList = new List<GameObject>();
    int currentBlockIdx;

    void UpdateBlock(int toBlockIdx)
    {
        if (toBlockIdx <= currentBlockIdx) { return; }

        for (int i = currentBlockIdx + 1; i <= toBlockIdx; i++)
        {
            var blockObj = GenerateBlock(i);
            generatedBlockList.Add(blockObj);
        }

        while (generatedBlockList.Count > preInstantiateNum + 2)
        {
            DestroyOldestBlock();
        }

        currentBlockIdx = toBlockIdx;
    }
    GameObject GenerateBlock(int blockIdx)
    {
        int nextBlockIdx = Random.Range(0, blockPrefabs.Length);

        var blockObj = (GameObject)Instantiate(
            blockPrefabs[nextBlockIdx],
            new Vector3(0, 0, blockIdx * BLOCK_SIZE),
            Quaternion.identity
            );

        blockObj.transform.SetParent(blockParent);

        if (blockIdx > 1)
        {
            // 오브젝트 생성

            int fishNum = Random.Range(10, 20);

            for (int i = 0; i < fishNum; i++)
            {
                int fishIdx = Random.Range(0, fishPrefabs.Length);

                var posX = Random.Range(FISH_MARGIN - RIVER_WIDTH / 2.0f, RIVER_WIDTH / 2.0f - FISH_MARGIN);
                var posZ = Random.Range(BLOCK_SIZE / -2.0f, BLOCK_SIZE / 2.0f);

                var fish = (GameObject)Instantiate(fishPrefabs[fishIdx]);
                fish.transform.SetParent(blockObj.transform);
                fish.transform.localPosition = new Vector3(posX, 0, posZ);
            }
        }


        return blockObj;
    }
    void DestroyOldestBlock()
    {
        var oldBlock = generatedBlockList[0];
        generatedBlockList.RemoveAt(0);
        Destroy(oldBlock);
    }

    private void Awake()
    {
        Constants.OnConstantsLoaded += Init;
    }

    private bool isInitialized = false;
    void Init()
    {
        if (isInitialized) { return; }

        currentBlockIdx = startBlockIdx - 1;
        UpdateBlock(preInstantiateNum);

        isInitialized = true;
    }

    void Update()
    {
        // 캐릭터 위치에서부터 현재 블록까지의 인덱스를 계산
        int charaPositionIdx = (int)(player.position.z / BLOCK_SIZE);
        
        // 다음 블록에 들어가면 블록들을 갱신
        if(charaPositionIdx + preInstantiateNum > currentBlockIdx)
        {
            UpdateBlock(charaPositionIdx + preInstantiateNum);
        }
    }

}
