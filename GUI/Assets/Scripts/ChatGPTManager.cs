using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ChatGPTManager : MonoBehaviour
{
    [SerializeField] private string openAI_API_Key = "";  // OpenAI API Ű �Է�
    [SerializeField] private TMP_InputField inputField;               // �÷��̾� ���� �Է� �ʵ�
    [SerializeField] private TMP_Text responseText;                   // ChatGPT ������ ǥ���� �ؽ�Ʈ

    private const string apiUrl = "https://api.openai.com/v1/chat/completions";  // ChatGPT API ��������Ʈ

    // AI ���� ������ ���� �ý��� �޽���
    private const string systemMessage = "You are a friendly and helpful assistant."; // AI ���� ����

    // ��ư Ŭ�� �� ȣ���� �޼���
    public void SendQuestion()
    {
        string question = inputField.text;
        if (!string.IsNullOrEmpty(question))
        {
            StartCoroutine(SendChatGPTRequest(question));
        }
    }

    // ChatGPT API ��û �޼���
    private IEnumerator SendChatGPTRequest(string prompt)
    {
        // ��û�� JSON �����͸� ���ڿ��� ����, systemMessage �߰�
        string requestData = "{ \"model\": \"gpt-4-turbo\", \"messages\": [" +
                             "{ \"role\": \"system\", \"content\": \"" + systemMessage + "\" }, " +
                             "{ \"role\": \"user\", \"content\": \"" + prompt + "\" }] }";

        // UnityWebRequest�� ����Ͽ� POST ��û ����
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAI_API_Key);

            // ��û ���� �� ���� ���
            yield return request.SendWebRequest();

            // ���� ��� ó��
            if (request.result == UnityWebRequest.Result.Success)
            {
                // ������ �Ľ��ϰ� �ؽ�Ʈ�� ǥ��
                ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(request.downloadHandler.text);
                if (response != null && response.choices.Length > 0)
                {
                    responseText.text = response.choices[0].message.content;
                }
                else
                {
                    responseText.text = "������ ������ �� �����ϴ�.";
                }
            }
            else
            {
                responseText.text = "����: " + request.error;
            }
        }
    }
}

// JSON ���� ������ ���� Ŭ����
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