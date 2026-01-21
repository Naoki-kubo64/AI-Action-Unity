using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace AIAction.EditorTool
{
    /// <summary>
    /// Runtime Level Editor Manager - Development Build Only
    /// Handles edit/play mode switching, grid-based placement, and brush management.
    /// </summary>
    public class LevelEditorManager : MonoBehaviour
    {
        public static LevelEditorManager Instance { get; private set; }
        public static bool IsEditing => Instance != null && Instance.currentState == EditorState.Editing;

        public enum EditorState { Playing, Editing }

        [Header("References")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Transform objectContainer;
        [SerializeField] private GameObject editorUI;
        [SerializeField] private SpriteRenderer gridCursor;

        [Header("Settings")]
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private Color cursorValidColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color cursorInvalidColor = new Color(1f, 0f, 0f, 0.5f);

        [Header("Palette")]
        [SerializeField] private List<EditorPaletteItem> paletteItems = new List<EditorPaletteItem>();

        // Current state
        private EditorState currentState = EditorState.Playing;
        private EditorPaletteItem currentBrush;
        private Camera mainCamera;
        private Dictionary<Vector3Int, GameObject> placedObjects = new Dictionary<Vector3Int, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        private void Start()
        {
            // Start in Playing mode
            SetState(EditorState.Playing);
        }

        private void Update()
        {
            // Toggle editor with F1 key
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleEditorMode();
            }

            if (currentState != EditorState.Editing) return;

            UpdateGridCursor();
            HandleInput();
        }

        #region State Management

        public void ToggleEditorMode()
        {
            SetState(currentState == EditorState.Playing ? EditorState.Editing : EditorState.Playing);
        }

        public void SetState(EditorState newState)
        {
            currentState = newState;

            if (newState == EditorState.Editing)
            {
                Time.timeScale = 0f;
                if (editorUI) editorUI.SetActive(true);
                if (gridCursor) gridCursor.gameObject.SetActive(true);
                DisableGameplay();
            }
            else // Playing
            {
                Time.timeScale = 1f;
                if (editorUI) editorUI.SetActive(false);
                if (gridCursor) gridCursor.gameObject.SetActive(false);
                EnableGameplay();
            }

            Debug.Log($"LevelEditor: State changed to {newState}");
        }

        private void DisableGameplay()
        {
            // Disable player input
            var player = FindObjectOfType<Core.PlayerActionController>();
            if (player) player.enabled = false;

            // Disable enemies
            var enemies = FindObjectsOfType<Core.Enemy>();
            foreach (var enemy in enemies)
            {
                enemy.enabled = false;
            }
        }

        private void EnableGameplay()
        {
            // Enable player input
            var player = FindObjectOfType<Core.PlayerActionController>();
            if (player) player.enabled = true;

            // Enable enemies
            var enemies = FindObjectsOfType<Core.Enemy>();
            foreach (var enemy in enemies)
            {
                enemy.enabled = true;
            }
        }

        #endregion

        #region Grid System

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

        private Vector3Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.RoundToInt(worldPos.x / gridSize),
                Mathf.RoundToInt(worldPos.y / gridSize),
                0
            );
        }

        private Vector3 GridToWorld(Vector3Int gridPos)
        {
            return new Vector3(
                gridPos.x * gridSize,
                gridPos.y * gridSize,
                0
            );
        }

        private void UpdateGridCursor()
        {
            if (!gridCursor) return;

            Vector3 worldPos = GetMouseWorldPosition();
            Vector3Int gridPos = WorldToGrid(worldPos);
            gridCursor.transform.position = GridToWorld(gridPos);

            // Update cursor color based on validity
            bool canPlace = CanPlaceAt(gridPos);
            gridCursor.color = canPlace ? cursorValidColor : cursorInvalidColor;

            // Update cursor sprite if brush selected
            if (currentBrush != null && currentBrush.previewSprite != null)
            {
                gridCursor.sprite = currentBrush.previewSprite;
            }
        }

        private bool CanPlaceAt(Vector3Int gridPos)
        {
            if (currentBrush == null) return false;

            if (currentBrush.brushType == BrushType.Tile)
            {
                return groundTilemap != null;
            }
            else // Object
            {
                return !placedObjects.ContainsKey(gridPos);
            }
        }

        #endregion

        #region Placement Logic

        private void HandleInput()
        {
            Vector3 worldPos = GetMouseWorldPosition();
            Vector3Int gridPos = WorldToGrid(worldPos);

            // Left click/drag - Place
            if (Input.GetMouseButton(0))
            {
                PlaceAt(gridPos);
            }
            // Right click/drag - Delete
            else if (Input.GetMouseButton(1))
            {
                DeleteAt(gridPos);
            }
        }

        private void PlaceAt(Vector3Int gridPos)
        {
            if (currentBrush == null) return;

            if (currentBrush.brushType == BrushType.Tile)
            {
                PlaceTile(gridPos);
            }
            else
            {
                PlaceObject(gridPos);
            }
        }

        private void PlaceTile(Vector3Int gridPos)
        {
            if (groundTilemap == null || currentBrush.tile == null) return;

            groundTilemap.SetTile(gridPos, currentBrush.tile);
        }

        private void PlaceObject(Vector3Int gridPos)
        {
            if (currentBrush.prefab == null) return;
            if (placedObjects.ContainsKey(gridPos)) return;

            Vector3 worldPos = GridToWorld(gridPos);
            GameObject obj = Instantiate(currentBrush.prefab, worldPos, Quaternion.identity);
            
            if (objectContainer != null)
            {
                obj.transform.SetParent(objectContainer);
            }

            placedObjects[gridPos] = obj;
        }

        private void DeleteAt(Vector3Int gridPos)
        {
            // Delete tile
            if (groundTilemap != null)
            {
                groundTilemap.SetTile(gridPos, null);
            }

            // Delete object
            if (placedObjects.TryGetValue(gridPos, out GameObject obj))
            {
                if (obj != null) Destroy(obj);
                placedObjects.Remove(gridPos);
            }
        }

        #endregion

        #region Brush Management

        public void SetBrush(EditorPaletteItem item)
        {
            currentBrush = item;
            Debug.Log($"LevelEditor: Brush set to {item?.itemName ?? "None"}");
        }

        public void ClearBrush()
        {
            currentBrush = null;
        }

        public EditorPaletteItem GetCurrentBrush() => currentBrush;
        public List<EditorPaletteItem> GetPaletteItems() => paletteItems;

        #endregion

        #region Public API for Serialization

        public Tilemap GetGroundTilemap() => groundTilemap;
        public Dictionary<Vector3Int, GameObject> GetPlacedObjects() => placedObjects;

        public void ClearAllPlacedObjects()
        {
            foreach (var kvp in placedObjects)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            placedObjects.Clear();
        }

        public void RegisterPlacedObject(Vector3Int gridPos, GameObject obj)
        {
            placedObjects[gridPos] = obj;
        }

        #endregion
    }
}
#endif
