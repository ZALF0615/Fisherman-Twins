using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public static class DataLoader
{
    public static event Action<string> OnDataLoaded;

    public static readonly string SHEET_ID = "1VA40i_QLGF7Q7YQhVyofrXhYH-HHyqIS-3mfShDKeCc";  // Your Sheet ID here

    static List<string> loadSheetNames = new List<string>() { "Constants", "FishData" };
    static public Dictionary<string, List<string[]>> dataSheets = new Dictionary<string, List<string[]>>();
    static DataLoader()
    {
        // Create an instance of MonoBehaviour to run the coroutine
        new GameObject("ConstantsInitializer", typeof(RuntimeInitializer));
    }

    private class RuntimeInitializer : MonoBehaviour
    {
        void Awake()
        {
            foreach (var sheetName in loadSheetNames)
            {
                StartCoroutine(LoadDataFromSpreadsheet(sheetName));
            }
        }

        public IEnumerator LoadDataFromSpreadsheet(string sheetName)
        {
            // print("Load Data: " + sheetName);

            string url = $"https://docs.google.com/spreadsheets/d/{SHEET_ID}/gviz/tq?tqx=out:csv&sheet={sheetName}";
            UnityWebRequest request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                dataSheets[sheetName] = ParseCSV(request.downloadHandler.text);
                OnDataLoaded?.Invoke(sheetName);
            }
            else
            {
                Debug.LogError("Data loading failed: " + request.error);
            }


            Destroy(this.gameObject);
        }

        public List<string[]> ParseCSV(string csvData)
        {
            List<string[]> rows = new List<string[]>();
            bool insideQuote = false;
            List<string> columns = new List<string>();
            string currentColumn = "";

            for (int i = 0; i < csvData.Length; i++)
            {
                char currentChar = csvData[i];

                if (currentChar == '"')
                {
                    insideQuote = !insideQuote;
                    continue;
                }

                if (insideQuote)
                {
                    currentColumn += currentChar;
                }
                else
                {
                    if (currentChar == ',')
                    {
                        columns.Add(currentColumn);
                        currentColumn = "";
                    }
                    else if (currentChar == '\n')
                    {
                        columns.Add(currentColumn);
                        rows.Add(columns.ToArray());
                        columns.Clear();
                        currentColumn = "";
                    }
                    else
                    {
                        currentColumn += currentChar;
                    }
                }
            }

            // 마지막 행 처리
            if (!string.IsNullOrEmpty(currentColumn) || columns.Count > 0)
            {
                columns.Add(currentColumn);
                rows.Add(columns.ToArray());
            }

            return rows;
        }
    }
}
