using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AIAction.AI;
using AIAction.Core;

namespace AIAction.Core
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        [Header("References")]
        public PlayerActionController player;
        public TMP_InputField promptInput;
        public TMP_Text logText;

        [Header("State")]
        public bool isWaitingForInput = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Find references if not set
            if (player == null)
                player = FindObjectOfType<PlayerActionController>();

            if (promptInput != null)
            {
                promptInput.onSubmit.AddListener(OnPromptSubmit);
            }

            // Ensure game is running
            Time.timeScale = 1f;
            isWaitingForInput = false;

            if (LLMService.Instance != null)
            {
                LLMService.Instance.OnActionReceived += OnActionsReceived;
                LLMService.Instance.OnError += OnLLMError;
            }
            else
            {
                Log("Warning: LLMService instance not found. AI features disabled.");
            }
        }

        private void OnDestroy()
        {
            if (LLMService.Instance != null)
            {
                LLMService.Instance.OnActionReceived -= OnActionsReceived;
                LLMService.Instance.OnError -= OnLLMError;
            }
        }

        public void OnPromptSubmit(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Log($"You: {text}");
            
            // Send to LLM
            if (LLMService.Instance != null)
            {
                LLMService.Instance.SendUserMessage(text);
            }

            // Resume game
            Time.timeScale = 1f;
            isWaitingForInput = false;

            // Clear input
            if (promptInput != null)
            {
                promptInput.text = "";
                promptInput.DeactivateInputField();
            }
        }

        private void OnActionsReceived(System.Collections.Generic.List<AIActionResponse> actions)
        {
            string actionLog = "AI: ";
            foreach (var a in actions)
            {
                actionLog += $"[{a.action} {a.duration}s] ";
            }
            Log(actionLog);

            // Actions will be processed by PlayerActionController
            // After all actions complete, we should pause again
            // This is handled via coroutine completion in PlayerActionController
        }

        private void OnLLMError(string error)
        {
            Log($"ERROR: {error}");
            // Pause and allow retry
            Time.timeScale = 0f;
            isWaitingForInput = true;
        }

        public void Log(string message)
        {
            Debug.Log(message);
            if (logText != null)
            {
                logText.text += message + "\n";
            }
        }

        public void ShowPromptUI()
        {
            Time.timeScale = 0f;
            isWaitingForInput = true;
            if (promptInput != null)
            {
                promptInput.ActivateInputField();
            }
        }
    }
}