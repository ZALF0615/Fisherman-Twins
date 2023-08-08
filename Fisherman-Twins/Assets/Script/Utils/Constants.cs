using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;

public static class Constants
{
    public static event Action OnConstantsLoaded;

    private const string SHEET_NAME = "Constants";

    public static float RIVER_WIDTH;
    public static float BLOCK_SIZE;
    public static float FISH_MARGIN;
    public static float BOAT_WIDTH;

    public static float RAISE_DIFF;
    public static float STUN_DURATION;

    public static int DEFAULT_NET_NUM;
    public static int MAX_NET_WEIGHT;

    static Constants()
    {
        // Create an instance of MonoBehaviour to run the coroutine
        new GameObject("ConstantsInitializer", typeof(RuntimeInitializer));
    }

    private class RuntimeInitializer : MonoBehaviour
    {
        private void Awake()
        {
            DataLoader.OnDataLoaded += OnDataLoaded;
        }

        private void OnDataLoaded(string sheetName)
        {
            if (sheetName == SHEET_NAME)
            {
                var data = DataLoader.dataSheets[SHEET_NAME];
                ParseData(data);
                OnConstantsLoaded?.Invoke();
            }
        }

        private void ParseData(List<string[]> csvData)
        {
            List<string[]> lines = csvData;

            for (int i = 1; i < lines.Count; i++) // Skip the first row
            {
                var columns = lines[i];

                string key = columns[0];
                float value = float.Parse(columns[1]);

                // Debug.Log("Key: " + key + ", Value: " + value);  // 여기서 로그를 출력합니다.

                switch (key)
                {
                    case "RIVER_WIDTH":
                        RIVER_WIDTH = value;
                        break;
                    case "BLOCK_SIZE":
                        BLOCK_SIZE = value;
                        break;
                    case "FISH_MARGIN":
                        FISH_MARGIN = value;
                        break;
                    case "BOAT_WIDTH":
                        BOAT_WIDTH = value;
                        break;
                    case "RAISE_DIFF":
                        RAISE_DIFF = value;
                        break;
                    case "STUN_DURATION":
                        STUN_DURATION = value;
                        break;
                    case "DEFAULT_NET_NUM":
                        DEFAULT_NET_NUM = (int)value;
                        break;
                    case "MAX_NET_WEIGHT":
                        MAX_NET_WEIGHT = (int)value;
                        break;
                    default:
                        Debug.LogWarning("Unknown key in spreadsheet: " + key);
                        break;
                }
            }

            Destroy(this.gameObject);
        }
    }
}
