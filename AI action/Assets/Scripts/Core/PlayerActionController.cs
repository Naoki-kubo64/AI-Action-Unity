using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIAction.AI;

namespace AIAction.Core
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerActionController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float creepSpeed = 2f;
        public float walkSpeed = 5f;
        public float runSpeed = 9f;
        public float dashSpeed = 14f;
        public float speedMultiplier = 1f;

        public float stepSpeed = 2f;
        public float slideSpeed = 8f;
        
        [Header("Vertical Jump Settings")]
        public float hopForce = 5f;
        public float jumpForce = 10f;
        public float highJumpForce = 15f;

        [Header("Directional Jump Settings (X, Y)")]
        public Vector2 jumpShortForce = new Vector2(4f, 6f);
        public Vector2 jumpMediumForce = new Vector2(7f, 9f);
        public Vector2 jumpLongForce = new Vector2(10f, 11f);

        [Header("Advanced Mobility")]
        public Vector2 wallkickForce = new Vector2(8f, 10f);
        public float wallSlideSpeed = 2f;
        public float airDashSpeed = 20f;
        public float groundPoundForce = 20f;
        public float crouchHeightMultiplier = 0.5f;

        [Header("Combat Settings")]
        public GameObject bulletPrefab;
        public Vector2 bulletSpawnOffset = new Vector2(0.8f, 0.2f);

        [Header("Physics Constraints")]
        public LayerMask groundLayer;
        public float groundCheckDist = 0.1f;
        public float wallCheckDist = 0.5f;
        public Vector2 normalColliderSize = new Vector2(1f, 2f);
        public Vector2 normalColliderOffset = Vector2.zero; // Will be overwritten by Awake

        private Rigidbody2D rb;
        private Animator anim;
        private PlayerAnimator playerAnim; // Reference to custom animator
        private SpriteRenderer sprite;
        private BoxCollider2D col;

        // State Checking
        private bool isGrounded;
        private bool isTouchingWall;
        private int facingDirection = 1; // 1: Right, -1: Left
        private bool isCrouching = false;
        private bool isWallSliding = false;

        private bool isExecuting = false;
        private Queue<AIActionResponse> actionQueue = new Queue<AIActionResponse>();

        // Refs for restoration
        private Vector2 originalSize;
        private Vector2 originalOffset;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            playerAnim = GetComponent<PlayerAnimator>(); // Get reference
            sprite = GetComponent<SpriteRenderer>();
            col = GetComponent<BoxCollider2D>();
            
            if(col) {
                originalSize = col.size;
                originalOffset = col.offset;
                normalColliderSize = originalSize;
                normalColliderOffset = originalOffset;
            }
        }

        private void Start()
        {
            // Example: If you want to start with a specific state or action
        }

        private void OnDestroy()
        {
            // Cleanup if necessary
        }

        private void FixedUpdate()
        {
            CheckPhysicsEnvironment();
            // Apply wall slide physics if active
            if (isWallSliding && !isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }

        private void Update()
        {
            ProcessActionQueue();
        }

        private void CheckPhysicsEnvironment()
        {
// Ground Check
            isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDist, groundLayer);
            // PlayerAnimator handles its own ground check internally

            // Wall Check (using a small offset to check slightly in front of the player)
            Vector2 wallCheckOrigin = (Vector2)transform.position + new Vector2(col.size.x / 2 * facingDirection, 0);
            isTouchingWall = Physics2D.Raycast(wallCheckOrigin, Vector2.right * facingDirection, wallCheckDist, groundLayer);

            // Update animator with current velocity
            if (anim)
            {
                anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
                anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
                anim.SetBool("IsGrounded", isGrounded);
            }
        }

        public void OnActionsReceived(List<AIActionResponse> actions)
        {
            Debug.Log($"Controller received {actions.Count} actions");
            foreach (var action in actions)
            {
                actionQueue.Enqueue(action);
            }
        }

        private void ProcessActionQueue()
        {
            if (!isExecuting && actionQueue.Count > 0)
            {
                AIActionResponse nextAction = actionQueue.Dequeue();
                StartCoroutine(ExecuteAction(nextAction));
            }
        }

        private IEnumerator ExecuteAction(AIActionResponse actionResponse)
        {
            isExecuting = true;
            string act = actionResponse.action.ToUpper();
            float duration = actionResponse.duration;
            // Ensure duration is reasonable
            if (duration <= 0.05f) duration = 0.1f;

            Debug.Log($"Exec Action: {act}, Dur: {duration}");

            try
            {
                float timer = 0f;

            // --- ONE-SHOT ACTIONS ---
            switch (act)
            {
                // Vertical Jumps
                case "HOP":           Jump(hopForce); break;
                case "JUMP":          Jump(jumpForce); break;
                case "HIGH_JUMP":     Jump(highJumpForce); break;
                case "WALL_JUMP":     PerformWallJump(); break;

                // Directional Jumps
                case "JUMP_RIGHT_SHORT": DirectionalJump(1, jumpShortForce); break;
                case "JUMP_LEFT_SHORT":  DirectionalJump(-1, jumpShortForce); break;
                case "JUMP_RIGHT_MEDIUM": DirectionalJump(1, jumpMediumForce); break;
                case "JUMP_LEFT_MEDIUM": DirectionalJump(-1, jumpMediumForce); break;
                case "JUMP_RIGHT_LONG": DirectionalJump(1, jumpLongForce); break;
                case "JUMP_LEFT_LONG": DirectionalJump(-1, jumpLongForce); break;

                // Combat & Special
                case "ATTACK":        PlayAnim("attack", 0.5f); break;
                case "GUARD":         PlayAnim("guard", 0.5f); break;
                case "DODGE_ROLL":    PlayAnim("roll", 0.5f); break; 
                case "STOMP":         rb.AddForce(Vector2.down * groundPoundForce, ForceMode2D.Impulse); break;
                case "INTERACT":      Debug.Log("Interacting..."); break;
                case "BREAK_OBJECT":  PlayAnim("attack", 0.5f); break;
                case "STOP":          Move(0, 0); break;
                case "FALL":          /* Natural fall - just let gravity do its work */ break;
                
                // Gun
                case "SHOOT":
                case "FIRE":
                    Shoot();
                    break;
            }

            // --- CONTINUOUS ACTIONS ---
            while (timer < duration)
            {
                timer += Time.deltaTime;
                    
                switch (act)
                {
                    // Basic Movement
                    case "CREEP_RIGHT": Move(creepSpeed, 1); break;
                    case "WALK_RIGHT":  Move(walkSpeed, 1); break;
                    case "RUN_RIGHT":   Move(runSpeed, 1); break;
                    case "DASH_RIGHT":  Move(dashSpeed, 1); break;
                    case "AIR_DASH_RIGHT": AirDash(1); break;

                    case "CREEP_LEFT":  Move(creepSpeed, -1); break;
                    case "WALK_LEFT":   Move(walkSpeed, -1); break;
                    case "RUN_LEFT":    Move(runSpeed, -1); break;
                    case "DASH_LEFT":   Move(dashSpeed, -1); break;
                    case "AIR_DASH_LEFT": AirDash(-1); break;

                    // Precision
                    case "STEP_RIGHT":  Move(stepSpeed, 1); break;
                    case "STEP_LEFT":   Move(stepSpeed, -1); break;

                    // Advanced Stance
                    case "SLIDE_RIGHT": SetCrouch(true); Move(slideSpeed, 1); break;
                    case "SLIDE_LEFT":  SetCrouch(true); Move(slideSpeed, -1); break;
                    case "CRAWL_RIGHT": SetCrouch(true); Move(creepSpeed, 1); break; // Slow crawl
                    case "CRAWL_LEFT":  SetCrouch(true); Move(creepSpeed, -1); break;
                    case "CROUCH":      SetCrouch(true); Move(0, 0); break;

                    case "WALL_SLIDE": 
                        if(isTouchingWall) 
                        {
                            rb.linearVelocity = new Vector2(0, -wallSlideSpeed);
                            SetWallSlide(true);
                        }
                        break;

                    case "WAIT": Move(0, 0); break;
                    case "GUARD": PlayAnim("guard", 0.1f); Move(0,0); break; // Guard hold
                }
                yield return null;
            }

            }
            finally
            {
                // Cleanup
                SetCrouch(false);
                SetWallSlide(false);
                Move(0, 0);
                Debug.Log($"Action Finished. isExecuting reset.");
                isExecuting = false;
            }
        }

        // --- CORE ACTIONS ---

        private void Move(float speed, int dir)
        {
            rb.linearVelocity = new Vector2(speed * dir, rb.linearVelocity.y);
            UpdateVisuals(dir);
        }

        private void AirDash(int dir)
        {
            // Ignore gravity for a moment creates true dash feel
            rb.linearVelocity = new Vector2(airDashSpeed * dir, 0); 
            UpdateVisuals(dir);
        }

        private void Jump(float force)
        {
            // Allow jump if grounded OR velocity is effectively zero (fallback for failed raycast setup)
            if (isGrounded || Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
                if(anim) anim.SetTrigger("Jump");
                // Trigger Jump on PlayerAnimator if needed? It detects it automatically via velocity.
                Debug.Log("Jump executed!");
            }
            else
            {
                Debug.LogWarning("Jump Failed: Not Grounded");
            }
        }

        private void PerformWallJump()
        {
            if (isTouchingWall && !isGrounded)
            {
                // Kick away from wall (opposite to facing direction)
                int kickDir = -facingDirection;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(kickDir * wallkickForce.x, wallkickForce.y), ForceMode2D.Impulse);
                UpdateVisuals(kickDir);
                Debug.Log("Wall Jump!");
            }
        }

        private void DirectionalJump(int dir, Vector2 forceInfo)
        {
             // Allow jump if grounded OR velocity is effectively zero (fallback for failed raycast setup)
            if (isGrounded || Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(forceInfo.x * dir, forceInfo.y), ForceMode2D.Impulse);
                UpdateVisuals(dir);
                if(anim) anim.SetTrigger("Jump");
                Debug.Log("Directional Jump executed!");
            }
             else
            {
                Debug.LogWarning("Directional Jump Failed: Not Grounded");
            }
            {
                Debug.LogWarning("Directional Jump Failed: Not Grounded");
            }
        }

        private void Shoot()
        {
            StartCoroutine(ShootCoroutine());
        }

        private IEnumerator ShootCoroutine()
        {
            // Start animation immediately
            PlayAnim("shoot", 0.5f);

            if (bulletPrefab != null)
            {
                // Wait until Element 4 timing (frame index 4 at 8fps = 0.5s)
                // Adjust delay as needed: frameIndex * (1 / frameRate)
                float shootDelay = 4f / 8f; // Element 4 at 8fps
                yield return new WaitForSeconds(shootDelay);

                // Calculate spawn position
                Vector2 spawnPos = (Vector2)transform.position + new Vector2(bulletSpawnOffset.x * facingDirection, bulletSpawnOffset.y);
                GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
                
                var bulletScript = bulletObj.GetComponent<AIAction.Combat.Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.Initialize(facingDirection);
                }
                
                Debug.Log("Shoot executed!");
            }
            else
            {
                Debug.LogWarning("Bullet Prefab not assigned!");
            }
        }

        // --- UTILS ---

        private void SetCrouch(bool active)
        {
            if (active == isCrouching) return;
            isCrouching = active;

            if (col)
            {
                if (active)
                {
                    col.size = new Vector2(originalSize.x, originalSize.y * crouchHeightMultiplier);
                    col.offset = new Vector2(originalOffset.x, originalOffset.y - (originalSize.y * (1f-crouchHeightMultiplier)*0.5f));
                }
                else
                {
                    col.size = originalSize;
                    col.offset = originalOffset;
                }
            }
            if(anim) anim.SetBool("IsCrouching", active);
            if(playerAnim) playerAnim.IsCrouching = active; // Update PlayerAnimator
        }

        private void SetWallSlide(bool active)
        {
            if (active == isWallSliding) return;
            isWallSliding = active;
            if(playerAnim) playerAnim.IsWallSliding = active;
        }

        private void UpdateVisuals(int dir)
        {
            if (dir != 0)
            {
                facingDirection = dir;
                if (sprite != null) sprite.flipX = dir < 0;
            }
            if (anim != null)
            {
                anim.SetBool("IsRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            }
        }

        private void PlayAnim(string name, float duration)
        {
            if(anim) anim.SetTrigger(name);
            if(playerAnim) playerAnim.PlayOneShot(name.ToLower(), duration);
        }
    }
}