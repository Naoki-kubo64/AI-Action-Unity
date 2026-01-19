using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AIAction.AI
{
    [Serializable]
    public struct ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class AIActionResponse
    {
        public string action;
        public float duration;
        public float strength; // Optional: 0.0 to 1.0
    }

    public interface IModelStrategy
    {
        IEnumerator SendRequest(List<ChatMessage> history, Action<string> onSuccess, Action<string> onError);
    }

    [Serializable]
    public class ConfigData
    {
        public string geminiApiKey;
    }

    public class StandardStrategy : IModelStrategy
    {
        private string apiKey;
        private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public StandardStrategy(string key) { this.apiKey = key; }

        public IEnumerator SendRequest(List<ChatMessage> history, Action<string> onSuccess, Action<string> onError)
        {
            Debug.Log($"SendRequest called. API Key present: {!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_API_KEY"}");
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY")
            {
                // Mock response if no API key
                Debug.Log("Using mock response (no API key)");
                yield return new WaitForSeconds(0.5f);
                onSuccess?.Invoke("[ { \"action\": \"WALK_RIGHT\", \"duration\": 2.0 } ]");
                yield break;
            }

            Debug.Log("Sending real Gemini API request...");
            string url = $"{API_URL}?key={apiKey}";
            
            // Enhanced system prompt for platformer game
            var systemPrompt = @"あなたは2Dプラットフォーマーゲームのキャラクターを操作するAIです。
プレイヤーの指示を解釈し、適切なアクションのJSON配列で応答してください。

【利用可能なアクション】
移動系:
- WALK_RIGHT: 右に歩く (duration: 秒数)
- WALK_LEFT: 左に歩く (duration: 秒数)  
- RUN_RIGHT: 右に走る (duration: 秒数)
- RUN_LEFT: 左に走る (duration: 秒数)
- STEP_RIGHT: 右に一歩 (duration: 0.3)
- STEP_LEFT: 左に一歩 (duration: 0.3)

ジャンプ系:
- HOP: 小ジャンプ、その場で軽く跳ぶ (duration: 0.5)
- JUMP: 通常ジャンプ、垂直に跳ぶ (duration: 0.8)
- HIGH_JUMP: 高ジャンプ、高く垂直に跳ぶ (duration: 1.0)
- LONG_JUMP_RIGHT: 右方向にジャンプ、右に飛ぶ (duration: 1.0)
- LONG_JUMP_LEFT: 左方向にジャンプ、左に飛ぶ (duration: 1.0)

その他:
- WAIT: 待機 (duration: 秒数)
- SLIDE_RIGHT: 右にスライド (duration: 秒数)
- SLIDE_LEFT: 左にスライド (duration: 秒数)

【重要なルール】
- 「右にジャンプ」「右に飛んで」「右上にジャンプ」→ LONG_JUMP_RIGHT を使う
- 「左にジャンプ」「左に飛んで」「左上にジャンプ」→ LONG_JUMP_LEFT を使う
- 「ジャンプ」だけ（方向指定なし）→ JUMP を使う
- 方向を指定されたら必ずLONG_JUMP_RIGHT/LEFTを使うこと！

【応答形式】
必ずJSON配列のみで応答してください。説明文は不要です。
[{""action"": ""アクション名"", ""duration"": 秒数}, ...]

【例】
入力: 「右に歩いて」
出力: [{""action"": ""WALK_RIGHT"", ""duration"": 2.0}]

入力: 「右にジャンプ」
出力: [{""action"": ""LONG_JUMP_RIGHT"", ""duration"": 1.0}]

入力: 「右に飛んで」
出力: [{""action"": ""LONG_JUMP_RIGHT"", ""duration"": 1.0}]

入力: 「ジャンプして」
出力: [{""action"": ""JUMP"", ""duration"": 0.8}]

入力: 「ジャンプしてから右に走って」  
出力: [{""action"": ""JUMP"", ""duration"": 0.8}, {""action"": ""RUN_RIGHT"", ""duration"": 3.0}]

入力: 「右に大きくジャンプして」
出力: [{""action"": ""LONG_JUMP_RIGHT"", ""duration"": 1.0}]

入力: 「少し待ってから左に歩いて」
出力: [{""action"": ""WAIT"", ""duration"": 1.0}, {""action"": ""WALK_LEFT"", ""duration"": 2.0}]";

            var contents = new List<object>();
            contents.Add(new { role = "user", parts = new[] { new { text = systemPrompt } } });
            contents.Add(new { role = "model", parts = new[] { new { text = "Understood. I will respond only with JSON arrays of actions." } } });
            
            foreach (var msg in history)
            {
                contents.Add(new { role = msg.role == "user" ? "user" : "model", parts = new[] { new { text = msg.content } } });
            }

            var requestBody = new { contents = contents };
            string jsonBody = JsonUtility.ToJson(new GeminiRequest { contents = ConvertToGeminiFormat(history, systemPrompt) });
            
            // Use simple JSON building since JsonUtility has limitations
            jsonBody = BuildGeminiRequestJson(history, systemPrompt);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Gemini API Error: {request.error}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    onError?.Invoke($"API Error: {request.error} - {request.downloadHandler.text}");
                }
                else
                {
                    // Parse Gemini response
                    string response = request.downloadHandler.text;
                    Debug.Log($"Gemini Raw Response: {response}");
                    string actionJson = ExtractActionJson(response);
                    Debug.Log($"Extracted Action JSON: {actionJson}");
                    onSuccess?.Invoke(actionJson);
                }
            }
        }

        private string BuildGeminiRequestJson(List<ChatMessage> history, string systemPrompt)
        {
            var sb = new StringBuilder();
            sb.Append("{\"contents\":[");
            
            // System prompt as first user message
            sb.Append("{\"role\":\"user\",\"parts\":[{\"text\":\"").Append(EscapeJson(systemPrompt)).Append("\"}]},");
            sb.Append("{\"role\":\"model\",\"parts\":[{\"text\":\"Understood. I will respond only with JSON arrays of actions.\"}]},");
            
            for (int i = 0; i < history.Count; i++)
            {
                var msg = history[i];
                string role = msg.role == "user" ? "user" : "model";
                sb.Append("{\"role\":\"").Append(role).Append("\",\"parts\":[{\"text\":\"").Append(EscapeJson(msg.content)).Append("\"}]}");
                if (i < history.Count - 1) sb.Append(",");
            }
            
            sb.Append("]}");
            return sb.ToString();
        }

        private string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private string ExtractActionJson(string geminiResponse)
        {
            // Gemini response format: {"candidates":[{"content":{"parts":[{"text":"..."}]}}]}
            try
            {
                Debug.Log($"Parsing Gemini response...");
                
                // Find the text content within the response
                int textIndex = geminiResponse.IndexOf("\"text\"");
                if (textIndex < 0)
                {
                    Debug.LogWarning("No 'text' field found in response");
                    return "[]";
                }
                
                // Extract the text value - find the content between quotes after "text":"
                int colonIndex = geminiResponse.IndexOf(":", textIndex);
                int textStart = geminiResponse.IndexOf("\"", colonIndex + 1) + 1;
                
                // Find the end of the text value (complex due to escaping)
                int textEnd = textStart;
                bool inEscape = false;
                for (int i = textStart; i < geminiResponse.Length; i++)
                {
                    if (inEscape)
                    {
                        inEscape = false;
                        continue;
                    }
                    if (geminiResponse[i] == '\\')
                    {
                        inEscape = true;
                        continue;
                    }
                    if (geminiResponse[i] == '"')
                    {
                        textEnd = i;
                        break;
                    }
                }
                
                string text = geminiResponse.Substring(textStart, textEnd - textStart);
                Debug.Log($"Raw text extracted: {text}");
                
                // Unescape the text
                text = text.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                Debug.Log($"Unescaped text: {text}");
                
                // Remove markdown code blocks if present (```json ... ```)
                if (text.Contains("```"))
                {
                    int codeStart = text.IndexOf("```");
                    int codeEnd = text.LastIndexOf("```");
                    if (codeEnd > codeStart)
                    {
                        // Find the actual content after ```json or ```
                        int contentStart = text.IndexOf("\n", codeStart);
                        if (contentStart < 0) contentStart = codeStart + 3;
                        text = text.Substring(contentStart, codeEnd - contentStart).Trim();
                    }
                }
                
                // Extract JSON array from text
                int jsonStart = text.IndexOf("[");
                int jsonEnd = text.LastIndexOf("]") + 1;
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    string json = text.Substring(jsonStart, jsonEnd - jsonStart);
                    Debug.Log($"Final JSON: {json}");
                    return json;
                }
                
                Debug.LogWarning("No JSON array found in text");
                return "[]";
            }
            catch (Exception e)
            {
                Debug.LogError($"ExtractActionJson error: {e.Message}");
                return "[]";
            }
        }

        private object ConvertToGeminiFormat(List<ChatMessage> history, string systemPrompt) { return null; }
    }

    [Serializable]
    public class GeminiRequest { public object contents; }

    public class LLMService : MonoBehaviour
    {
        public static LLMService Instance { get; private set; }

        public event Action<List<AIActionResponse>> OnActionReceived;
        public event Action<string> OnError;

        private IModelStrategy currentStrategy;
        private List<ChatMessage> conversationHistory = new List<ChatMessage>();
        
        [Header("Settings")]
        public string apiKey = "YOUR_API_KEY"; // Loaded from config
        public bool useProModel = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadApiKey();
                SetStrategy(new StandardStrategy(apiKey));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadApiKey()
        {
            TextAsset configFile = Resources.Load<TextAsset>("config");
            if (configFile != null)
            {
                ConfigData config = JsonUtility.FromJson<ConfigData>(configFile.text);
                if (config != null && !string.IsNullOrEmpty(config.geminiApiKey))
                {
                    apiKey = config.geminiApiKey;
                    Debug.Log("API Key loaded from config.json");
                }
            }
            else
            {
                Debug.LogWarning("config.json not found in Resources. Using default API key.");
            }
        }

        public void SetStrategy(IModelStrategy strategy)
        {
            currentStrategy = strategy;
        }

        public void SendUserMessage(string userText, string context = "")
        {
            // Inject System/Memory context if starting fresh?
            // For now just append user message
            conversationHistory.Add(new ChatMessage { role = "user", content = userText });

            StartCoroutine(currentStrategy.SendRequest(conversationHistory, HandleSuccess, HandleError));
        }

        private void HandleSuccess(string jsonResponse)
        {
            // Add assistant response to history
            conversationHistory.Add(new ChatMessage { role = "model", content = jsonResponse });

            try
            {
                // Parse JSON Array of Actions
                // Simple parsing wrappers needed for Unity JsonUtility depending on format
                // Assuming the model returns a direct JSON array string
                var actions = ParseActions(jsonResponse);
                OnActionReceived?.Invoke(actions);
            }
            catch (Exception e)
            {
                HandleError($"Parse Error: {e.Message}");
            }
        }

        private void HandleError(string errorMsg)
        {
            Debug.LogError($"LLM Error: {errorMsg}");
            OnError?.Invoke(errorMsg);
        }

        private List<AIActionResponse> ParseActions(string json)
        {
            // Unity JsonUtility doesn't support top-level arrays directly.
            // Wrapping it or using a simple helper.
            // Hacky workaround for top-level array: wrap in object
            string wrapped = "{\"items\":" + json + "}";
            if (!json.TrimStart().StartsWith("["))
            {
                // If the model returns text + json, we need to extract the JSON block.
                // For this mock, assume pure JSON or handle it later.
                return new List<AIActionResponse>(); 
            }
            
            Wrapper<AIActionResponse> wrapper = JsonUtility.FromJson<Wrapper<AIActionResponse>>(wrapped);
            return wrapper.items;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
    }
}