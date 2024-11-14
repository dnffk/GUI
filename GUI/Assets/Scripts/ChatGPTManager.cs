using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;

public class ChatGPTManager : MonoBehaviour
{
    [SerializeField] private string openAI_API_Key = "API";  // OpenAI API 키 입력 (기본 값)

    [SerializeField] private TMP_InputField inputField; // 플레이어 질문 입력 필드
    [SerializeField] private TMP_Text responseText;    // ChatGPT 응답 표시 텍스트

    private const string apiUrl = "https://api.openai.com/v1/chat/completions"; // ChatGPT API 엔드포인트

    // AI 성격 설정을 위한 시스템 메시지
    private const string systemMessage = "You are a friendly and helpful assistant."; // AI 성격 설정

    // Unity 시작 시 호출
    void Awake()
    {
        LoadApiKeyFromJson(); // JSON 파일에서 API 키 로드
    }

    // API 키를 JSON 파일에서 로드하여 변수에 저장
    private void LoadApiKeyFromJson()
    {
        string configPath = Path.Combine(Application.dataPath, "JSON/config.json"); // JSON 파일 경로

        if (File.Exists(configPath))
        {
            try
            {
                // JSON 파일 읽기
                string jsonContent = File.ReadAllText(configPath);
                Config config = JsonUtility.FromJson<Config>(jsonContent); // JSON 데이터 파싱
                openAI_API_Key = config.apiKey; // API 키를 설정
                Debug.Log("API Key successfully loaded from JSON: " + openAI_API_Key);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load API key from config.json: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("config.json file not found at: " + configPath);
        }
    }

    // 버튼 클릭 시 호출할 메서드
    public void SendQuestion()
    {
        string question = inputField.text;
        if (!string.IsNullOrEmpty(question))
        {
            StartCoroutine(SendChatGPTRequest(question));
        }
    }

    // ChatGPT API 요청 메서드
    private IEnumerator SendChatGPTRequest(string prompt)
    {
        // 요청할 JSON 데이터를 문자열로 생성
        string requestData = "{ \"model\": \"gpt-4-turbo\", \"messages\": [" +
                             "{ \"role\": \"system\", \"content\": \"" + systemMessage + "\" }, " +
                             "{ \"role\": \"user\", \"content\": \"" + prompt + "\" }] }";

        // UnityWebRequest를 사용하여 POST 요청 설정
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAI_API_Key);

            // 요청 전송 및 응답 대기
            yield return request.SendWebRequest();

            // 응답 결과 처리
            if (request.result == UnityWebRequest.Result.Success)
            {
                // 응답을 파싱하고 텍스트에 표시
                ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(request.downloadHandler.text);
                if (response != null && response.choices.Length > 0)
                {
                    responseText.text = response.choices[0].message.content;
                }
                else
                {
                    responseText.text = "응답을 가져올 수 없습니다.";
                }
            }
            else
            {
                responseText.text = "에러: " + request.error;
                Debug.LogError("Request failed: " + request.downloadHandler.text);
            }
        }
    }
}

// JSON 파일의 구조를 정의하는 클래스
[System.Serializable]
public class Config
{
    public string apiKey;
}

// JSON 응답 데이터 구조 클래스
[System.Serializable]
public class ChatGPTResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class Message
{
    public string content;
}
