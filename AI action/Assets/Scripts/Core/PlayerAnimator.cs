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
        
        [Header("Action Sprites (Optional)")]
        public Sprite[] attackSprites;
        public Sprite[] guardSprites;
        public Sprite[] crouchSprites;
        public Sprite[] wallSlideSprites;
        public Sprite[] airDashSprites;
        public Sprite[] rollSprites;
        public Sprite[] shootSprites; // Added for gun mechanics
        public Sprite fallSprite;
        
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

        public bool IsCrouching { get; set; }
        public bool IsWallSliding { get; set; }
        
        private float lockTimer;

        public void PlayOneShot(string state, float duration)
        {
            SetState(state);
            lockTimer = duration;
        }

        private void UpdateAnimationState()
        {
            // If animation is locked (e.g. attacking), don't change state
            if (lockTimer > 0)
            {
                lockTimer -= Time.deltaTime;
                return;
            }

            // Determine state based on velocity
            if (rb == null) return;
            
            bool isGrounded = IsGrounded();
            float vy = rb.linearVelocity.y;
            
            // Priority 1: Wall Slide
            if (IsWallSliding)
            {
                SetState("wallslide");
                spriteRenderer.flipX = false; // Usually handled by controller direction
                return;
            }

            // Priority 2: Crouch (Ground only)
            if (isGrounded && IsCrouching)
            {
                SetState("crouch");
                return;
            }
            
            // Only switch to air states if not grounded AND moving vertically
            if (!isGrounded && Mathf.Abs(vy) > 0.1f)
            {
                if (vy > 0) SetState("jump");
                else SetState("fall");
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
            // Check ground using a small box below the player
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                return Mathf.Abs(rb.linearVelocity.y) < 0.1f;
            }
            
            // Cast a small box downward from feet
            Vector2 boxCenter = (Vector2)transform.position + collider.offset + Vector2.down * (collider.size.y / 2 + 0.1f);
            Vector2 boxSize = new Vector2(collider.size.x * 0.8f, 0.2f);
            
            // Use OverlapBoxAll to handle cases where Player and Ground are on the same layer
            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
            
            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject && !hit.isTrigger)
                {
                    return true;
                }
            }
            
            return false;
        }

        private void OnDrawGizmos()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                Gizmos.color = IsGrounded() ? Color.green : Color.red;
                Vector2 boxCenter = (Vector2)transform.position + collider.offset + Vector2.down * (collider.size.y / 2 + 0.1f);
                Vector2 boxSize = new Vector2(collider.size.x * 0.8f, 0.2f);
                Gizmos.DrawWireCube(boxCenter, boxSize);
            }
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
                    if (jumpSprite != null) return new Sprite[] { jumpSprite };
                    return idleSprites != null && idleSprites.Length > 0 ? new Sprite[] { idleSprites[0] } : null;
                case "fall":
                    return fallSprite != null ? new Sprite[] { fallSprite } : (jumpSprite != null ? new Sprite[] { jumpSprite } : null);
                case "attack":
                    return attackSprites != null && attackSprites.Length > 0 ? attackSprites : idleSprites;
                case "guard":
                    return guardSprites != null && guardSprites.Length > 0 ? guardSprites : idleSprites;
                case "crouch":
                    return crouchSprites != null && crouchSprites.Length > 0 ? crouchSprites : idleSprites;
                case "wallslide":
                    return wallSlideSprites != null && wallSlideSprites.Length > 0 ? wallSlideSprites : (jumpSprite != null ? new Sprite[] { jumpSprite } : null);
                case "airdash":
                    return airDashSprites != null && airDashSprites.Length > 0 ? airDashSprites : runSprites;
                case "roll":
                    return rollSprites != null && rollSprites.Length > 0 ? rollSprites : runSprites;
                case "shoot":
                    return shootSprites != null && shootSprites.Length > 0 ? shootSprites : (attackSprites != null ? attackSprites : idleSprites);
                default:
                    return idleSprites;
            }
        }
        
        // Public method to set animation state from PlayerActionController
        public void SetAnimationState(string state)
        {
            SetState(state.ToLower());
        }
    }
}
