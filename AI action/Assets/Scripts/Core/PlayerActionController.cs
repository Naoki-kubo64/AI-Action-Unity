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
        public float moveSpeed = 5f;
        public float runMultiplier = 1.5f;
        public float stepSpeed = 2f;
        public float slideSpeed = 8f;
        
        [Header("Jump Settings")]
        public float hopForce = 5f;
        public float jumpForce = 10f;
        public float highJumpForce = 14f;
        public float longJumpForceX = 5f;
        public float longJumpForceY = 8f;
        public float wallKickForceX = 8f;
        public float wallKickForceY = 10f;

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

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
            col = GetComponent<BoxCollider2D>();
            // col.size might be set in editor, but we can init here
            if(col) col.size = normalColliderSize;
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
            // Notify game that actions are done
        }

        private IEnumerator ExecuteAction(AIActionResponse action)
        {
            float timer = 0f;
            string act = action.action.ToUpper();
            float duration = action.duration;

            Debug.Log($"Executing Action: {act} for {duration}s");

            // One-shot actions
            switch (act)
            {
                case "HOP": Jump(hopForce); break;
                case "JUMP": Jump(jumpForce); break;
                case "HIGH_JUMP": Jump(highJumpForce); break;
                case "LONG_JUMP_RIGHT": LongJump(1); break;
                case "LONG_JUMP_LEFT": LongJump(-1); break;
                case "WALL_KICK_RIGHT": WallKick(1); break;
                case "WALL_KICK_LEFT": WallKick(-1); break;
                case "STUMBLE": 
                    if(anim) anim.SetTrigger("Stumble");
                    break;
            }

            // Continuous actions
            while (timer < duration)
            {
                timer += Time.deltaTime;
                
                switch (act)
                {
                    case "STEP_RIGHT": Move(stepSpeed, 1); break;
                    case "STEP_LEFT": Move(stepSpeed, -1); break;
                    case "WALK_RIGHT": Move(moveSpeed, 1); break;
                    case "WALK_LEFT": Move(moveSpeed, -1); break;
                    case "RUN_RIGHT": Move(moveSpeed * runMultiplier, 1); break;
                    case "RUN_LEFT": Move(moveSpeed * runMultiplier, -1); break;
                    
                    case "SLIDE_RIGHT": Slide(1); break;
                    case "SLIDE_LEFT": Slide(-1); break;
                    
                    case "PUSH_RIGHT": Move(moveSpeed * 0.5f, 1); break;
                    case "PUSH_LEFT": Move(moveSpeed * 0.5f, -1); break;

                    case "WAIT": Move(0, 0); break;
                }

                yield return null;
            }

            Move(0, 0);
            ResetCollider();
        }

        private void Move(float speed, int dir)
        {
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
                col.size = normalColliderSize;
                col.offset = Vector2.zero;
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

        private void LongJump(int dir)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
            {
                rb.AddForce(new Vector2(dir * longJumpForceX, longJumpForceY), ForceMode2D.Impulse);
                UpdateVisuals(dir);
            }
        }

        private void WallKick(int dir)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(dir * wallKickForceX, wallKickForceY), ForceMode2D.Impulse);
            UpdateVisuals(dir);
        }

        private void UpdateVisuals(int dir)
        {
            if (dir != 0 && sprite != null)
            {
                sprite.flipX = dir < 0;
            }
            if (anim != null)
            {
                anim.SetBool("IsRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            }
        }
    }
}