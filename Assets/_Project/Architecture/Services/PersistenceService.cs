using UnityEngine;
using System;
using System.IO;
using System.Collections;

namespace QLDMathApp.Architecture.Services
{
    /// <summary>
    /// PERSISTENCE SERVICE: Handles secure storage to local filesystem.
    /// PERFORMANCE FIX: Uses dirty flag and end-of-frame batching to reduce I/O.
    /// </summary>
    public class PersistenceService : MonoBehaviour, IInitializable
    {
        public static PersistenceService Instance { get; private set; }

        private string _savePath;
        public bool IsInitialized { get; private set; }

        // PERFORMANCE: Cached data and dirty flag for deferred writes
        private AppUserData _cachedData;
        private bool _isDirty = false;
        private Coroutine _saveCoroutine;

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
            // Load data into cache on init
            _cachedData = LoadFromDisk<AppUserData>();
            yield return null;
            IsInitialized = true;
            Debug.Log($"[PersistenceService] Target: {_savePath}");
        }

        /// <summary>
        /// PERFORMANCE: Marks data as dirty and schedules end-of-frame save.
        /// Multiple saves in same frame are batched into one I/O operation.
        /// </summary>
        public void Save<T>(T data) where T : AppUserData
        {
            _cachedData = data;
            _isDirty = true;

            // Schedule deferred save if not already pending
            if (_saveCoroutine == null)
            {
                _saveCoroutine = StartCoroutine(DeferredSaveRoutine());
            }
        }

        /// <summary>
        /// PERFORMANCE: Returns cached data immediately (no I/O).
        /// </summary>
        public T Load<T>() where T : AppUserData, new()
        {
            if (_cachedData != null)
            {
                return (T)_cachedData;
            }
            
            _cachedData = LoadFromDisk<T>();
            return (T)_cachedData;
        }

        /// <summary>
        /// Actual disk read - only called on first load or after clear.
        /// </summary>
        private T LoadFromDisk<T>() where T : new()
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

        /// <summary>
        /// PERFORMANCE: Waits until end of frame, then writes if dirty.
        /// Batches multiple Save() calls into single I/O operation.
        /// </summary>
        private IEnumerator DeferredSaveRoutine()
        {
            yield return new WaitForEndOfFrame();

            if (_isDirty && _cachedData != null)
            {
                try
                {
                    string json = JsonUtility.ToJson(_cachedData, true);
                    File.WriteAllText(_savePath, json);
                    _isDirty = false;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PersistenceService] Save failed: {e.Message}");
                }
            }

            _saveCoroutine = null;
        }

        /// <summary>
        /// Force immediate save - use only during app quit.
        /// </summary>
        private void OnApplicationQuit()
        {
            if (_isDirty && _cachedData != null)
            {
                try
                {
                    string json = JsonUtility.ToJson(_cachedData, true);
                    File.WriteAllText(_savePath, json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PersistenceService] Emergency save failed: {e.Message}");
                }
            }
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
