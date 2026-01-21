using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace AIAction.EditorTool
{
    /// <summary>
    /// UI Controller for the Level Editor
    /// </summary>
    public class LevelEditorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelEditorManager editorManager;
        [SerializeField] private LevelSerializer serializer;

        [Header("UI Panels")]
        [SerializeField] private GameObject palettePanel;
        [SerializeField] private Transform paletteContent;
        [SerializeField] private GameObject paletteBtnPrefab;

        [Header("Controls")]
        [SerializeField] private Button saveBtn;
        [SerializeField] private Button loadBtn;
        [SerializeField] private Button playEditToggleBtn;
        [SerializeField] private TMP_InputField levelNameInput;
        [SerializeField] private TMP_Text modeLabel;

        [Header("Load Panel")]
        [SerializeField] private GameObject loadPanel;
        [SerializeField] private Transform loadListContent;
        [SerializeField] private GameObject loadItemPrefab;

        private void Start()
        {
            if (editorManager == null) editorManager = LevelEditorManager.Instance;
            
            // Setup buttons
            if (saveBtn) saveBtn.onClick.AddListener(OnSaveClicked);
            if (loadBtn) loadBtn.onClick.AddListener(OnLoadClicked);
            if (playEditToggleBtn) playEditToggleBtn.onClick.AddListener(OnToggleClicked);

            // Build palette
            BuildPalette();
            
            // Hide load panel
            if (loadPanel) loadPanel.SetActive(false);
            
            UpdateModeLabel();
        }

        private void Update()
        {
            // Update UI in realtime (unscaled time)
        }

        private void BuildPalette()
        {
            if (editorManager == null || paletteContent == null || paletteBtnPrefab == null) return;

            // Clear existing
            foreach (Transform child in paletteContent)
                Destroy(child.gameObject);

            // Create palette buttons
            var items = editorManager.GetPaletteItems();
            foreach (var item in items)
            {
                GameObject btnObj = Instantiate(paletteBtnPrefab, paletteContent);
                btnObj.name = $"Palette_{item.itemName}";

                // Setup button
                Button btn = btnObj.GetComponent<Button>();
                Image img = btnObj.GetComponent<Image>();
                TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();

                if (img && item.previewSprite)
                    img.sprite = item.previewSprite;
                if (label)
                    label.text = item.itemName;

                // Capture for closure
                var capturedItem = item;
                if (btn)
                    btn.onClick.AddListener(() => OnPaletteItemClicked(capturedItem));
            }
        }

        private void OnPaletteItemClicked(EditorPaletteItem item)
        {
            editorManager.SetBrush(item);
            Debug.Log($"Selected: {item.itemName}");
        }

        private void OnSaveClicked()
        {
            if (serializer == null || levelNameInput == null) return;
            string levelName = levelNameInput.text;
            if (string.IsNullOrEmpty(levelName)) levelName = "CustomLevel";
            serializer.SaveLevel(levelName);
        }

        private void OnLoadClicked()
        {
            if (loadPanel == null) return;
            loadPanel.SetActive(true);
            RefreshLoadList();
        }

        private void RefreshLoadList()
        {
            if (serializer == null || loadListContent == null || loadItemPrefab == null) return;

            // Clear
            foreach (Transform child in loadListContent)
                Destroy(child.gameObject);

            // Get saved levels
            string[] levels = serializer.GetSavedLevels();
            foreach (string levelName in levels)
            {
                GameObject itemObj = Instantiate(loadItemPrefab, loadListContent);
                TMP_Text label = itemObj.GetComponentInChildren<TMP_Text>();
                Button btn = itemObj.GetComponent<Button>();

                if (label) label.text = levelName;

                string capturedName = levelName;
                if (btn)
                    btn.onClick.AddListener(() => {
                        serializer.LoadLevel(capturedName);
                        loadPanel.SetActive(false);
                    });
            }
        }

        private void OnToggleClicked()
        {
            editorManager.ToggleEditorMode();
            UpdateModeLabel();
        }

        private void UpdateModeLabel()
        {
            if (modeLabel == null) return;
            bool isEditing = LevelEditorManager.IsEditing;
            modeLabel.text = isEditing ? "EDIT MODE" : "PLAY MODE";
        }

        public void CloseLoadPanel()
        {
            if (loadPanel) loadPanel.SetActive(false);
        }
    }
}
#endif