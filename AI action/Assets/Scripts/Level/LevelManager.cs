using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AIAction.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public int currentWorld = 1;
        public int currentStage = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadLevel(int world, int stage)
        {
            currentWorld = world;
            currentStage = stage;
            string sceneName = $"Level_{world}-{stage}";
            
            // Check if scene exists in build settings (pseudo-check)
            // In reality, we just try to load
            SceneManager.LoadScene(sceneName);
            Debug.Log($"Loading Level: {sceneName}");
        }

        public void NextLevel()
        {
            currentStage++;
            // Logic to wrap world if needed
            // e.g., if stage > 5, world++, stage=1
            LoadLevel(currentWorld, currentStage);
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}