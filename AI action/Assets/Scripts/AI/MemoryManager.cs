using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AIAction.AI
{
    [Serializable]
    public class MemoryData
    {
        public List<string> longTermFacts = new List<string>();
        public string lastSummary = "";
    }

    public class MemoryManager : MonoBehaviour
    {
        public static MemoryManager Instance { get; private set; }

        private string filePath;
        private MemoryData data;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                filePath = Path.Combine(Application.persistentDataPath, "ai_memory.json");
                LoadMemory();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadMemory()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    data = JsonUtility.FromJson<MemoryData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load memory: {e.Message}");
                    data = new MemoryData();
                }
            }
            else
            {
                data = new MemoryData();
            }
        }

        public void SaveMemory()
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save memory: {e.Message}");
            }
        }

        public void AddFact(string fact)
        {
            if (!data.longTermFacts.Contains(fact))
            {
                data.longTermFacts.Add(fact);
                SaveMemory();
            }
        }

        public string GetContextMemory()
        {
            if (data.longTermFacts.Count == 0 && string.IsNullOrEmpty(data.lastSummary))
                return "";

            string context = "Player Memory:\n";
            if (!string.IsNullOrEmpty(data.lastSummary))
                context += $"- Last Session Summary: {data.lastSummary}\n";
            
            foreach (var fact in data.longTermFacts)
            {
                context += $"- {fact}\n";
            }
            return context;
        }
    }
}