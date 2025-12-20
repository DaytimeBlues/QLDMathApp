using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// OFFLINE STORAGE: Implements IInteractionLogStore using JSON files.
    /// Saves each interaction to a scrolling log file in PersistentDataPath.
    /// </summary>
    public class JsonInteractionStore : IInteractionLogStore
    {
        private readonly string _logPath;

        public JsonInteractionStore()
        {
            _logPath = Path.Combine(Application.persistentDataPath, "interaction_logs.json");
        }

        public void SaveLog(InteractionLog log)
        {
            try
            {
                var logs = GetAllLogs();
                logs.Add(log);
                
                string json = JsonUtility.ToJson(new LogWrapper { logs = logs }, true);
                File.WriteAllText(_logPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonInteractionStore] Save failed: {e.Message}");
            }
        }

        public List<InteractionLog> GetAllLogs()
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    string json = File.ReadAllText(_logPath);
                    var wrapper = JsonUtility.FromJson<LogWrapper>(json);
                    return wrapper?.logs ?? new List<InteractionLog>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonInteractionStore] Load failed: {e.Message}");
            }
            return new List<InteractionLog>();
        }

        public void ClearLogs()
        {
            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
        }

        [Serializable]
        private class LogWrapper
        {
            public List<InteractionLog> logs;
        }
    }
}
