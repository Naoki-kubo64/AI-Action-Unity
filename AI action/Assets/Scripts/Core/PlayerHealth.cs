using UnityEngine;

namespace AIAction.Core
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public int maxHealth = 3;
        public float invincibilityDuration = 1.5f;
        public float knockbackForce = 8f;
        
        private int currentHealth;
        private bool isInvincible = false;
        private float invincibilityTimer = 0f;
        
        private Rigidbody2D rb;
        private SpriteRenderer sprite;

        public System.Action<int, int> OnHealthChanged; // current, max
        public System.Action OnDeath;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                
                // Blink effect
                if (sprite != null)
                {
                    sprite.enabled = Mathf.FloorToInt(invincibilityTimer * 10) % 2 == 0;
                }
                
                if (invincibilityTimer <= 0f)
                {
                    isInvincible = false;
                    if (sprite != null) sprite.enabled = true;
                }
            }
        }

        public void TakeDamage(int damage, Vector3 damageSource)
        {
            if (isInvincible) return;
            
            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Knockback
            if (rb != null)
            {
                Vector2 knockbackDir = (transform.position - damageSource).normalized;
                knockbackDir.y = 0.5f; // Add upward component
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
            
            // Start invincibility
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            
            Debug.Log($"Player took {damage} damage! HP: {currentHealth}/{maxHealth}");
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            Debug.Log("Player died!");
            OnDeath?.Invoke();
            // Could trigger game over screen here
        }
    }
}
