using UnityEngine;

namespace AIAction.Core
{
    /// <summary>
    /// Platform that moves back and forth between two points
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Movement Settings")]
        public Vector3 endOffset = new Vector3(5f, 0f, 0f);
        public float speed = 2f;
        public float waitTime = 0.5f;
        
        private Vector3 startPosition;
        private Vector3 endPosition;
        private bool movingToEnd = true;
        private float waitTimer = 0f;

        private void Start()
        {
            startPosition = transform.position;
            endPosition = startPosition + endOffset;
        }

        private void Update()
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                return;
            }
            
            Vector3 target = movingToEnd ? endPosition : startPosition;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                movingToEnd = !movingToEnd;
                waitTimer = waitTime;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Parent player to platform so they move with it
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize path in editor
            Gizmos.color = Color.yellow;
            Vector3 start = Application.isPlaying ? startPosition : transform.position;
            Vector3 end = start + endOffset;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireCube(start, Vector3.one * 0.3f);
            Gizmos.DrawWireCube(end, Vector3.one * 0.3f);
        }
    }
}
