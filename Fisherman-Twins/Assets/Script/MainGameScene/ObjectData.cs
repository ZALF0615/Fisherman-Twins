using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectData : MonoBehaviour
{
    private const string SHEET_NAME = "FishData";
    public Dictionary<int, Fish> FishList = new Dictionary<int, Fish>();
    public Dictionary<int, Obstacle> ObstacleList = new Dictionary<int, Obstacle>();

    private void Awake()
    {
        DataLoader.OnDataLoaded += OnDataLoaded;
    }
    private void OnDataLoaded(string sheetName)
    {
        if (sheetName == SHEET_NAME)
        {
            ParseFishData();
        }
    }

    void ParseData(string sheetName)
    {
        switch (sheetName)
        {
            case "FishData":
                ParseFishData();
                break;
        }
    }

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

            var fish = new Fish(idx, name, isBad, weight, price, width, speedZ);

            FishList.Add(idx, fish);
        }

        // DisplayAllFishData();
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
        string csvHeader = "Idx,Name,IsBad,Weight,Price,Width,SpeedZ";
        List<string> csvRows = new List<string> { csvHeader };

        // 각 레코드를 CSV 형식의 행으로 변환
        foreach (var fishEntry in FishList)
        {
            Fish fish = fishEntry.Value;
            string csvRow = $"{fish.Idx},{fish.Name},{fish.IsBad},{fish.Weight},{fish.Price},{fish.Width},{fish.SpeedZ}";
            csvRows.Add(csvRow);
        }

        // 모든 행을 하나의 문자열로 결합
        string csvOutput = string.Join("\n", csvRows);
        Debug.Log(csvOutput);
    }

}

public class Fish{
    public int Idx;
    public string Name;

    public bool IsBad;

    public float Weight;
    public int Price;

    public float Width;
    public float SpeedZ;

    // public Action Behaviour;

    public Fish(int idx, string name, bool isBad, float weight, int price, float width, float speedZ)
    {
        Idx = idx;
        Name = name;
        IsBad = isBad;
        Weight = weight;
        Price = price;
        Width = width;
        SpeedZ = speedZ;
    }

    public override string ToString()
    {
        return $"Idx: {Idx}, Name: {Name}, IsBad: {IsBad}, Weight: {Weight}, Price: {Price}, Width: {Width}, SpeedZ: {SpeedZ}";
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

