using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectData : MonoBehaviour
{
    public Dictionary<int, FishData> FishDataList = new Dictionary<int, FishData>();
    public Dictionary<int, int> PhaseFishCounts = new Dictionary<int, int>(); // 각 단계별 총 물고기 수량

    public Dictionary<int, Obstacle> ObstacleList = new Dictionary<int, Obstacle>();

    private void Awake()
    {
        DataLoader.OnDataLoaded += OnDataLoaded;
    }

    private void OnDataLoaded(string sheetName)
    {
        if (sheetName == "FishData")
        {
            ParseFishData();
        }
    }

    public delegate void FishDataParsedHandler();
    public static event FishDataParsedHandler OnFishDataParsed;
    public void ParseFishData()
    {
        print("LoadFishData");

        List<string[]> lines = DataLoader.dataSheets["FishData"];

        // 헤더 행을 가져와 각 열의 인덱스를 찾음
        var headers = lines[0];
        Dictionary<string, int> columnIndexes = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
        {
            if (!string.IsNullOrEmpty(headers[i]))  // 헤더 이름이 없는 경우 패스
            {
                columnIndexes[headers[i]] = i;
            }
        }

        // foreach (var key in columnIndexes.Keys) { print($"key: {key}, idx: {columnIndexes[key]}"); }

        for (int i = 1; i < lines.Count; i++)
        {
            var columns = lines[i];

            if (!int.TryParse(columns[columnIndexes["idx"]], out int idx))
            {
                continue; // idx 값이 존재하지 않으면 현재 행을 건너뜀
            }

            string name = columns[columnIndexes["이름"]];
            bool isBad = columns[columnIndexes["구분"]] == "나쁜";

            if (!float.TryParse(columns[columnIndexes["무게 (kg)"]], out float weight)) { weight = 0f; }
            if (!int.TryParse(columns[columnIndexes["가격 (G)"]], out int price)) { price = 0; }
            if (!float.TryParse(columns[columnIndexes["너비"]], out float width)) { width = 0; }
            if (!float.TryParse(columns[columnIndexes["속도"]], out float speedZ)) { speedZ = 0; }


            // 각 단계(Phase)별 등장 개수 데이터 처리

            int[] phaseCounts = new int[6]; // 6단계까지의 물고기 수량

            for (int phase = 0; phase < 6; phase++)
            {
                int.TryParse(columns[columnIndexes["1단계"] + phase], out phaseCounts[phase]);

                // 현재 단계의 물고기 수량을 누적
                if (!PhaseFishCounts.ContainsKey(phase + 1))
                {
                    PhaseFishCounts[phase + 1] = 0;
                }
                PhaseFishCounts[phase + 1] += phaseCounts[phase];
            }

            // 등장 스테이지 번호 처리

            int.TryParse(columns[columnIndexes["등장스테이지"]], out int stageIdx);

            var fishData = new FishData(idx, name, isBad, weight, price, width, speedZ, phaseCounts, stageIdx);
            FishDataList.Add(idx, fishData);
        }

        DisplayAllFishData();

        // 모든 FishData가 파싱된 후에 이벤트 발생
        OnFishDataParsed?.Invoke();
    }
    public void ParseObstacleData()
    {
        print("LoadObstacleData");

        List<string[]> lines = DataLoader.dataSheets["ObstacleData"];

        // 헤더 행을 가져와 각 열의 인덱스를 찾음
        var headers = lines[0];
        Dictionary<string, int> columnIndexes = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
        {
            if (!string.IsNullOrEmpty(headers[i]))  // 헤더 이름이 없는 경우 패스
            {
                columnIndexes[headers[i]] = i;
            }
        }

        foreach (var key in columnIndexes.Keys) { print($"key: {key}, idx: {columnIndexes[key]}"); }

        for (int i = 1; i < lines.Count; i++)
        {
            var columns = lines[i];

            if (!int.TryParse(columns[columnIndexes["idx"]], out int idx))
            {
                continue; // idx 값이 존재하지 않으면 현재 행을 건너뜀
            }

            string name = columns[columnIndexes["이름"]];

            if (!float.TryParse(columns[columnIndexes["너비"]], out float width)) { width = 0; }

            var obstacle = new Obstacle(idx, name, width);

            ObstacleList.Add(idx, obstacle);
        }

    }
    public void DisplayAllFishData()
    {
        // 헤더 구성
        string csvHeader = "Idx,Name,IsBad,Weight,Price,Width,SpeedZ,StageIdx,Phase1,Phase2,Phase3,Phase4,Phase5,Phase6";
        List<string> csvRows = new List<string> { csvHeader };

        // 각 레코드를 CSV 형식의 행으로 변환
        foreach (var fishEntry in FishDataList)
        {
            FishData fish = fishEntry.Value;
            string phaseCountsStr = string.Join(",", fish.PhaseCounts); // 각 단계별 수량을 콤마로 구분
            string csvRow = $"{fish.Idx},{fish.Name},{fish.IsBad},{fish.Weight},{fish.Price},{fish.Width},{fish.SpeedZ},{fish.StageIdx},{phaseCountsStr}";
            csvRows.Add(csvRow);
        }

        // 모든 행을 하나의 문자열로 결합
        string csvOutput = string.Join("\n", csvRows);
        Debug.Log(csvOutput);
    }


}

public class FishData{

    public int Idx;
    public string Name;
    public bool IsBad;
    public float Weight;
    public int Price;
    public float Width;
    public float SpeedZ;

    public int StageIdx; // 이 물고기가 귀속된 스테이지
    public int[] PhaseCounts;
    // public Action Behaviour;

    public FishData(int idx, string name, bool isBad, float weight, int price, float width, float speedZ, int[] phaseCounts, int stageIdx)
    {
        Idx = idx;
        Name = name;
        IsBad = isBad;
        Weight = weight;
        Price = price;
        Width = width;
        SpeedZ = speedZ;
        PhaseCounts = phaseCounts;
        StageIdx = stageIdx;
    }

    public override string ToString()
    {
        string phaseCountsStr = $"PhaseCounts: [{string.Join(", ", PhaseCounts)}]";
        return $"Idx: {Idx}, Name: {Name}, IsBad: {IsBad}, Weight: {Weight}, Price: {Price}, Width: {Width}, SpeedZ: {SpeedZ}, {phaseCountsStr}";
    }


}
public class Obstacle
{
    public int Idx;
    public string Name;

    public float Width;

    // public Action Behaviour;

    public Obstacle(int idx, string name, float width)
    {
        Idx = idx;
        Name = name;
        Width = width;
    }

    public override string ToString()
    {
        return $"Idx: {Idx}, Name: {Name}, Width: {Width}";
    }

}

