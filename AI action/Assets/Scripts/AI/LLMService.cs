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
プレイヤーの自然言語指示を解釈し、適切なアクションのJSON配列で応答してください。

【究極アクションリスト】

■基本移動:
- CREEP_RIGHT / CREEP_LEFT: 忍び足、そっと、ゆっくり
- WALK_RIGHT / WALK_LEFT: 歩く、散歩
- RUN_RIGHT / RUN_LEFT: 走る、駆ける、急ぐ
- DASH_RIGHT / DASH_LEFT: ダッシュ、全力、猛ダッシュ
- STEP_RIGHT / STEP_LEFT: 一歩、ちょっと移動
- STOP: 止まる、ストップ、待て
- WAIT: 待機、じっとする

■基本ジャンプ:
- HOP: 小ジャンプ、ぴょん、軽く
- JUMP: ジャンプ、飛ぶ、跳ねる
- HIGH_JUMP: 高くジャンプ、大ジャンプ、思いっきり飛ぶ
- FALL: 落下、着地、降りる、落ちる

■方向ジャンプ (飛び移る):
- JUMP_RIGHT_SHORT: 右に軽くジャンプ
- JUMP_RIGHT_MEDIUM: 右にジャンプ、右へ飛ぶ
- JUMP_RIGHT_LONG: 右に大きくジャンプ、右へ大ジャンプ、右に遠くへ
- JUMP_LEFT_SHORT: 左に軽くジャンプ  
- JUMP_LEFT_MEDIUM: 左にジャンプ、左へ飛ぶ
- JUMP_LEFT_LONG: 左に大きくジャンプ、左へ大ジャンプ、左に遠くへ

■高度な機動:
- AIR_DASH_RIGHT / LEFT: 空中ダッシュ、エアダッシュ、空中で飛ぶ
- WALL_JUMP: 壁キック、壁蹴り、壁ジャンプ
- WALL_SLIDE: 壁ずり、壁につかまる、壁を滑る
- STOMP: 急降下、踏みつけ、ヒップドロップ、地面を叩く

■姿勢制御:
- CROUCH: しゃがむ、かがむ、屈む、低姿勢
- CRAWL_RIGHT / LEFT: 匍匐、はいはい、這う
- SLIDE_RIGHT / LEFT: スライディング、滑り込む

■戦闘・アクション:
- ATTACK: 攻撃、殴る、斬る、叩く、パンチ、キック
- SHOOT: 銃を撃つ、発砲、撃て、射撃、撃つ
- GUARD: 防御、ガード、守る、防ぐ
- DODGE_ROLL: 回避、避ける、ローリング、転がる
- INTERACT: 調べる、触る、拾う、押す、使う
- BREAK_OBJECT: 壊す、破壊、砕く

【同義語マッピング（重要）】
以下の言葉は対応するアクションに変換してください:
- 「進め」「行け」「向かえ」= RUN (方向が不明なら右)
- 「飛べ」「跳べ」= JUMP
- 「大きく飛べ」「遠くへ飛べ」= JUMP_LONG
- 「着地」「降りろ」= FALL (duration 0で瞬時)
- 「止まれ」「ストップ」= STOP
- 「様子を見ろ」「待て」= WAIT
- 「避けろ」「よけろ」= DODGE_ROLL
- 「やっつけろ」「倒せ」= ATTACK

【曖昧な指示の解釈ルール】
1. 方向が不明な場合: デフォルトは「右」
2. 距離が不明な場合: MEDIUM を使う
3. 「～して、～して」の連続指示: 複数アクションのシーケンスに変換
4. 未知の動詞: 最も近いアクションを推測で選ぶ
5. 「大きく」「思いっきり」= LONG / HIGH バリエーション
6. 「ちょっと」「軽く」= SHORT / HOP バリエーション

【空間参照の解釈（重要）】
プレイヤーは右を向いているのがデフォルトです。
- 「目の前の○○」「前方の○○」= 向いている方向（右）
- 「後ろの○○」「背後の○○」= 反対方向（左）
- 「上の○○」「頭上の○○」= ジャンプが必要
- 「下の○○」「足元の○○」= STOMPまたはCROUCH
- 「あの○○」「そこの○○」= 右方向と推測
- 「近くの○○」= 移動距離SHORT
- 「遠くの○○」= 移動距離LONG

○○の種類に応じたアクション:
- 敵、モンスター → ATTACK または DODGE_ROLL
- 壁 → WALL_JUMP または WALL_SLIDE
- 穴、崖 → JUMP系
- アイテム、宝箱 → INTERACT
- 障害物、ブロック → BREAK_OBJECT または ジャンプで回避

例:
- 「目の前の敵を倒せ」→ [{""action"": ""ATTACK"", ""duration"": 0.3}]
- 「上の足場に乗れ」→ [{""action"": ""HIGH_JUMP"", ""duration"": 0.5}]
- 「後ろに下がれ」→ [{""action"": ""RUN_LEFT"", ""duration"": 1.0}]
- 「あの穴を飛び越えろ」→ [{""action"": ""JUMP_RIGHT_LONG"", ""duration"": 0.8}]
- 「近くのアイテムを拾え」→ [{""action"": ""STEP_RIGHT"", ""duration"": 0.3}, {""action"": ""INTERACT"", ""duration"": 0.2}]

【応答形式】
[{""action"": ""アクション名"", ""duration"": 秒数}, ...]

duration目安:
- 瞬時アクション (JUMP, ATTACK, STOMP): 0.3~0.5秒
- 移動アクション: 0.5~2.0秒
- 待機系: 1.0~3.0秒

【例】
入力: 「右に大きくジャンプして着地して」
出力: [{""action"": ""JUMP_RIGHT_LONG"", ""duration"": 0.8}, {""action"": ""FALL"", ""duration"": 0.3}]

入力: 「向こうへ飛んでって！」
出力: [{""action"": ""JUMP_RIGHT_LONG"", ""duration"": 0.8}]

入力: 「壁を蹴って登れ！」
出力: [{""action"": ""WALL_JUMP"", ""duration"": 0.5}, {""action"": ""WALL_JUMP"", ""duration"": 0.5}]

入力: 「敵を避けて反撃！」
出力: [{""action"": ""DODGE_ROLL"", ""duration"": 0.3}, {""action"": ""ATTACK"", ""duration"": 0.3}]

入力: 「とりあえず進んで」
出力: [{""action"": ""RUN_RIGHT"", ""duration"": 2.0}]

入力: 「ちょっと待って」
出力: [{""action"": ""WAIT"", ""duration"": 1.5}]";

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