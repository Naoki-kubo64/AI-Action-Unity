using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using AIAction.Core;

public class Phase4Setup : EditorWindow
{
    [MenuItem("AIAction/Setup Phase 4 UI")]
    public static void SetupUI()
    {
        // 1. Create Canvas
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
        if (Object.FindObjectOfType<EventSystem>() == null)
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
            // We create a basic input structure
            inputObj = new GameObject("PromptInput");
            inputObj.transform.SetParent(canvasObj.transform, false);
            
            // Background Image
            Image bg = inputObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            inputField = inputObj.AddComponent<TMP_InputField>();
            
            // RectTransform: Bottom stretch
            RectTransform rt = inputObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 50); // Height 50

            // Text Area
            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform areaRt = textArea.AddComponent<RectTransform>();
            areaRt.anchorMin = Vector2.zero;
            areaRt.anchorMax = Vector2.one;
            areaRt.offsetMin = new Vector2(10, 0);
            areaRt.offsetMax = new Vector2(-10, 0);
            
            // Placeholder
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Enter command...";
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholderText.fontSize = 20;
            RectTransform phRt = placeholder.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;

            // Text
            GameObject text = new GameObject("Text");
            text.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI inputText = text.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.color = Color.white;
            inputText.fontSize = 20;
            RectTransform textRt = text.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;

            inputField.textViewport = areaRt;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;

            inputField.onSubmit.AddListener((val) => { 
                Debug.Log("Submit via Editor setup event"); 
            });
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
            logText.text = "Log initialized...";
            logText.color = Color.yellow;
            logText.fontSize = 18;
            logText.alignment = TextAlignmentOptions.BottomLeft;

            RectTransform rt = logObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0.5f); // Bottom half
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(10, 60); // Above input
            rt.sizeDelta = new Vector2(-20, 0);
        }
        else
        {
            logText = logObj.GetComponent<TextMeshProUGUI>();
        }

        // 5. Link to GameController
        GameController gc = Object.FindObjectOfType<GameController>();
        if (gc != null)
        {
            Undo.RecordObject(gc, "Link UI");
            gc.promptInput = inputField;
            gc.logText = logText;
            EditorUtility.SetDirty(gc);
            Debug.Log("Linked UI to GameController");
        }
        else
        {
            Debug.LogError("GameController not found!");
        }
    }
}