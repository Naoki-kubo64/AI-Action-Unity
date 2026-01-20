using UnityEngine;
using UnityEditor;

public class Level1_2Setup : MonoBehaviour
{
    [MenuItem("Tools/Setup Level 1-2")]
    public static void SetupLevel1_2()
    {
        // Find or create Level 1-2 parent
        GameObject levelParent = GameObject.Find("Level 1-2");
        if (levelParent == null)
        {
            levelParent = new GameObject("Level 1-2");
        }

        // Starting position (separate scene, so start at 0)
        float xOffset = 0f;

        // --- GROUND PLATFORMS ---
        CreatePlatform(levelParent.transform, new Vector3(xOffset + 5f, -3f, 0f), new Vector2(10f, 1f), "Ground_1");
        CreatePlatform(levelParent.transform, new Vector3(xOffset + 20f, -3f, 0f), new Vector2(8f, 1f), "Ground_2");
        CreatePlatform(levelParent.transform, new Vector3(xOffset + 35f, -3f, 0f), new Vector2(12f, 1f), "Ground_3");

        // --- FLOATING PLATFORMS ---
        CreatePlatform(levelParent.transform, new Vector3(xOffset + 12f, -1f, 0f), new Vector2(3f, 0.5f), "Platform_1");
        CreatePlatform(levelParent.transform, new Vector3(xOffset + 15f, 1f, 0f), new Vector2(3f, 0.5f), "Platform_2");
        CreatePlatform(levelParent.transform, new Vector3(xOffset + 28f, 0f, 0f), new Vector2(4f, 0.5f), "Platform_3");

        // --- ENEMIES (using opossum sprites) ---
        CreateEnemy(levelParent.transform, new Vector3(xOffset + 8f, -2f, 0f), "Opossum_1");
        CreateEnemy(levelParent.transform, new Vector3(xOffset + 22f, -2f, 0f), "Opossum_2");
        CreateEnemy(levelParent.transform, new Vector3(xOffset + 38f, -2f, 0f), "Opossum_3");

        // --- SPIKE TRAPS ---
        CreateSpikeTrap(levelParent.transform, new Vector3(xOffset + 17f, -2.7f, 0f));

        // --- MOVING PLATFORM ---
        CreateMovingPlatform(levelParent.transform, new Vector3(xOffset + 25f, -1f, 0f), new Vector3(0f, 4f, 0f));

        // --- BREAKABLE BLOCKS ---
        CreateBreakableBlock(levelParent.transform, new Vector3(xOffset + 30f, 0f, 0f));
        CreateBreakableBlock(levelParent.transform, new Vector3(xOffset + 31f, 0f, 0f));

        // --- GOAL ---
        CreateGoal(levelParent.transform, new Vector3(xOffset + 42f, -2f, 0f));

        Debug.Log("Level 1-2 setup complete!");
    }

    private static void CreatePlatform(Transform parent, Vector3 position, Vector2 size, string name)
    {
        GameObject platform = new GameObject(name);
        platform.transform.parent = parent;
        platform.transform.position = position;
        platform.layer = LayerMask.NameToLayer("Default");

        var sr = platform.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.4f, 0.7f, 0.3f); // Grassy green

        // Try to load tileset sprite
        Sprite[] sprites = Resources.LoadAll<Sprite>("SunnyLand Artwork/Environment/Tileset/tileset-sliced");
        if (sprites != null && sprites.Length > 0)
        {
            sr.sprite = sprites[0];
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
        }

        var col = platform.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    private static void CreateEnemy(Transform parent, Vector3 position, string name)
    {
        GameObject enemy = new GameObject(name);
        enemy.transform.parent = parent;
        enemy.transform.position = position;
        enemy.tag = "Enemy";

        var sr = enemy.AddComponent<SpriteRenderer>();
        
        // Try to load opossum sprite
        Sprite opossumSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/SunnyLand Artwork/Sprites/Enemies/opossum/opossum-1.png"
        );
        if (opossumSprite != null)
        {
            sr.sprite = opossumSprite;
        }
        else
        {
            sr.color = Color.red;
        }

        var col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.5f);

        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var enemyScript = enemy.AddComponent<AIAction.Core.Enemy>();
        enemyScript.patrolDistance = 2f;
        enemyScript.patrolSpeed = 1.5f;
    }

    private static void CreateSpikeTrap(Transform parent, Vector3 position)
    {
        GameObject spike = new GameObject("SpikeTrap");
        spike.transform.parent = parent;
        spike.transform.position = position;

        var sr = spike.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.5f, 0.5f, 0.5f);

        var col = spike.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, 0.3f);
        col.isTrigger = true;

        spike.AddComponent<AIAction.Core.SpikeTrap>();
    }

    private static void CreateMovingPlatform(Transform parent, Vector3 position, Vector3 endOffset)
    {
        GameObject platform = new GameObject("MovingPlatform");
        platform.transform.parent = parent;
        platform.transform.position = position;

        var sr = platform.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.8f, 0.8f);

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

    private static void CreateGoal(Transform parent, Vector3 position)
    {
        GameObject goal = new GameObject("Goal_1-2");
        goal.transform.parent = parent;
        goal.transform.position = position;
        goal.tag = "Goal";

        var sr = goal.AddComponent<SpriteRenderer>();
        sr.color = Color.yellow;

        var col = goal.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 2f);
        col.isTrigger = true;
    }
}
