using UnityEngine;
using UnityEngine.SceneManagement;

namespace AIAction.Core
{
    /// <summary>
    /// Goal trigger - loads next scene when player reaches it
    /// </summary>
    public class GoalTrigger : MonoBehaviour
    {
        [Header("Scene Settings")]
        public string nextSceneName = "";
        public int nextSceneIndex = -1; // Use index if name is empty
        
        [Header("Effects")]
        public float delayBeforeLoad = 1f;
        
        private bool triggered = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (triggered) return;
            
            if (collision.CompareTag("Player"))
            {
                triggered = true;
                Debug.Log("Goal reached!");
                
                // Optional: Freeze player
                var rb = collision.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Static;
                }
                
                StartCoroutine(LoadNextScene());
            }
        }

        private System.Collections.IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(delayBeforeLoad);
            
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else if (nextSceneIndex >= 0)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                // Load next scene in build order
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                int nextIndex = currentIndex + 1;
                
                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextIndex);
                }
                else
                {
                    Debug.Log("Last level! Returning to first scene.");
                    SceneManager.LoadScene(0);
                }
            }
        }
    }
}
