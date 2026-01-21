using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace AIAction.EditorTool
{
    /// <summary>
    /// Serializable level data for JSON save/load
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public List<TileInfo> tiles = new List<TileInfo>();
        public List<ObjectInfo> objects = new List<ObjectInfo>();
    }

    [System.Serializable]
    public class TileInfo
    {
        public int x;
        public int y;
        public string tileId;

        public TileInfo() { }
        public TileInfo(int x, int y, string tileId)
        {
            this.x = x;
            this.y = y;
            this.tileId = tileId;
        }
    }

    [System.Serializable]
    public class ObjectInfo
    {
        public float x;
        public float y;
        public string prefabId;

        public ObjectInfo() { }
        public ObjectInfo(float x, float y, string prefabId)
        {
            this.x = x;
            this.y = y;
            this.prefabId = prefabId;
        }
    }
}
#endif