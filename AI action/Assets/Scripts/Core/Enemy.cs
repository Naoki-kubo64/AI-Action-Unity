using UnityEngine;

namespace AIAction.Core
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("Patrol Settings")]
        public float patrolSpeed = 2f;
        public float patrolDistance = 3f;
        public bool facingRight = true;
        
        [Header("Combat Settings")]
        public int damage = 1;
        public float knockbackForce = 5f;
        
        [Header("Health")]
        public int maxHealth = 1;
        private int currentHealth;
        
        private Vector3 startPosition;
        private Rigidbody2D rb;
        private SpriteRenderer sprite;
        private bool movingRight = true;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
            startPosition = transform.position;
            currentHealth = maxHealth;
            
            // Freeze rotation
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private float turnCooldown = 0f;

        private void FixedUpdate()
        {
            Patrol();
        }

        private void Patrol()
        {
            // Cooldown to prevent rapid direction flipping
            if (turnCooldown > 0)
            {
                turnCooldown -= Time.fixedDeltaTime;
            }
            
            float direction = movingRight ? 1f : -1f;
            
            // Update sprite facing direction (flip based on movement)
            if (sprite != null)
            {
                // Opossum sprite faces left by default, so flip when moving right
                sprite.flipX = movingRight;
            }
            
            // Edge detection - cast from front-bottom of enemy
            Vector2 edgeCheckOrigin = (Vector2)transform.position + new Vector2(direction * 0.6f, 0f);
            RaycastHit2D groundCheck = Physics2D.Raycast(edgeCheckOrigin, Vector2.down, 1.5f);
            
            // Wall detection - cast from center
            Vector2 wallCheckOrigin = (Vector2)transform.position;
            RaycastHit2D wallCheck = Physics2D.Raycast(wallCheckOrigin, new Vector2(direction, 0), 0.6f);
            
            // Debug rays (visible in Scene view with Gizmos on)
            Debug.DrawRay(edgeCheckOrigin, Vector2.down * 1.5f, groundCheck.collider != null ? Color.green : Color.red);
            Debug.DrawRay(wallCheckOrigin, new Vector2(direction, 0) * 0.6f, wallCheck.collider != null ? Color.yellow : Color.blue);
            
            // Check if should turn around (only if cooldown expired)
            bool shouldTurn = false;
            
            // No ground ahead = cliff
            if (groundCheck.collider == null)
            {
                shouldTurn = true;
            }
            // Wall ahead (but not player)
            else if (wallCheck.collider != null && 
                     wallCheck.collider.gameObject != this.gameObject &&
                     !wallCheck.collider.CompareTag("Player"))
            {
                shouldTurn = true;
            }
            
            if (shouldTurn && turnCooldown <= 0)
            {
                movingRight = !movingRight;
                turnCooldown = 0.3f; // Prevent re-flipping for 0.3 seconds
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                return;
            }
            
            // Move
            rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Check if player is stomping (coming from above)
                Vector2 contactNormal = collision.contacts[0].normal;
                if (contactNormal.y < -0.5f)
                {
                    // Player stomped on enemy
                    TakeDamage(1);
                    
                    // Bounce player up
                    var playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);
                    }
                }
                else
                {
                    // Enemy damages player
                    var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage, transform.position);
                    }
                }
            }
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Simple death - just destroy
            Destroy(gameObject);
        }
    }
}
