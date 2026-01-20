using UnityEngine;
using AIAction.Core;

namespace AIAction.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Bullet : MonoBehaviour
    {
        public float speed = 10f;
        public float lifeTime = 2f;
        public int damage = 1;

        private Rigidbody2D rb;
        private int direction = 1;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // Ensure Dynamic body for collision detection
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            Destroy(gameObject, lifeTime);
        }

        public void Initialize(int dir)
        {
            direction = dir;
            // Flip sprite if moving left
            if (dir < 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(speed * direction, 0);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Ignore Player
            if (collision.CompareTag("Player")) return;

            // Hit Enemy
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
            // Hit Ground/Wall
            else if (!collision.isTrigger) 
            {
                Destroy(gameObject);
            }
        }

        // Fallback for enemies with solid colliders (isTrigger = false)
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Ignore Player
            if (collision.gameObject.CompareTag("Player")) return;

            // Hit Enemy
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
            else
            {
                // Hit wall/ground
                Destroy(gameObject);
            }
        }
    }
}