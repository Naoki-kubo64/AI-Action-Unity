using UnityEngine;
using UnityEngine.SceneManagement;

namespace AIAction.Core
{
    /// <summary>
    /// Persistent UI Manager - Creates and manages UI that persists across all scenes
    /// </summary>
    public class PersistentUIManager : MonoBehaviour
    {
        private static PersistentUIManager instance;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Find or create the persistent UI
            if (instance == null)
            {
                // Check if already exists (e.g., from prefab)
                instance = FindFirstObjectByType<PersistentUIManager>();
                
                if (instance == null)
                {
                    // Create from prefab if exists
                    var prefab = Resources.Load<GameObject>("PersistentUI");
                    if (prefab != null)
                    {
                        var go = Instantiate(prefab);
                        go.name = "PersistentUI";
                        instance = go.GetComponent<PersistentUIManager>();
                    }
                    else
                    {
                        // Create minimal UI
                        var go = new GameObject("PersistentUI");
                        instance = go.AddComponent<PersistentUIManager>();
                    }
                }
                
                DontDestroyOnLoad(instance.gameObject);
            }
        }

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
