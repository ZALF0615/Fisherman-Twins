using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

public class MessageWindow : MonoBehaviour
{
    // 메시지와 이름 텍스트 UI
    public Text messageText;
    public Text nameText;

    // 메시지와 이름을 담는 패널
    public GameObject messagePanel;
    public GameObject namePanel;

    // 패널과 버튼을 담는 배경 오브젝트
    public GameObject panelBG;
    public Button panelButton;

    // 게임 오브젝트 창을 담는 오브젝트 창
    public GameObject ObjectWindow;

    // 터치 패드와 커서 표시기
    public Button TouchPad;
    public GameObject CursorMarker;

    // 오디오 소스들
    private AudioSource audioSource;
    public AudioSource SEPlayer;

    // 캐릭터 이미지들과 위치 좌표들
    [SerializeField] private Image[] characterImages;
    private Dictionary<CharacterPosition, Vector3> positionCoordinates;

    // 스프라이트 캐시
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // 구글 스프레드시트 ID
    private const string SHEET_ID = "1Gyz0EZX6HKQdqQ2-ZlDs4FbAoXeSgl1vGzVEVBgrerQ";

    // 메시지 데이터 리스트와 현재 인덱스
    private List<MessageData> messages = new List<MessageData>();
    public int currentIndex = 0;

    private bool isDisplaying = false;
    private bool isPaused = false;
    private string remainingMessage;

    // loading
    // public GameObject loadingModal;


    // 게임 로딩
    public void Init(int stageIdx)
    {
        StartCoroutine(Initialize(stageIdx));
    }

    IEnumerator Initialize(int stageIdx)
    {
        // 구글 스프레드시트 데이터 로드
        var loadData = LoadDataFromSpreadsheet(stageIdx);
        yield return StartCoroutine(loadData);

        // 스프라이트 로드
        var loadSprites = LoadCharacterSprites();
        yield return StartCoroutine(loadSprites);

        // 캐릭터 위치 설정
        positionCoordinates = new Dictionary<CharacterPosition, Vector3>
        {
            { CharacterPosition.None, Vector3.zero },
            { CharacterPosition.L, characterImages[0].transform.position },
            { CharacterPosition.R, characterImages[1].transform.position }
        };

        // 패널 클릭 이벤트 리스너 설정
        TouchPad.onClick.AddListener(HandleClick);
        audioSource = gameObject.AddComponent<AudioSource>();

        // 첫 번째 메시지 표시
        DisplayNextMessage();
    }

    // 캐릭터 스프라이트 로드
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

    // 구글 스프레드시트에서 데이터 로드
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


    // CSV 데이터를 파싱하여 메시지 데이터 리스트로 반환
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


    // 다음 메시지 표시
    public void DisplayNextMessage()
    {
        if (currentSkipBranch != 0)
        {
            // 분기 끝까지 스킵
            while (true)
            {
                var nextMessage = messages[++currentIndex];
                // PrintMessageData(nextMessage);

                if (currentIndex >= messages.Count) 
                {
                    // 스크립트 끝, 일치하는 분기 끝을 찾을 수 없음
                    currentSkipBranch = 0;
                    return;
                }

                var parts = nextMessage.SystemCode.Split('/');

                if (parts[0] == "branch" && parts[2] == "end" && int.Parse(parts[1]) == currentSkipBranch)
                {
                    currentSkipBranch = 0; // 스킵 중지
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
            // 모든 메시지를 다 표시했을 때, 패널과 이름 텍스트 비활성화
            messagePanel.SetActive(false);
            namePanel.SetActive(false);
        }
    }


    // 메시지 표시 코루틴
    private IEnumerator DisplayMessageCoroutine(MessageData data)
    {
        isDisplaying = true;
        CursorMarker.SetActive(false);

        remainingMessage = data.Message;
        messageText.text = "";

        // 캐릭터 이름이 없으면 이름 패널 비활성화
        if (string.IsNullOrEmpty(data.CharacterName))
        {
            namePanel.SetActive(false);
        }
        else
        {
            nameText.text = data.CharacterName;
            namePanel.SetActive(true);
        }

        // 오브젝트 이미지 변경
        if (!string.IsNullOrEmpty(data.ObjectImage))
        {
            ObjectWindow.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>($"Objects/{data.ObjectImage}");
            ObjectWindow.SetActive(true);
        }
        else
        {
            ObjectWindow.SetActive(false);
        }

        // SE 재생
        if (!string.IsNullOrEmpty(data.SE))
        {
            AudioClip clip = Resources.Load<AudioClip>($"SE/{data.SE}");
            SEPlayer.PlayOneShot(clip);
        }


        // 캐릭터 이미지 업데이트
        for (int i = 0; i < characterImages.Length; i++)
        {
            if (data.Characters != null && i < data.Characters.Count)
            {
                CharacterData characterData = data.Characters[i];

                spriteCache.TryGetValue(characterData.ImageFilename, out Sprite characterSprite);

                characterImages[i].sprite = characterSprite;
                characterImages[i].transform.position = positionCoordinates[characterData.Position];

                // 현재 말하고 있는 캐릭터를 강조
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

            // 문자를 한 글자씩 표시
            for (int i = 0; i < data.Message.Length; i++)
            {
                messageText.text += data.Message[i];
                remainingMessage = remainingMessage.Substring(1);

                // 문자마다의 표시 속도 조정
                yield return new WaitForSeconds(0.05f);
            }

            CursorMarker.SetActive(true);
        }

        isDisplaying = false;
    }


    // 선택지 처리 관련

    public GameObject choiceButtonPrefab;
    public GameObject choicePanel;

    public Dictionary<int, bool> choiceFlags = new Dictionary<int, bool>();

    public int currentBranch = 0;
    public int currentSkipBranch = 0;

    Dictionary<string, int> labelIndices = new Dictionary<string, int>();

    // 시스템 코드 처리 함수
    bool ProcessSystemCode(string systemCode, string message)
    {
        var parts = systemCode.Split('/');

        // 점프 (다른 레이블로 이동)
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

        // 레이블
        if (parts[0] == "label")
        {
            return true;
        }

        // 일시 정지
        if (parts[0] == "pause")
        {
            if (float.TryParse(parts[1], out float pauseSeconds))
            {
                StartCoroutine(PauseForSeconds(pauseSeconds));
            }
        }

        // 선택지
        choicePanel.SetActive(parts[0] == "choice");

        if (parts[0] == "choice")
        {
            choicePanel.SetActive(true);
            messagePanel.SetActive(false);
            TouchPad.gameObject.SetActive(false);

            foreach (Transform child in choicePanel.transform) { Destroy(child.gameObject); }

            string[] choices = message.Split('/');
            for (int i = 1; i < parts.Length; i++)
            {
                int choiceIndex = int.Parse(parts[i]);

                // 프리팹에서 버튼을 만들고 choicePanel의 자식으로 만듦
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choicePanel.transform);
                Button button = buttonObj.GetComponent<Button>();
                Text buttonText = buttonObj.GetComponentInChildren<Text>();

                // 버튼 텍스트 설정
                buttonText.text = choices[i - 1];

                // 리스너 추가
                button.GetComponent<Button>().onClick.AddListener(() => {

                    choiceFlags[choiceIndex] = true; // 플래그 설정
                    print($"set flag: {choiceIndex}");

                    foreach (Transform child in choicePanel.transform) { Destroy(child.gameObject); }

                    choicePanel.gameObject.SetActive(false); // 선택지 패널 비활성화
                    messagePanel.SetActive(true);
                    TouchPad.gameObject.SetActive(true);

                    DisplayNextMessage();
                });
            }

            return true;
        }

        // 분기 시작
        else if (parts[0] == "branch" && parts[2] == "start")
        {
            int branchIndex = int.Parse(parts[1]);

            if (choiceFlags.ContainsKey(branchIndex) && choiceFlags[branchIndex])
            {
                // 해당 분기 실행
                print($"Branch start: {branchIndex}");
                currentBranch = branchIndex;
            }
            else
            {
                // 해당 분기 스킵
                print($"Branch skip: {branchIndex}");
                currentSkipBranch = branchIndex;
            }

            DisplayNextMessage();

            return true;
        }

        // 분기 종료
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
            // 모든 문자를 한 번에 표시
            StopAllCoroutines();
            messageText.text = messageText.text + remainingMessage;
            isDisplaying = false;
        }
        else 
        {
            // 다음 메시지 표시
            DisplayNextMessage(); 
        }
    }

    // 메시지 표시를 일시 정지하는 코루틴
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

    // 캐릭터 위치를 나타내는 enum. None은 위치가 지정되지 않은 경우
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

        // 출력 문자열을 콘솔에 표시
        print(output.ToString());
    }


}
