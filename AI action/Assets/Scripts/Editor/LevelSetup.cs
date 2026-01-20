using UnityEngine;
using UnityEditor;

public class LevelSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Level 1-1 Gimmicks")]
    public static void SetupLevel()
    {
        // Find or create Gimmicks parent
        GameObject gimmicksParent = GameObject.Find("Gimmicks");
        if (gimmicksParent == null)
        {
            gimmicksParent = new GameObject("Gimmicks");
        }

        // --- ENEMY ---
        CreateEnemy(gimmicksParent.transform, new Vector3(8f, -1.5f, 0f));
        
        // --- SPIKES ---
        CreateSpike(gimmicksParent.transform, new Vector3(12f, -3f, 0f));
        
        // --- MOVING PLATFORM ---
        CreateMovingPlatform(gimmicksParent.transform, new Vector3(6f, 0f, 0f), new Vector3(0f, 3f, 0f));
        
        // --- BREAKABLE BLOCK ---
        CreateBreakableBlock(gimmicksParent.transform, new Vector3(14f, 0f, 0f));

        Debug.Log("Level 1-1 Gimmicks setup complete!");
    }

    private static void CreateEnemy(Transform parent, Vector3 position)
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.parent = parent;
        enemy.transform.position = position;
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Default");
        
        // Add components
        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.color = Color.red;
        
        var col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
        
        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        enemy.AddComponent<AIAction.Core.Enemy>();
        
        // Try to load sprite
        var sprite = Resources.Load<Sprite>("Sprites/enemy-idle");
        if (sprite != null) sr.sprite = sprite;
    }

    private static void CreateSpike(Transform parent, Vector3 position)
    {
        GameObject spike = new GameObject("SpikeTrap");
        spike.transform.parent = parent;
        spike.transform.position = position;
        
        var sr = spike.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.5f, 0.5f, 0.5f);
        
        var col = spike.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, 0.5f);
        col.isTrigger = true;
        
        spike.AddComponent<AIAction.Core.SpikeTrap>();
    }

    private static void CreateMovingPlatform(Transform parent, Vector3 position, Vector3 endOffset)
    {
        GameObject platform = new GameObject("MovingPlatform");
        platform.transform.parent = parent;
        platform.transform.position = position;
        platform.layer = LayerMask.NameToLayer("Default");
        
        var sr = platform.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.8f, 0.3f);
        
        var col = platform.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, 0.5f);
        
        var rb = platform.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        var mp = platform.AddComponent<AIAction.Core.MovingPlatform>();
        mp.endOffset = endOffset;
        mp.speed = 2f;
    }

    private static void CreateBreakableBlock(Transform parent, Vector3 position)
    {
        GameObject block = new GameObject("BreakableBlock");
        block.transform.parent = parent;
        block.transform.position = position;
        
        var sr = block.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.6f, 0.4f, 0.2f);
        
        var col = block.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
        
        block.AddComponent<AIAction.Core.BreakableBlock>();
    }

    [MenuItem("Tools/Add PlayerHealth to Player")]
    public static void AddPlayerHealth()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<AIAction.Core.PlayerHealth>() == null)
        {
            player.AddComponent<AIAction.Core.PlayerHealth>();
            Debug.Log("PlayerHealth added to Player!");
        }
    }
}
