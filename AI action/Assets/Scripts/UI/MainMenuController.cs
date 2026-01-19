using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace AIAction.UI
{
    public class MainMenuController : MonoBehaviour
    {
        private void Start()
        {
            CreateMenuUI();
        }

        private void CreateMenuUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("MenuCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create Title Text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "AI ACTION";
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.font = TMP_Settings.defaultFontAsset;
            titleText.color = Color.white;
            
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.7f);
            titleRt.anchorMax = new Vector2(0.5f, 0.7f);
            titleRt.sizeDelta = new Vector2(600, 100);

            // Create Start Button
            GameObject buttonObj = new GameObject("StartButton");
            buttonObj.transform.SetParent(canvasObj.transform, false);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnStartClicked);
            
            RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();
            buttonRt.anchorMin = new Vector2(0.5f, 0.4f);
            buttonRt.anchorMax = new Vector2(0.5f, 0.4f);
            buttonRt.sizeDelta = new Vector2(200, 60);

            // Button Text
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "START";
            buttonText.fontSize = 36;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.font = TMP_Settings.defaultFontAsset;
            buttonText.color = Color.white;
            
            RectTransform textRt = buttonTextObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            // Create EventSystem if needed
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        public void OnStartClicked()
        {
            Debug.Log("Starting Game...");
            SceneManager.LoadScene("SampleScene");
        }
    }
}
