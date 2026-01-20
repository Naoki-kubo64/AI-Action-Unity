using UnityEngine;

namespace AIAction.Core
{
    /// <summary>
    /// Spike trap - damages player on contact
    /// </summary>
    public class SpikeTrap : MonoBehaviour
    {
        [Header("Damage Settings")]
        public int damage = 1;
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                }
            }
        }
    }
}
