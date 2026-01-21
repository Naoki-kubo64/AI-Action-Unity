using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace AIAction.EditorTool
{
    /// <summary>
    /// Handles saving and loading level data to/from JSON
    /// </summary>
    public class LevelSerializer : MonoBehaviour
    {
        [SerializeField] private LevelEditorManager editorManager;
        [SerializeField] private List<EditorPaletteItem> paletteRegistry;

        private string SavePath => Path.Combine(Application.persistentDataPath, "Levels");

        private void Awake()
        {
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);
        }

        public void SaveLevel(string levelName)
        {
            if (editorManager == null)
            {
                Debug.LogError("LevelSerializer: EditorManager not assigned!");
                return;
            }

            LevelData data = new LevelData { levelName = levelName };

            // Save tiles
            Tilemap tilemap = editorManager.GetGroundTilemap();
            if (tilemap != null)
            {
                BoundsInt bounds = tilemap.cellBounds;
                foreach (Vector3Int pos in bounds.allPositionsWithin)
                {
                    TileBase tile = tilemap.GetTile(pos);
                    if (tile != null)
                    {
                        string tileId = GetTileId(tile);
                        if (!string.IsNullOrEmpty(tileId))
                        {
                            data.tiles.Add(new TileInfo(pos.x, pos.y, tileId));
                        }
                    }
                }
            }

            // Save objects
            var placedObjects = editorManager.GetPlacedObjects();
            foreach (var kvp in placedObjects)
            {
                if (kvp.Value == null) continue;
                string prefabId = GetPrefabId(kvp.Value);
                if (!string.IsNullOrEmpty(prefabId))
                {
                    Vector3 pos = kvp.Value.transform.position;
                    data.objects.Add(new ObjectInfo(pos.x, pos.y, prefabId));
                }
            }

            // Write to file
            string json = JsonUtility.ToJson(data, true);
            string filePath = Path.Combine(SavePath, $"{levelName}.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"Level saved to: {filePath}");
        }

        public void LoadLevel(string levelName)
        {
            string filePath = Path.Combine(SavePath, $"{levelName}.json");
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Level file not found: {filePath}");
                return;
            }

            string json = File.ReadAllText(filePath);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            // Clear current level
            Tilemap tilemap = editorManager.GetGroundTilemap();
            if (tilemap != null) tilemap.ClearAllTiles();
            editorManager.ClearAllPlacedObjects();

            // Load tiles
            if (tilemap != null)
            {
                foreach (var tileInfo in data.tiles)
                {
                    TileBase tile = GetTileById(tileInfo.tileId);
                    if (tile != null)
                    {
                        tilemap.SetTile(new Vector3Int(tileInfo.x, tileInfo.y, 0), tile);
                    }
                }
            }

            // Load objects
            foreach (var objInfo in data.objects)
            {
                GameObject prefab = GetPrefabById(objInfo.prefabId);
                if (prefab != null)
                {
                    Vector3 pos = new Vector3(objInfo.x, objInfo.y, 0);
                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
                    Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(objInfo.x), Mathf.RoundToInt(objInfo.y), 0);
                    editorManager.RegisterPlacedObject(gridPos, obj);
                }
            }

            Debug.Log($"Level loaded: {levelName}");
        }

        public string[] GetSavedLevels()
        {
            if (!Directory.Exists(SavePath)) return new string[0];
            string[] files = Directory.GetFiles(SavePath, "*.json");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            return files;
        }

        private string GetTileId(TileBase tile)
        {
            foreach (var item in paletteRegistry)
            {
                if (item.brushType == BrushType.Tile && item.tile == tile)
                    return item.itemId;
            }
            return tile.name;
        }

        private TileBase GetTileById(string id)
        {
            foreach (var item in paletteRegistry)
            {
                if (item.brushType == BrushType.Tile && item.itemId == id)
                    return item.tile;
            }
            return null;
        }

        private string GetPrefabId(GameObject obj)
        {
            string objName = obj.name.Replace("(Clone)", "").Trim();
            foreach (var item in paletteRegistry)
            {
                if (item.brushType == BrushType.Object && item.prefab != null && item.prefab.name == objName)
                    return item.itemId;
            }
            return objName;
        }

        private GameObject GetPrefabById(string id)
        {
            foreach (var item in paletteRegistry)
            {
                if (item.brushType == BrushType.Object && item.itemId == id)
                    return item.prefab;
            }
            return Resources.Load<GameObject>($"Prefabs/{id}");
        }
    }
}
#endif