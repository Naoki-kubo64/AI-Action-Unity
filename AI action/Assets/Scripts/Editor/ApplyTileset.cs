using UnityEngine;
using UnityEditor;

public class ApplyTileset : MonoBehaviour
{
    [MenuItem("Tools/Apply SunnyLand Tileset to Platforms")]
    public static void ApplyTilesetSprites()
    {
        // Load tileset sprites
        string tilesetPath = "Assets/SunnyLand Artwork/Environment/Tileset/tileset-sliced.png";
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(tilesetPath);
        
        Sprite grassTop = null;
        Sprite grassMiddle = null;
        Sprite dirt = null;
        
        foreach (var obj in sprites)
        {
            if (obj is Sprite sprite)
            {
                // Grass top tiles (use tile index 43 - grass top)
                if (sprite.name == "tileset-sliced_43")
                    grassTop = sprite;
                // Middle/dirt tile (use tile index 59)
                if (sprite.name == "tileset-sliced_59")
                    grassMiddle = sprite;
                // Pure dirt (use tile index 75)
                if (sprite.name == "tileset-sliced_75")
                    dirt = sprite;
            }
        }

        if (grassTop == null)
        {
            Debug.LogError("Could not find tileset sprites!");
            return;
        }

        // Find all platform objects
        string[] platformNames = { "Ground_1", "Ground_2", "Ground_3", "Platform_1", "Platform_2", "Platform_3" };
        
        foreach (string name in platformNames)
        {
            var obj = GameObject.Find(name);
            if (obj == null)
            {
                obj = FindDeep(name);
            }
            
            if (obj != null)
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = grassTop;
                    sr.drawMode = SpriteDrawMode.Tiled;
                    
                    // Get box collider size for tiling
                    var col = obj.GetComponent<BoxCollider2D>();
                    if (col != null)
                    {
                        sr.size = col.size;
                    }
                    
                    Debug.Log($"Applied tileset to {name}");
                }
            }
        }

        Debug.Log("Tileset application complete!");
    }

    private static GameObject FindDeep(string name)
    {
        // Search in all root objects
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var found = FindInChildren(root.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
