using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace AIAction.UI
{
    public class GameResultUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject clearPanel;
        [SerializeField] private GameObject gameOverPanel;
        
        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextLevelButton;
        
        private static GameResultUI instance;
        public static GameResultUI Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Hide panels initially
            if (clearPanel) clearPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
            
            // Setup button listeners
            if (retryButton) retryButton.onClick.AddListener(OnRetryClicked);
            if (menuButton) menuButton.onClick.AddListener(OnMenuClicked);
            if (nextLevelButton) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }
        
        public void ShowClear()
        {
            Debug.Log("Showing Clear UI");
            Time.timeScale = 0f;
            if (clearPanel) clearPanel.SetActive(true);
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }
        
        public void ShowGameOver()
        {
            Debug.Log("Showing Game Over UI");
            Time.timeScale = 0f;
            if (gameOverPanel) gameOverPanel.SetActive(true);
            if (clearPanel) clearPanel.SetActive(false);
        }
        
        public void HideAll()
        {
            if (clearPanel) clearPanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        
        private void OnRetryClicked()
        {
            Debug.Log("Retry clicked");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        private void OnMenuClicked()
        {
            Debug.Log("Menu clicked");
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
        
        private void OnNextLevelClicked()
        {
            Debug.Log("Next Level clicked");
            Time.timeScale = 1f;
            // For now just reload, can be extended to load next level
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                // No more levels, return to menu
                SceneManager.LoadScene("MainMenu");
            }
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
