using UnityEngine;

namespace AIAction.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Idle Sprites")]
        public Sprite[] idleSprites;
        
        [Header("Run Sprites")]
        public Sprite[] runSprites;
        
        [Header("Jump Sprite")]
        public Sprite jumpSprite;
        
        [Header("Animation Settings")]
        public float frameRate = 8f;
        
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private PlayerActionController controller;
        
        private float frameTimer;
        private int currentFrame;
        private string currentState = "idle";

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerActionController>();
            
            // Auto-load sprites from Resources if not assigned
            if (idleSprites == null || idleSprites.Length == 0)
            {
                LoadSpritesFromResources();
            }
        }

        private void LoadSpritesFromResources()
        {
            // Try to load from SunnyLand Artwork
            var idleSprite1 = Resources.Load<Sprite>("Sprites/player-idle-1");
            var idleSprite2 = Resources.Load<Sprite>("Sprites/player-idle-2");
            var idleSprite3 = Resources.Load<Sprite>("Sprites/player-idle-3");
            var idleSprite4 = Resources.Load<Sprite>("Sprites/player-idle-4");
            
            if (idleSprite1 != null)
            {
                idleSprites = new Sprite[] { idleSprite1, idleSprite2, idleSprite3, idleSprite4 };
            }
            
            var runSprite1 = Resources.Load<Sprite>("Sprites/player-run-1");
            var runSprite2 = Resources.Load<Sprite>("Sprites/player-run-2");
            var runSprite3 = Resources.Load<Sprite>("Sprites/player-run-3");
            
            if (runSprite1 != null)
            {
                runSprites = new Sprite[] { runSprite1, runSprite2, runSprite3 };
            }
            
            jumpSprite = Resources.Load<Sprite>("Sprites/player-jump-1");
        }

        private void Update()
        {
            UpdateAnimationState();
            AnimateCurrentState();
        }

        private void UpdateAnimationState()
        {
            // Determine state based on velocity
            if (rb == null) return;
            
            bool isGrounded = IsGrounded();
            
            if (!isGrounded)
            {
                SetState("jump");
            }
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                SetState("run");
                // Flip sprite based on direction
                spriteRenderer.flipX = rb.linearVelocity.x < 0;
            }
            else
            {
                SetState("idle");
            }
        }

        private bool IsGrounded()
        {
            // Simple ground check using raycast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Default"));
            return hit.collider != null && !hit.collider.isTrigger;
        }

        private void SetState(string newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                currentFrame = 0;
                frameTimer = 0;
            }
        }

        private void AnimateCurrentState()
        {
            frameTimer += Time.deltaTime;
            
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0;
                
                Sprite[] sprites = GetCurrentSprites();
                if (sprites != null && sprites.Length > 0)
                {
                    currentFrame = (currentFrame + 1) % sprites.Length;
                    if (sprites[currentFrame] != null)
                    {
                        spriteRenderer.sprite = sprites[currentFrame];
                    }
                }
            }
        }

        private Sprite[] GetCurrentSprites()
        {
            switch (currentState)
            {
                case "idle":
                    return idleSprites;
                case "run":
                    return runSprites;
                case "jump":
                    return jumpSprite != null ? new Sprite[] { jumpSprite } : null;
                default:
                    return idleSprites;
            }
        }
    }
}
