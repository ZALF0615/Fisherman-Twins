using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;
using System.IO;

public class MessageWindow : MonoBehaviour
{
    public Text messageText;
    public Text nameText;
    public GameObject messagePanel;
    public GameObject namePanel;

    public GameObject panelBG;
    public Button panelButton;
    public GameObject ObjectWindow;

    public Button TouchPad;
    public GameObject CursorMarker;

    private AudioSource audioSource;
    public AudioSource SEPlayer;

    public AudioSource SE_dice;
    public AudioSource SE_select;
    public AudioSource Happy;


    [SerializeField] private Image[] characterImages;
    private Dictionary<CharacterPosition, Vector3> positionCoordinates;
    
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private const string SHEET_ID = "1Gyz0EZX6HKQdqQ2-ZlDs4FbAoXeSgl1vGzVEVBgrerQ";

    private List<MessageData> messages = new List<MessageData>();
    public int currentIndex = 0;

    private bool isDisplaying = false;
    private bool isPaused = false;
    private string remainingMessage;

    // backLog

    public GameObject backlogPanel; // BacklogPanelへの参照
    private List<(string, string)> backlogEntries = new List<(string, string)>(); // バックログのデータを格納するリスト

    public Transform contentTransform; // Contentオブジェクトへの参照
    public GameObject backlogEntryPrefab; // バックログエントリのPrefabへの参照

    // loading

    // public GameObject loadingModal;

    public void Init(int stageIdx)
    {
        StartCoroutine(Initialize(stageIdx));
    }

    IEnumerator Initialize(int stageIdx)
    {
        // loadingModal.SetActive(true);

        // Load spreadsheet data
        var loadData = LoadDataFromSpreadsheet(stageIdx);
        yield return StartCoroutine(loadData);

        // Load sprites
        var loadSprites = LoadCharacterSprites();
        yield return StartCoroutine(loadSprites);

        // Set up positions
        positionCoordinates = new Dictionary<CharacterPosition, Vector3>
    {
        { CharacterPosition.None, Vector3.zero },
        { CharacterPosition.L, characterImages[0].transform.position },
        { CharacterPosition.R, characterImages[1].transform.position }
    };

        // Set up panel click event listener
        TouchPad.onClick.AddListener(HandleClick);
        audioSource = gameObject.AddComponent<AudioSource>();


        DisplayNextMessage();
        // yield return new WaitForSeconds(0.7f);
        //yield return StartCoroutine(FadeOutLoadingModal());

        // Display first message

    }

    private IEnumerator LoadCharacterSprites()
    {
        Sprite[] characterSprites = Resources.LoadAll<Sprite>("CharacterImages");
        foreach (Sprite sprite in characterSprites)
        {
            // print($"loadSprite: " + sprite.name);
            spriteCache[sprite.name] = sprite;
        }
        yield return null;
    }
    private IEnumerator LoadDataFromSpreadsheet(int stageIdx)
    {
        string sheet_name = string.Format("scenario_stage_{0}", stageIdx);
        UnityWebRequest request = UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/" + SHEET_ID + "/gviz/tq?tqx=out:csv&sheet=" + sheet_name);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            var dataText = request.downloadHandler.text;

            var messageData = ParseDialogueData(dataText);
            messages = messageData;
        }
    }

    private List<MessageData> ParseDialogueData(string csvText)
    {
        var lines = csvText.Split('\n');

        var messages = new List<MessageData>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Split(new string[1] { "\",\"" }, System.StringSplitOptions.None);
            columns[0] = columns[0].TrimStart('"');
            columns[columns.Length - 1] = columns[columns.Length - 1].TrimEnd('"');
            // print($"line: {i}, count: {columns.Length}, content: {line}");

            string systemCode = columns[0];
            string characterName = columns[1];
            string speaker = columns[2];
            string message = columns[3];
            string SE = columns[6];
            string objectImage = columns[7];

            // character image
            var characters = new List<CharacterData>();
            for (int j = 4; j <= 5; j++)
            {
                var character = columns[j];
                if (!string.IsNullOrEmpty(character))
                {
                    var position = (CharacterPosition)(j - 3);
                    // print("========================================");
                    // print("speaker: " + speaker);
                    // print("position.ToString().ToLower()n: " + position.ToString().ToLower());
                    // print("========================================");
                    characters.Add(new CharacterData(character, position, speaker.Contains(position.ToString().ToUpper())));
                }
            }

            // label
            if (systemCode.StartsWith("label/"))
            {
                string labelName = systemCode.Split('/')[1];
                if (!labelIndices.ContainsKey(labelName))
                {
                    labelIndices[labelName] = i;
                }

            }

            var mes = new MessageData(i + 1, systemCode, message, characterName, characters, SE, objectImage);
            // PrintMessageData(mes);

            messages.Add(mes);
        }

        return messages;
    }

    public void DisplayNextMessage()
    {
        if (currentSkipBranch != 0)
        {
            // Skipping until branch end
            while (true)
            {
                var nextMessage = messages[++currentIndex];
                // PrintMessageData(nextMessage);

                if (currentIndex >= messages.Count) 
                {
                    // End of script, can't find matching branch end
                    currentSkipBranch = 0;
                    return;
                }

                var parts = nextMessage.SystemCode.Split('/');

                if (parts[0] == "branch" && parts[2] == "end" && int.Parse(parts[1]) == currentSkipBranch)
                {
                    currentSkipBranch = 0; // Stop skipping
                    DisplayNextMessage();
                    break;
                }

                // print($"skip: {nextMessage.Message}");
            }
        }

        if (isDisplaying) return;
        if (isPaused) return;

        if (currentIndex < messages.Count)
        {
            MessageData nextMessage = messages[currentIndex++];
            // PrintMessageData(nextMessage);

            if (ProcessSystemCode(nextMessage.SystemCode, nextMessage.Message)) { return; }

            StartCoroutine(DisplayMessageCoroutine(nextMessage));
        }
        else
        {
            messagePanel.SetActive(false);
            namePanel.SetActive(false);
        }
    }

    public bool wasnone = false;
    string beforeBGM = "";

    private IEnumerator DisplayMessageCoroutine(MessageData data)
    {
        isDisplaying = true;
        CursorMarker.SetActive(false);

        remainingMessage = data.Message;
        messageText.text = "";

        if (string.IsNullOrEmpty(data.CharacterName))
        {
            namePanel.SetActive(false);
        }
        else
        {
            nameText.text = data.CharacterName;
            namePanel.SetActive(true);
        }

        // Object image change

        if (!string.IsNullOrEmpty(data.ObjectImage))
        {
            ObjectWindow.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>($"Objects/{data.ObjectImage}");
            ObjectWindow.SetActive(true);
        }
        else
        {
            ObjectWindow.SetActive(false);
        }

        // SE Play
        if (!string.IsNullOrEmpty(data.SE))
        {
            AudioClip clip = Resources.Load<AudioClip>($"SE/{data.SE}");
            SEPlayer.PlayOneShot(clip);
        }


        // 立ち絵の更新
        for (int i = 0; i < characterImages.Length; i++)
        {
            if (data.Characters != null && i < data.Characters.Count)
            {
                CharacterData characterData = data.Characters[i];

                // DictionaryからSpriteを取得
                spriteCache.TryGetValue(characterData.ImageFilename, out Sprite characterSprite);

                characterImages[i].sprite = characterSprite;
                characterImages[i].transform.position = positionCoordinates[characterData.Position];

                // 現在話しているキャラクターを強調
                characterImages[i].color = characterData.IsActive ? Color.white : Color.gray;

                characterImages[i].gameObject.SetActive(true);
            }
            else
            {
                characterImages[i].gameObject.SetActive(false);
            }
        }
        
        if (string.IsNullOrEmpty(data.Message))
        {
            panelBG.SetActive(false);

            // CursorMarker.SetActive(true);
            messageText.gameObject.SetActive(false);
            nameText.gameObject.SetActive(false);
        }
        else
        {
            panelBG.SetActive(true);

            messageText.gameObject.SetActive(true);
            nameText.gameObject.SetActive(true);

            // 文字を一文字ずつ表示
            for (int i = 0; i < data.Message.Length; i++)
            {
                messageText.text += data.Message[i];
                remainingMessage = remainingMessage.Substring(1);

                // 文字ごとの表示速度を調整
                yield return new WaitForSeconds(0.05f);
            }

            CursorMarker.SetActive(true);
            backlogEntries.Add((data.CharacterName, data.Message));

        }

        isDisplaying = false;
    }


    // choice
    public GameObject choiceButtonPrefab;
    public GameObject choicePanel;

    public Dictionary<int, bool> choiceFlags = new Dictionary<int, bool>();

    public int currentBranch = 0;
    public int currentSkipBranch = 0;

    //skip

    Dictionary<string, int> labelIndices = new Dictionary<string, int>();

    // ED

    public Text EDText;

    bool ProcessSystemCode(string systemCode, string message)
    {
        var parts = systemCode.Split('/');

        // jump
        if (parts[0] == "jump")
        {
            string labelName = parts[1];
            if (labelIndices.ContainsKey(labelName))
            {
                currentIndex = labelIndices[labelName] - 1;
                print($"jump to: {labelName}, {labelIndices[labelName]}");
                currentSkipBranch = 0;
            }

            return true;
        }

        // label
        if (parts[0] == "label")
        {

            if(parts[1] == "alone")
            {
                Happy.Stop();
            }

            return true;
        }

        // pause

        if (parts[0] == "pause")
        {
            if (float.TryParse(parts[1], out float pauseSeconds))
            {
                StartCoroutine(PauseForSeconds(pauseSeconds));
            }
        }

        // Choice
        choicePanel.SetActive(parts[0] == "choice");

        if (parts[0] == "choice")
        {
            int idx1 = int.Parse(parts[1]);
            int idx2 = int.Parse(parts[2]);

            if (choiceFlags.ContainsKey(1) && choiceFlags[1] && idx1 != 17)
            {
                // Roll Dice

                int forceIdx = Random.Range(0, 2) % 2 == 0 ? idx1 : idx2;

                if (idx1 == 3 || idx1 == 5 || idx1 == 15) { forceIdx = idx1; }     

                choiceFlags[forceIdx] = true;
                print($"set radnom flag: {forceIdx}");


                SE_dice.Play();
                StartCoroutine(PauseForSeconds(2f));
            }
            else
            {
                choicePanel.SetActive(true);
                messagePanel.SetActive(false);
                TouchPad.gameObject.SetActive(false);

                foreach (Transform child in choicePanel.transform) { Destroy(child.gameObject); }

                string[] choices = message.Split('/');
                for (int i = 1; i < parts.Length; i++)
                {
                    int choiceIndex = int.Parse(parts[i]);

                    // Create button from prefab and make it a child of choicePanel

                    GameObject buttonObj = Instantiate(choiceButtonPrefab, choicePanel.transform);
                    Button button = buttonObj.GetComponent<Button>();
                    Text buttonText = buttonObj.GetComponentInChildren<Text>();

                    // Set button text
                    buttonText.text = choices[i - 1];

                    // Add listener
                    button.GetComponent<Button>().onClick.AddListener(() => {

                        choiceFlags[choiceIndex] = true; // Set flag
                        print($"set flag: {choiceIndex}");

                        foreach (Transform child in choicePanel.transform) { Destroy(child.gameObject); }

                        choicePanel.gameObject.SetActive(false); // Hide choicePanel
                        messagePanel.SetActive(true);
                        TouchPad.gameObject.SetActive(true);

                        DisplayNextMessage();
                    });
                }
            }

            return true;
        }

        else if (parts[0] == "check")
        {
            bool f7 = choiceFlags.ContainsKey(7) && choiceFlags[7];
            bool f9 = choiceFlags.ContainsKey(9) && choiceFlags[9];

            if (!f7 || !f9) {
                choiceFlags[11] = true;
                currentIndex = labelIndices["work"];
            }

            DisplayNextMessage();
            return true;
        }

        // Branch start
        else if (parts[0] == "branch" && parts[2] == "start")
        {
            int branchIndex = int.Parse(parts[1]);

        

            if (choiceFlags.ContainsKey(branchIndex) && choiceFlags[branchIndex])
            {
                // branch 실행
                print($"Branch start: {branchIndex}");
                currentBranch = branchIndex;
            }
            else
            {
                // branch 스킵
                print($"Branch skip: {branchIndex}");
                currentSkipBranch = branchIndex;
            }

            DisplayNextMessage();

            return true;
        }

        // Branch end
        else if (parts[0] == "branch" && parts[2] == "end" && int.Parse(parts[1]) == currentBranch)
        {
            currentBranch = 0;
            DisplayNextMessage();

            return true;
        }

        else if(parts[0] == "game_start")
        {
            this.gameObject.SetActive(false);
            GameController.GetInstance().GameStart();

            print("game_start");

            return true;
        }

            return false;
    }

    private void HandleClick()
    {
        if (isPaused) return;

        if (isDisplaying)
        {
            // すべての文字を表示する
            StopAllCoroutines();
            messageText.text = messageText.text + remainingMessage;
            isDisplaying = false;
        }
        else 
        {
            // SE_select.Play();
            DisplayNextMessage(); 
        }
    }

    public void ToggleBacklog()
    {
        backlogPanel.SetActive(!backlogPanel.activeSelf);

        if (backlogPanel.activeSelf) { UpdateBacklogText(); }
    }

    private void UpdateBacklogText()
    {
        // 既存のエントリを削除
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // 新しいエントリを追加
        foreach (var entry in backlogEntries)
        {
            GameObject newEntry = Instantiate(backlogEntryPrefab, contentTransform);
            Text nameText = newEntry.transform.GetComponentsInChildren<Text>()[0];
            Text messageText = newEntry.transform.GetComponentsInChildren<Text>()[1];

            string characterName = entry.Item1;
            string message = entry.Item2;

            // 名前がない場合は名前のテキストを非表示に
            if (string.IsNullOrEmpty(characterName))
            {
                newEntry.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                nameText.text = characterName;
            }

            messageText.text = message;
        }
    }


    private IEnumerator PauseForSeconds(float seconds)
    {
        isPaused = true;
        panelBG.SetActive(false);
        messageText.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
        CursorMarker.SetActive(false);

        yield return new WaitForSeconds(seconds);

        isPaused = false;
        isDisplaying = false;

        DisplayNextMessage();
    }

    //private IEnumerator FadeOutLoadingModal()
    //{
    // CanvasGroup canvasGroup = loadingModal.GetComponent<CanvasGroup>();

    // フェードアウトの速さ
    //float fadeSpeed = 2f;

    // alphaを1から0にフェードアウト
    // while (canvasGroup.alpha > 0f)
    //{
    //canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
    //yield return null;
    // }

    // フェードアウト後、モーダルを非アクティブにする
    //loadingModal.SetActive(false);
    // }
    //[SerializeField]
    // float fadeDuration = 1f; // フェードにかかる時間。Inspectorから変更可能。

    public class MessageData
    {
        public int Idx;

        public string SystemCode;
        public string Message;
        public string CharacterName;
        public List<CharacterData> Characters;
        public string SE;
        public string ObjectImage;

        public MessageData(int idx, string systemCode, string message, string characterName, List<CharacterData> characters, string se, string objectImage)
        {
            Idx = idx;
            SystemCode = systemCode;
            Message = message;
            CharacterName = characterName;
            Characters = characters;
            SE = se;
            ObjectImage = objectImage;
        }
    }



    public class CharacterData
    {
        public string ImageFilename;
        public CharacterPosition Position;
        public bool IsActive;

        public CharacterData(string imageFilename, CharacterPosition position, bool isActive = true)
        {
            ImageFilename = imageFilename;
            Position = position;
            IsActive = isActive;
        }
    }


    public enum CharacterPosition { None, L, R }

    public static void PrintMessageData(MessageData messageData)
    {
        if (messageData == null)
        {
            print("MessageData is null.");
            return;
        }

        StringBuilder output = new StringBuilder();

        output.AppendLine($"idx: { messageData.Idx}, systemCode: {messageData.SystemCode}, content: {messageData.Message}");

        output.AppendLine("SystemCode: " + messageData.SystemCode);
        output.AppendLine("Message: " + messageData.Message);
        output.AppendLine("CharacterName: " + messageData.CharacterName);

        if (messageData.Characters != null)
        {
            output.AppendLine("Characters:");
            foreach (var character in messageData.Characters)
            {
                if (character != null)
                {
                    output.AppendLine("\tImageFilename: " + character.ImageFilename);
                    output.AppendLine("\tPosition: " + character.Position);
                    output.AppendLine("\tIsActive: " + character.IsActive);
                }
            }
        }

        // 出力文字列をコンソールに表示
        print(output.ToString());
    }


}
