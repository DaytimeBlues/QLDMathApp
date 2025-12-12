using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QLDMathApp.UI.Services
{
    /// <summary>
    /// SCENE TRANSITIONER: Async scene loading with fade overlay.
    /// Persists across scenes. Prevents freeze during load.
    /// </summary>
    public sealed class SceneTransitioner : MonoBehaviour
    {
        public static SceneTransitioner Instance { get; private set; }

        [Header("Overlay")]
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private Image progressBar; // optional

        [Header("Timing")]
        [SerializeField] private float fadeSeconds = 0.25f;

        private bool _busy;

        public bool IsBusy => _busy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (overlay != null)
            {
                overlay.alpha = 0f;
                overlay.blocksRaycasts = false;
                overlay.interactable = false;
            }

            if (progressBar != null) progressBar.fillAmount = 0f;
        }

        /// <summary>
        /// Load a scene with fade transition.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_busy) return;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _busy = true;

            // Fade to black
            yield return Fade(1f);

            // Async load
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = true;

            while (!op.isDone)
            {
                // Unity progress goes 0..0.9 until activation; normalize for UI.
                if (progressBar != null)
                    progressBar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);

                yield return null;
            }

            if (progressBar != null) progressBar.fillAmount = 1f;

            // Fade from black
            yield return Fade(0f);

            _busy = false;
        }

        private IEnumerator Fade(float target)
        {
            if (overlay == null) yield break;

            overlay.blocksRaycasts = target > 0.001f;
            overlay.interactable = target > 0.001f;

            float start = overlay.alpha;
            float t = 0f;

            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.Lerp(start, target, t / fadeSeconds);
                yield return null;
            }

            overlay.alpha = target;
        }

        /// <summary>
        /// Instant scene load (no fade). Use for debugging.
        /// </summary>
        public void LoadSceneImmediate(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
