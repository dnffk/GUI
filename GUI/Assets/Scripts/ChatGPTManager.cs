using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ChatGPTManager : MonoBehaviour
{
    [SerializeField] private string openAI_API_Key = "";  // OpenAI API 키 입력
    [SerializeField] private TMP_InputField inputField;               // 플레이어 질문 입력 필드
    [SerializeField] private TMP_Text responseText;                   // ChatGPT 응답을 표시할 텍스트

    private const string apiUrl = "https://api.openai.com/v1/chat/completions";  // ChatGPT API 엔드포인트

    // AI 성격 설정을 위한 시스템 메시지
    private const string systemMessage = "You are a friendly and helpful assistant."; // AI 성격 설정

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
        // 요청할 JSON 데이터를 문자열로 생성, systemMessage 추가
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
            }
        }
    }
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
