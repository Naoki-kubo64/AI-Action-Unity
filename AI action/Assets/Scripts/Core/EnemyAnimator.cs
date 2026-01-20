using UnityEngine;

namespace AIAction.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("Walk Animation Sprites")]
        public Sprite[] walkSprites;
        
        [Header("Idle Animation Sprites (Optional)")]
        public Sprite[] idleSprites;
        
        [Header("Animation Settings")]
        public float frameRate = 8f;
        public bool autoLoadFromPath = true;
        public string spriteFolderPath = "SunnyLand Artwork/Sprites/Enemies/opossum";
        
        private SpriteRenderer spriteRenderer;
        private Enemy enemyScript;
        
        private float frameTimer;
        private int currentFrame;
        private bool isMoving = true;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            enemyScript = GetComponent<Enemy>();
            
            // Auto-load sprites if not assigned
            if ((walkSprites == null || walkSprites.Length == 0) && autoLoadFromPath)
            {
                LoadSpritesFromResources();
            }
        }

        private void LoadSpritesFromResources()
        {
            // Try to load opossum walk sprites
            var sprites = new System.Collections.Generic.List<Sprite>();
            
            for (int i = 1; i <= 6; i++)
            {
                string path = $"{spriteFolderPath}/opossum-{i}";
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
            
            if (sprites.Count > 0)
            {
                walkSprites = sprites.ToArray();
                Debug.Log($"Loaded {sprites.Count} opossum sprites");
            }
        }

        private void Update()
        {
            // Check if moving (based on velocity)
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            }
            
            AnimateCurrentState();
        }

        private void AnimateCurrentState()
        {
            Sprite[] sprites = isMoving ? walkSprites : (idleSprites != null && idleSprites.Length > 0 ? idleSprites : walkSprites);
            
            if (sprites == null || sprites.Length == 0) return;
            
            frameTimer += Time.deltaTime;
            
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0;
                currentFrame = (currentFrame + 1) % sprites.Length;
                
                if (sprites[currentFrame] != null)
                {
                    spriteRenderer.sprite = sprites[currentFrame];
                }
            }
        }
    }
}
