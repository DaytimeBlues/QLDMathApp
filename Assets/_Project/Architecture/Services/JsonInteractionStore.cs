using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// OFFLINE STORAGE: Implements IInteractionLogStore using NDJSON (Newline-Delimited JSON).
    /// PERFORMANCE FIX: Uses append-only writes for O(1) per-save instead of O(N) read-modify-write.
    /// </summary>
    public class JsonInteractionStore : IInteractionLogStore
    {
        private readonly string _logPath;
        
        // Cached logs for reads (avoids re-parsing on every GetAllLogs call)
        private List<InteractionLog> _cachedLogs;
        private bool _cacheValid = false;

        public JsonInteractionStore()
        {
            _logPath = Path.Combine(Application.persistentDataPath, "interaction_logs.ndjson");
        }

        /// <summary>
        /// PERFORMANCE: Appends single log as NDJSON line. O(1) operation.
        /// </summary>
        public void SaveLog(InteractionLog log)
        {
            try
            {
                // Convert to JSON and append as single line
                string jsonLine = JsonUtility.ToJson(log) + "\n";
                File.AppendAllText(_logPath, jsonLine);
                
                // Invalidate cache so next read reflects new data
                _cacheValid = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonInteractionStore] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Reads all logs from NDJSON file. Uses caching to avoid repeated parsing.
        /// </summary>
        public List<InteractionLog> GetAllLogs()
        {
            if (_cacheValid && _cachedLogs != null)
            {
                return _cachedLogs;
            }

            _cachedLogs = new List<InteractionLog>();

            try
            {
                if (File.Exists(_logPath))
                {
                    // Read line by line (NDJSON format)
                    string[] lines = File.ReadAllLines(_logPath);
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            try
                            {
                                var log = JsonUtility.FromJson<InteractionLog>(line);
                                _cachedLogs.Add(log);
                            }
                            catch
                            {
                                // Skip malformed lines
                            }
                        }
                    }
                }
                _cacheValid = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonInteractionStore] Load failed: {e.Message}");
            }

            return _cachedLogs;
        }

        public void ClearLogs()
        {
            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
            _cachedLogs = new List<InteractionLog>();
            _cacheValid = true;
        }
    }
}
