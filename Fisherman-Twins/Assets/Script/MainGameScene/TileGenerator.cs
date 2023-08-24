/*
 * TileGenerator.cs
 *
 * 타일 생성 관련
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;
using System.Linq;
using System.Text;

public class TileGenerator : MonoBehaviour
{
    public Transform player; // 플레이어 위치
    public Transform tileParent; // 생성된 모든 타일의 부모 객체

    public GameObject[] tilePrefabs; // 타일 프리팹 배열
    public GameObject[] ObjectPrefabs; // 오브젝트(물고기, 장애물, 아이템) 프리팹 배열

    public int startTileIdx; // 시작 타일 인덱스
    public int preInstantiateNum; // 사전 인스턴스화 숫자

    List<GameObject> generatedTileList = new List<GameObject>(); // 생성된 타일 리스트
    public int currentTileIdx; // 현재 타일 인덱스

    public int currentPhaseIdx; // 현재 단계(페이즈) 번호 

    // 타일을 업데이트하는 함수
    // 주어진 인덱스까지 타일을 생성하고, 타일 리스트의 크기가 사전 인스턴스화 숫자보다 큰 경우 가장 오래된 타일을 제거
    // 이 방식으로 게임이 진행됨에 따라 신규 타일이 생성되고 오래된 타일이 제거되어 전체 타일 수를 일정하게 유지
    void UpdateTile(int toTileIdx)
    {
        // 주어진 인덱스가 현재 타일 인덱스보다 작거나 같으면 업데이트할 필요가 없으므로 함수를 종료
        if (toTileIdx <= currentTileIdx) { return; }

        // 현재 타일 인덱스에서 주어진 인덱스까지 타일을 생성
        for (int i = currentTileIdx + 1; i <= toTileIdx; i++)
        {
            var tileObj = GenerateTile(i); // 타일 생성
            generatedTileList.Add(tileObj); // 생성된 타일을 리스트에 추가
        }

        // 타일 리스트의 크기가 사전 인스턴스화 숫자보다 큰 경우 가장 오래된 타일을 제거
        while (generatedTileList.Count > preInstantiateNum + 2)
        {
            DestroyOldestTile(); // 가장 오래된 타일 제거
        }

        // 현재 타일 인덱스를 업데이트
        currentTileIdx = toTileIdx; 
    }

    // 타일과 물고기를 생성하는 함수
    // 주어진 타일 인덱스 위치에 타일을 생성하고, 해당 타일 위에 물고기 생성
    GameObject GenerateTile(int tileIdx)
    {
        // 타일 프리팹 배열에서 랜덤하게 타일을 선택
        int nextTileIdx = Random.Range(0, tilePrefabs.Length);

        // 선택된 타일 프리팹을 인스턴스화하여 타일 오브젝트를 생성
        var tileObj = (GameObject)Instantiate(
            tilePrefabs[nextTileIdx],
            new Vector3(0, 0, tileIdx * BLOCK_SIZE),
            Quaternion.identity
            );

        // 생성된 타일의 부모를 tileParent로 설정
        tileObj.transform.SetParent(tileParent);

        // 페이즈 인덱스 계산
        int phaseIdx = tileIdx - 1;

        print($"phaseIdx:{phaseIdx}, tileIdx:{tileIdx}");

        // 물고기 생성
        if (tileIdx > 1 && GameController.GetInstance().isGameOngoing)
        {
            int currentStage = 1; // 임시값

            StringBuilder logBuilder = new StringBuilder(); // 로그 문자열을 만들기 위한 StringBuilder

            // 현재 스테이지에 귀속된 물고기 프리팹만 가져옴
            List<GameObject> stageFishPrefabs = ObjectPrefabs.Where(fishPrefab =>
            {
                FishScript fishScript = fishPrefab.GetComponent<FishScript>();

                if (fishScript == null) // FishScript가 없을 경우 -> 물고기 떼
                {
                    Transform child = fishPrefab.transform.GetChild(0); // 첫 번째 자식
                    for (int i = 0; i < fishPrefab.transform.childCount; i++)
                    {
                        fishScript = child.GetComponent<FishScript>();
                        if (fishScript != null)
                            break; // FishScript를 가진 첫 번째 자식을 찾았으므로 loop 탈출
                        child = fishPrefab.transform.GetChild(i); // 다음 자식으로 이동
                    }
                }

                int fishIdx = fishScript.fishIdx;  // 물고기의 idx 가져오기
                FishData fishData = GameController.GetInstance().objectData.FishDataList[fishIdx]; // 해당 idx의 FishData 가져오기
                return fishData.StageIdx == currentStage;  // 현재 스테이지와 물고기의 StageBound가 일치하는지 확인
            }).ToList();

            // stageFishPrefabs 내의 프리팹 이름들을 출력
            // string prefabNames = string.Join(", ", stageFishPrefabs.Select(prefab => prefab.name));
            // print(prefabNames);

            logBuilder.AppendLine($"Current Stage: {currentStage} | Tile Index: {tileIdx}");


            // 추린 각각의 물고기에 대해
            foreach (var fishPrefab in stageFishPrefabs)
            {
                FishScript fishScript = fishPrefab.GetComponent<FishScript>();
                int fishIdx, totalFishCountForCurrentPhase;

                // 물고기 떼인 경우
                if (fishScript == null)
                {
                    var schoolFish = fishPrefab.transform.GetChild(0).GetComponent<FishScript>(); // 첫 번째 물고기로부터 정보 가져오기
                    int schoolFishCount = fishPrefab.transform.childCount; // 물고기 떼 내의 물고기 수

                    fishIdx = schoolFish.fishIdx;
                    FishData fishData = GameController.GetInstance().objectData.FishDataList[fishIdx];
                    totalFishCountForCurrentPhase = fishData.PhaseCounts[phaseIdx - 1] / schoolFishCount; // 현재 페이즈에서의 해당 물고기의 총 생성 수
                }
                else // 개별 물고기인 경우
                {
                    fishIdx = fishScript.fishIdx;
                    FishData fishData = GameController.GetInstance().objectData.FishDataList[fishIdx];
                    totalFishCountForCurrentPhase = fishData.PhaseCounts[phaseIdx - 1];
                }

                for (int i = 0; i < totalFishCountForCurrentPhase; i++)
                {
                    var posX = UnityEngine.Random.Range(FISH_MARGIN - RIVER_WIDTH / 2.0f, RIVER_WIDTH / 2.0f - FISH_MARGIN);
                    var posZ = UnityEngine.Random.Range(BLOCK_SIZE / -2.0f, BLOCK_SIZE / 2.0f);

                    var fish = (GameObject)Instantiate(fishPrefab);
                    fish.transform.SetParent(tileObj.transform);
                    fish.transform.localPosition = new Vector3(posX, 0, posZ);

                    logBuilder.AppendLine($"Fish Prefab: {fishPrefab.name} | Created at Position: ({posX}, 0, {posZ})");
                }
            }

            print(logBuilder.ToString());  // 로그 출력
        }

        // 생성된 타일 오브젝트를 반환
        return tileObj;
    }


    // 가장 오래된 타일을 삭제하는 함수
    void DestroyOldestTile()
    {
        var oldTile = generatedTileList[0]; // 가장 오래된 타일을 가져옴
        generatedTileList.RemoveAt(0); // 리스트에서 제거
        Destroy(oldTile); // 오브젝트 삭제
    }

    private bool isInitialized = false;

    // 초기화 함수
    // 초기 타일을 생성하고, 타일 리스트를 업데이트
    public void Init()
    {
        if (isInitialized) { return; }

        currentTileIdx = startTileIdx - 1; // 현재 타일 인덱스를 시작 타일 인덱스로 설정
        UpdateTile(preInstantiateNum); // 타일 업데이트

        isInitialized = true;
    }

    // 게임이 시작될 때 Constants가 로드된 뒤에 Init 함수를 호출
    private void Awake() { ObjectData.OnFishDataParsed += Init; }

    void Update()
    {
        // if (!isInitialized) { return; }

        // 플레이어의 현재 위치를 기반으로 캐릭터 위치 인덱스를 계산
        int charaPositionIdx = (int)(player.position.z / BLOCK_SIZE);

        // 플레이어가 다음 타일으로 이동하면 타일들을 업데이트
        if (charaPositionIdx + preInstantiateNum > currentTileIdx)
        {
            UpdateTile(charaPositionIdx + preInstantiateNum);
        }
    }
}
