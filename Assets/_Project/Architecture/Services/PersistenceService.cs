using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// PERSISTENCE SERVICE: Handles secure storage to local filesystem.
    /// AUDIT FIX: Replaces PlayerPrefs with JSON files in PersistentDataPath.
    /// </summary>
    public class PersistenceService : MonoBehaviour, IInitializable
    {
        public static PersistenceService Instance { get; private set; }

        private string _savePath;
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _savePath = Path.Combine(Application.persistentDataPath, "user_data.json");
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public IEnumerator Initialize()
        {
            // Prepare directory if needed (should exist by default in persistentDataPath)
            yield return null;
            IsInitialized = true;
            Debug.Log($"[PersistenceService] Target: {_savePath}");
        }

        public void Save<T>(T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(_savePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PersistenceService] Save failed: {e.Message}");
            }
        }

        public T Load<T>() where T : new()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    string json = File.ReadAllText(_savePath);
                    return JsonUtility.FromJson<T>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[PersistenceService] Load failed: {e.Message}");
            }
            return new T();
        }
    }

    [Serializable]
    public class AppUserData
    {
        // Session Statistics
        public int TotalSessions = 0;
        public float TotalMinutes = 0f;
        public float OverallAccuracy = 0f;
        public int TotalProblems = 0;

        // Accessibility Settings
        public bool ZenMode = false;
        public bool HighContrast = false;
        public bool ReducedMotion = false;
    }
}
