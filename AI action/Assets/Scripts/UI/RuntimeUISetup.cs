using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using AIAction.Core;

namespace AIAction.UI
{
    [ExecuteInEditMode]
    public class RuntimeUISetup : MonoBehaviour
    {
        [Header("Japanese Font (drag NotoSansJP SDF here)")]
        [SerializeField] private TMP_FontAsset japaneseFont;

        private void Awake()
        {
            // If not assigned in Inspector, try loading from various paths
            if (japaneseFont == null)
            {
                japaneseFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-VariableFont_wght SDF");
            }
            if (japaneseFont == null)
            {
                // Try direct path
                japaneseFont = Resources.Load<TMP_FontAsset>("NotoSansJP-VariableFont_wght SDF");
            }
            if (japaneseFont == null)
            {
                Debug.LogWarning("Japanese font not found! Using default font.");
                japaneseFont = TMP_Settings.defaultFontAsset;
            }
            else
            {
                Debug.Log($"Japanese font loaded: {japaneseFont.name}");
            }
            SetupUI();
        }

        public void SetupUI()
        {
            // 1. Create Canvas if missing
            GameObject canvasObj = GameObject.Find("Canvas");
            Canvas canvas;
            if (canvasObj == null)
            {
                canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvas = canvasObj.GetComponent<Canvas>();
            }

            // 2. EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 3. Prompt Input (TMP)
            GameObject inputObj = GameObject.Find("PromptInput");
            TMP_InputField inputField;
            if (inputObj == null)
            {
                inputObj = new GameObject("PromptInput");
                inputObj.transform.SetParent(canvasObj.transform, false);
                
                Image bg = inputObj.AddComponent<Image>();
                bg.color = new Color(0, 0, 0, 0.8f);

                inputField = inputObj.AddComponent<TMP_InputField>();
                
                RectTransform rt = inputObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.anchoredPosition = new Vector2(0, 0);
                rt.sizeDelta = new Vector2(0, 50);
                rt.offsetMin = new Vector2(0, 0);
                rt.offsetMax = new Vector2(0, 50);

                // Text Area logic... slightly complex for runtime without prefab, 
                // but we will create minimal functional structure.
                GameObject textArea = new GameObject("TextArea");
                textArea.transform.SetParent(inputObj.transform, false);
                RectTransform areaRt = textArea.AddComponent<RectTransform>();
                areaRt.anchorMin = Vector2.zero;
                areaRt.anchorMax = Vector2.one;
                areaRt.offsetMin = new Vector2(10, 5);
                areaRt.offsetMax = new Vector2(-10, -5);
                
                // Add RectMask2D to clip text
                textArea.AddComponent<UnityEngine.UI.RectMask2D>();
                
                // Text
                GameObject text = new GameObject("Text");
                text.transform.SetParent(textArea.transform, false);
                TextMeshProUGUI inputText = text.AddComponent<TextMeshProUGUI>();
                inputText.color = Color.white;
                inputText.fontSize = 24;
                inputText.font = japaneseFont;
                
                // Set RectTransform for text
                RectTransform textRt = text.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(10, 5);
                textRt.offsetMax = new Vector2(-10, -5);
                
                inputField.textViewport = areaRt;
                inputField.textComponent = inputText;
            }
            else
            {
                inputField = inputObj.GetComponent<TMP_InputField>();
            }

            // 4. Log Text (TMP)
            GameObject logObj = GameObject.Find("LogText");
            TextMeshProUGUI logText;
            if (logObj == null)
            {
                logObj = new GameObject("LogText");
                logObj.transform.SetParent(canvasObj.transform, false);
                logText = logObj.AddComponent<TextMeshProUGUI>();
                logText.color = Color.yellow;
                logText.fontSize = 20;
                logText.alignment = TextAlignmentOptions.BottomLeft;
                // Use Japanese font for log to support AI response text
                logText.font = japaneseFont != null ? japaneseFont : TMP_Settings.defaultFontAsset;

                RectTransform rt = logObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(20, 60); // Padding left/bottom
                rt.offsetMax = new Vector2(-20, -20);
            }
            else
            {
                logText = logObj.GetComponent<TextMeshProUGUI>();
            }

            // 5. Link to GameController
            GameController gc = GetComponent<GameController>();
            if (gc == null) gc = FindObjectOfType<GameController>();
            
            if (gc != null)
            {
                gc.promptInput = inputField;
                gc.logText = logText;
                
                // Manually trigger listener registration since Start might have passed
                inputField.onSubmit.RemoveListener(gc.OnPromptSubmit);
                inputField.onSubmit.AddListener(gc.OnPromptSubmit);
                
                Debug.Log("UI Linked to GameController successfully.");
            }
            else
            {
                Debug.LogError("RuntimeUISetup: GameController not found!");
            }
        }
    }
}