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
        
        public float stepSpeed = 2f;
        public float slideSpeed = 8f;
        
        [Header("Vertical Jump Settings")]
        public float hopForce = 5f;       // Little hop
        public float jumpForce = 10f;     // Standard jump
        public float highJumpForce = 15f; // High jump

        [Header("Directional Jump Settings (X, Y)")]
        // Short: small gap
        public Vector2 jumpShortForce = new Vector2(4f, 6f);
        // Medium: standard gap
        public Vector2 jumpMediumForce = new Vector2(7f, 9f);
        // Long: wide gap
        public Vector2 jumpLongForce = new Vector2(10f, 11f);

        [Header("Physics Constraints")]
        public Vector2 normalColliderSize = new Vector2(1f, 2f);
        public Vector2 slideColliderSize = new Vector2(1f, 1f);
        public Vector2 slideColliderOffset = new Vector2(0f, -0.5f);

        private Rigidbody2D rb;
        private Animator anim;
        private SpriteRenderer sprite;
        private BoxCollider2D col;

        private bool isExecuting = false;
        private Queue<AIActionResponse> actionQueue = new Queue<AIActionResponse>();
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
            col = GetComponent<BoxCollider2D>();
            // Store the original collider size set in the editor
            if(col) {
                originalColliderSize = col.size;
                originalColliderOffset = col.offset;
            }
        }

        private void Start()
        {
            if (LLMService.Instance != null)
            {
                LLMService.Instance.OnActionReceived += OnActionsReceived;
            }
        }

        private void OnDestroy()
        {
            if (LLMService.Instance != null)
            {
                LLMService.Instance.OnActionReceived -= OnActionsReceived;
            }
        }

        public void OnActionsReceived(List<AIActionResponse> actions)
        {
            foreach (var action in actions)
            {
                actionQueue.Enqueue(action);
            }

            if (!isExecuting)
            {
                StartCoroutine(ProcessActionQueue());
            }
        }

        private IEnumerator ProcessActionQueue()
        {
            isExecuting = true;
            Time.timeScale = 1f; 

            while (actionQueue.Count > 0)
            {
                var currentAction = actionQueue.Dequeue();
                yield return ExecuteAction(currentAction);
            }

            isExecuting = false;
        }

        private IEnumerator ExecuteAction(AIActionResponse action)
        {
            float timer = 0f;
            string act = action.action.ToUpper();
            float duration = action.duration;

            Debug.Log($"Executing Action: {act} for {duration}s");

            // --- Impulse Actions (One-shot) ---
            switch (act)
            {
                // Vertical
                case "HOP":       Jump(hopForce); break;
                case "JUMP":      Jump(jumpForce); break;
                case "HIGH_JUMP": Jump(highJumpForce); break;

                // Directional (Right)
                case "JUMP_RIGHT_SHORT":  DirectionalJump(1, jumpShortForce); break;
                case "JUMP_RIGHT_MEDIUM": DirectionalJump(1, jumpMediumForce); break;
                case "JUMP_RIGHT_LONG":   DirectionalJump(1, jumpLongForce); break;

                // Directional (Left)
                case "JUMP_LEFT_SHORT":   DirectionalJump(-1, jumpShortForce); break;
                case "JUMP_LEFT_MEDIUM":  DirectionalJump(-1, jumpMediumForce); break;
                case "JUMP_LEFT_LONG":    DirectionalJump(-1, jumpLongForce); break;
                
                // Legacy / Aliases
                case "LONG_JUMP_RIGHT":   DirectionalJump(1, jumpLongForce); break;
                case "LONG_JUMP_LEFT":    DirectionalJump(-1, jumpLongForce); break;

                case "STUMBLE": 
                    if(anim) anim.SetTrigger("Stumble");
                    break;
                case "STOP":
                    Move(0, 0);
                    break;
            }

            // --- Continuous Actions (Over Duration) ---
            // If duration is 0, we can skip loop unless it's a move command that needs at least 1 frame?
            // Usually AI sends duration > 0 for moves.
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                
                switch (act)
                {
                    // --- Right Movement ---
                    case "CREEP_RIGHT": Move(creepSpeed, 1); break;
                    case "WALK_RIGHT":  Move(walkSpeed, 1); break;
                    case "RUN_RIGHT":   Move(runSpeed, 1); break;
                    case "DASH_RIGHT":  Move(dashSpeed, 1); break;
                    
                    case "STEP_RIGHT":  Move(stepSpeed, 1); break; // Slow precise step

                    // --- Left Movement ---
                    case "CREEP_LEFT":  Move(creepSpeed, -1); break;
                    case "WALK_LEFT":   Move(walkSpeed, -1); break;
                    case "RUN_LEFT":    Move(runSpeed, -1); break;
                    case "DASH_LEFT":   Move(dashSpeed, -1); break;

                    case "STEP_LEFT":   Move(stepSpeed, -1); break;

                    // --- Sliding ---
                    case "SLIDE_RIGHT": Slide(1); break;
                    case "SLIDE_LEFT":  Slide(-1); break;
                    
                    // --- Utility ---
                    case "WAIT":        Move(0, 0); break;
                }

                yield return null;
            }

            // End of action cleanup
            Move(0, 0);
            ResetCollider();
        }

        private void Move(float speed, int dir)
        {
            // Preserve Y velocity for gravity
            rb.linearVelocity = new Vector2(speed * dir, rb.linearVelocity.y);
            UpdateVisuals(dir);
        }

        private void Slide(int dir)
        {
            rb.linearVelocity = new Vector2(slideSpeed * dir, rb.linearVelocity.y);
            if(col) 
            {
                col.size = slideColliderSize;
                col.offset = slideColliderOffset;
            }
            UpdateVisuals(dir);
            if(anim) anim.SetBool("IsSliding", true);
        }

        private void ResetCollider()
        {
            if(col)
            {
                col.size = originalColliderSize;
                col.offset = originalColliderOffset;
            }
            if(anim) anim.SetBool("IsSliding", false);
        }

        private void Jump(float force)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
            {
                rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
                if(anim) anim.SetTrigger("Jump");
            }
        }

        private void DirectionalJump(int dir, Vector2 forceInfo)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
            {
                // ForceInfo.x is magnitude, multiply by dir
                rb.AddForce(new Vector2(forceInfo.x * dir, forceInfo.y), ForceMode2D.Impulse);
                UpdateVisuals(dir);
                if(anim) anim.SetTrigger("Jump");
            }
        }

        private void UpdateVisuals(int dir)
        {
            if (dir != 0 && sprite != null)
            {
                sprite.flipX = dir < 0;
            }
            // Simple run check
            if (anim != null)
            {
                anim.SetBool("IsRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            }
        }
    }
}