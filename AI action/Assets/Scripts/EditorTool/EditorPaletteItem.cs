using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace AIAction.EditorTool
{
    public enum BrushType { Tile, Object }

    /// <summary>
    /// Data class for palette items (tiles or prefabs)
    /// </summary>
    [System.Serializable]
    public class EditorPaletteItem
    {
        public string itemName;
        public string itemId; // For save/load identification
        public BrushType brushType;
        public Sprite previewSprite;
        
        [Header("Tile Mode")]
        public TileBase tile;
        
        [Header("Object Mode")]
        public GameObject prefab;
    }
}
#endif