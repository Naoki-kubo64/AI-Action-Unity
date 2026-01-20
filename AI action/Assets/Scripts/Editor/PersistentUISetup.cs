using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class PersistentUISetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Persistent UI (All Scenes)")]
    public static void SetupPersistentUI()
    {
        // Create UI Canvas
        GameObject canvasGO = new GameObject("PersistentUI");
        
        // Add PersistentUIManager
        canvasGO.AddComponent<AIAction.Core.PersistentUIManager>();
        
        // Add Canvas
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Always on top
        
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create input panel at bottom
        GameObject inputPanel = CreateInputPanel(canvasGO.transform);
        
        // Create GameController if not exists
        var gameController = FindFirstObjectByType<AIAction.Core.GameController>();
        if (gameController == null)
        {
            GameObject gcGO = new GameObject("GameController");
            gcGO.AddComponent<AIAction.Core.GameController>();
            Debug.Log("GameController created");
        }
        
        // Link input field to GameController
        var inputField = inputPanel.GetComponentInChildren<TMP_InputField>();
        if (inputField == null)
        {
            inputField = inputPanel.GetComponentInChildren<TMPro.TMP_InputField>();
        }
        
        Debug.Log("Persistent UI setup complete! This UI will persist across all scene loads.");
    }

    private static GameObject CreateInputPanel(Transform parent)
    {
        // Panel background
        GameObject panel = new GameObject("InputPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 10);
        panelRect.sizeDelta = new Vector2(-20, 60);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Horizontal layout
        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 10;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        
        // Input field
        GameObject inputFieldGO = new GameObject("PromptInput");
        inputFieldGO.transform.SetParent(panel.transform, false);
        
        RectTransform inputRect = inputFieldGO.AddComponent<RectTransform>();
        
        Image inputBg = inputFieldGO.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        TMP_InputField inputField = inputFieldGO.AddComponent<TMP_InputField>();
        
        // Text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputFieldGO.transform, false);
        
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);
        
        textArea.AddComponent<RectMask2D>();
        
        // Placeholder
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        
        RectTransform phRect = placeholder.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI phText = placeholder.AddComponent<TextMeshProUGUI>();
        phText.text = "Enter command... (Press Enter)";
        phText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        phText.fontSize = 18;
        phText.alignment = TextAlignmentOptions.MidlineLeft;
        
        // Input text
        GameObject inputText = new GameObject("Text");
        inputText.transform.SetParent(textArea.transform, false);
        
        RectTransform itRect = inputText.AddComponent<RectTransform>();
        itRect.anchorMin = Vector2.zero;
        itRect.anchorMax = Vector2.one;
        itRect.offsetMin = Vector2.zero;
        itRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI itText = inputText.AddComponent<TextMeshProUGUI>();
        itText.color = Color.white;
        itText.fontSize = 18;
        itText.alignment = TextAlignmentOptions.MidlineLeft;
        
        // Configure input field
        inputField.textViewport = textAreaRect;
        inputField.textComponent = itText;
        inputField.placeholder = phText;
        
        // Layout element for sizing
        LayoutElement layoutElement = inputFieldGO.AddComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1;
        
        // Send button
        GameObject sendButton = new GameObject("SendButton");
        sendButton.transform.SetParent(panel.transform, false);
        
        Image btnImage = sendButton.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        Button btn = sendButton.AddComponent<Button>();
        
        LayoutElement btnLayout = sendButton.AddComponent<LayoutElement>();
        btnLayout.minWidth = 80;
        btnLayout.preferredWidth = 80;
        
        // Button text
        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(sendButton.transform, false);
        
        RectTransform btnTextRect = btnTextGO.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.text = "Send";
        btnText.color = Color.white;
        btnText.fontSize = 20;
        btnText.alignment = TextAlignmentOptions.Center;
        
        return panel;
    }
}
