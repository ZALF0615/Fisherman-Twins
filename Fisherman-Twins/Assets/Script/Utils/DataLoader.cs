using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
    public string SHEET_ID;

    // ｽﾃﾆｮ ﾀﾌｸｧｰ? ｵ･ﾀﾌﾅﾍｸｦ ﾀ?ﾀ衂ﾒ ｵ?ｼﾅｳﾊｸｮ

    private List<string> loadSheetNames = new List<string>() { "", "" };
    public Dictionary<string, string> dataSheets = new Dictionary<string, string>();

    void Start()
    {
        // loadSheetNames ｾﾈｿ｡ ﾀﾖｴﾂ ｸ?ｵ?ｽﾃﾆｮｸ?｡ ｴ?ﾘ ｷﾎｵ?
        foreach(var sheetName in loadSheetNames)
        {
            StartCoroutine(LoadDataFromSpreadsheet(sheetName));
        }
    }

    // ﾀﾌ ｸﾞｼﾒｵ蟠ﾂ ｽｺﾇﾁｷｹｵ蠖ﾃﾆｮｿ｡ｼｭ ｵ･ﾀﾌﾅﾍｸｦ ｺﾒｷｯｿﾍ ﾀ?ﾀ衂ﾕｴﾏｴﾙ.
    public IEnumerator LoadDataFromSpreadsheet(string sheetName)
    {
        string url = $"https://docs.google.com/spreadsheets/d/{SHEET_ID}/gviz/tq?tqx=out:csv&sheet={sheetName}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ｵ･ﾀﾌﾅﾍｸｦ ｵ?ｼﾅｳﾊｸｮｿ｡ ﾀ?ﾀ衂ﾕｴﾏｴﾙ.
            dataSheets[sheetName] = request.downloadHandler.text;
        }
        else
        {
            Debug.LogError("Data loading failed: " + request.error);
        }
    }

    // ﾀﾌ ｸﾞｼﾒｵ蟠ﾂ ﾀ?ﾀ蠏ﾈ ｵ･ﾀﾌﾅﾍｸｦ ﾆﾄｽﾌﾇﾕｴﾏｴﾙ.
    public void ParseData(string sheetName)
    {
        // ｽﾃﾆｮ ﾀﾌｸｧﾀｸｷﾎ ｵ･ﾀﾌﾅﾍｸｦ ﾃ｣ｽﾀｴﾏｴﾙ.
        if (dataSheets.ContainsKey(sheetName))
        {
            string csvData = dataSheets[sheetName];
            // ｿｩｱ篩｡ ｵ･ﾀﾌﾅﾍｸｦ ﾆﾄｽﾌﾇﾏｴﾂ ﾄﾚｵ蟶ｦ ﾃﾟｰ｡ﾇﾕｴﾏｴﾙ.
            // ｿｹｸｦ ｵ鮴? CSV ｵ･ﾀﾌﾅﾍｸｦ ﾇ牴? ｿｭｷﾎ ｳｪｴｩｰ? ｰ｢ ﾇﾗｸ?ﾀｻ ﾆﾄｽﾌﾇﾏｿｩ ｸｮｽｺﾆｮｳｪ ｵ?ｼﾅｳﾊｸｮｿ｡ ﾀ?ﾀ衂ﾒ ｼ? ﾀﾖｽﾀｴﾏｴﾙ.
        }
        else
        {
            Debug.LogError("Sheet not found: " + sheetName);
        }
    }
}
