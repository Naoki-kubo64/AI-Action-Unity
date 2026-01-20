using UnityEngine;

namespace AIAction.Core
{
    /// <summary>
    /// Block that can be destroyed by player attack or stomp
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BreakableBlock : MonoBehaviour
    {
        [Header("Settings")]
        public int hitsToBreak = 1;
        public bool breakFromBelow = true;  // Mario-style
        public bool breakFromAttack = true;
        
        [Header("Effects")]
        public GameObject breakEffect;  // Optional particle effect
        
        private int currentHits = 0;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            
            // Check if hit from below
            if (breakFromBelow)
            {
                Vector2 contactNormal = collision.contacts[0].normal;
                if (contactNormal.y > 0.5f) // Player hitting from below
                {
                    Hit();
                }
            }
        }

        // Called from PlayerActionController when BREAK_OBJECT or ATTACK is used nearby
        public void OnAttacked()
        {
            if (breakFromAttack)
            {
                Hit();
            }
        }

        private void Hit()
        {
            currentHits++;
            
            // Visual feedback - shake
            StartCoroutine(ShakeEffect());
            
            if (currentHits >= hitsToBreak)
            {
                Break();
            }
        }

        private System.Collections.IEnumerator ShakeEffect()
        {
            Vector3 originalPos = transform.position;
            for (int i = 0; i < 5; i++)
            {
                transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
                yield return new WaitForSeconds(0.02f);
            }
            transform.position = originalPos;
        }

        private void Break()
        {
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }
            
            Debug.Log("Block broken!");
            Destroy(gameObject);
        }
    }
}
