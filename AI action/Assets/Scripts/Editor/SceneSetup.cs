using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSetup : MonoBehaviour
{
    [MenuItem("Tools/Create Level 1-2 Scene")]
    public static void CreateLevel1_2Scene()
    {
        // Ensure Scenes folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // Create new scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Setup background
        BackgroundSetup.Setup();
        
        // Setup level content
        Level1_2Setup.SetupLevel1_2();
        
        // Add Player
        CreatePlayer(new Vector3(0f, -1f, 0f));
        
        // Setup camera
        SetupCamera();
        
        // Save scene
        string scenePath = "Assets/Scenes/Level1-2.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        // Add to build settings
        AddSceneToBuildSettings(scenePath);
        
        Debug.Log("Level 1-2 Scene created and saved!");
    }

    private static void CreatePlayer(Vector3 position)
    {
        // Check if player exists
        var existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null) return;

        GameObject player = new GameObject("Player");
        player.transform.position = position;
        player.tag = "Player";

        var sr = player.AddComponent<SpriteRenderer>();
        sr.color = Color.blue;

        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        var rb = player.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        player.AddComponent<AIAction.Core.PlayerActionController>();
        player.AddComponent<AIAction.Core.PlayerAnimator>();
        player.AddComponent<AIAction.Core.PlayerHealth>();
    }

    private static void SetupCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            var follow = mainCamera.GetComponent<AIAction.Core.CameraFollow>();
            if (follow == null)
            {
                follow = mainCamera.gameObject.AddComponent<AIAction.Core.CameraFollow>();
            }
        }
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // Check if already added
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return;
        }
        
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"Added {scenePath} to Build Settings");
    }
}
