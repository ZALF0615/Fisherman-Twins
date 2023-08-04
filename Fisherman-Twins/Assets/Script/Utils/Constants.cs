using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public static class Constants
{
    public static event Action OnConstantsLoaded;

    private const string SHEET_ID = "1VA40i_QLGF7Q7YQhVyofrXhYH-HHyqIS-3mfShDKeCc";  // Your Sheet ID here
    private const string SHEET_NAME = "Constants";  // Your Sheet Name here

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
            StartCoroutine(LoadDataFromSpreadsheet());
        }

        private IEnumerator LoadDataFromSpreadsheet()
        {
            UnityWebRequest request = UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/" + SHEET_ID + "/gviz/tq?tqx=out:csv&sheet=" + SHEET_NAME);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                ParseData(request.downloadHandler.text);
            }

            // Destroy the GameObject after loading the data
            OnConstantsLoaded?.Invoke();
            Destroy(gameObject);
        }

        private void ParseData(string csvData)
        {
            var lines = csvData.Split('\n');

            for (int i = 1; i < lines.Length; i++) // Skip the first row
            {
                var line = lines[i];
                var cells = line.Split(',');

                string key = cells[0].Trim('"');
                float value = float.Parse(cells[1].Trim('"'));

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
        }
    }
}
