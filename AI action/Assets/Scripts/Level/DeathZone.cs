using UnityEngine;
using AIAction.UI;

namespace AIAction.Level
{
    /// <summary>
    /// Triggers game over when player falls into this zone.
    /// Add a BoxCollider2D with IsTrigger = true.
    /// </summary>
    public class DeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player fell into death zone!");
                
                // Try to use GameResultUI
                if (GameResultUI.Instance != null)
                {
                    GameResultUI.Instance.ShowGameOver();
                }
                else
                {
                    // Fallback to LevelManager
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.RestartLevel();
                    }
                }
            }
        }
    }
}
