using UnityEngine;
using AIAction.UI;

namespace AIAction.Level
{
    public enum GimmickType { MovingPlatform, Spikes, Goal }

    public class StageGimmicks : MonoBehaviour
    {
        public GimmickType type;
        
        [Header("Moving Platform")]
        public Vector2 moveOffset = new Vector2(3, 0);
        public float moveDuration = 2f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            if (type == GimmickType.MovingPlatform)
            {
                float t = Mathf.PingPong(Time.time, moveDuration) / moveDuration;
                transform.position = startPos + (Vector3)(moveOffset * t);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (type == GimmickType.Spikes)
                {
                    Debug.Log("Player hit spikes!");
                    if (GameResultUI.Instance != null)
                    {
                        GameResultUI.Instance.ShowGameOver();
                    }
                    else
                    {
                        LevelManager.Instance?.RestartLevel();
                    }
                }
                else if (type == GimmickType.Goal)
                {
                    Debug.Log("Level Clear!");
                    if (GameResultUI.Instance != null)
                    {
                        GameResultUI.Instance.ShowClear();
                    }
                    else
                    {
                        LevelManager.Instance?.NextLevel();
                    }
                }
            }
        }

        // OnCollisionEnter for moving platform parenting is handled by Physics2D typically,
        // or we can add a simple parent/unparent logic here if strictly needed.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (type == GimmickType.MovingPlatform && collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(this.transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (type == GimmickType.MovingPlatform && collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null);
            }
        }
    }
}