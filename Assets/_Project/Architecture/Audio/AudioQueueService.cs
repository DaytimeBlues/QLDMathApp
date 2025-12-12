using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QLDMathApp.Architecture.Audio
{
    /// <summary>
    /// AUDIO QUEUE SERVICE: Prevents overlapping voice-overs and narration.
    /// Queues audio requests and plays them sequentially.
    /// Essential for accessibility and comprehension.
    /// </summary>
    public class AudioQueueService : MonoBehaviour
    {
        public static AudioQueueService Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource narrationSource;

        [Header("Settings")]
        [SerializeField] private float delayBetweenClips = 0.3f;

        private Queue<AudioRequest> _queue = new Queue<AudioRequest>();
        private Coroutine _playRoutine;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio source if not assigned
            if (narrationSource == null)
            {
                narrationSource = gameObject.AddComponent<AudioSource>();
                narrationSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// Queue an audio clip to play.
        /// </summary>
        public void Enqueue(AudioClip clip, Action onComplete = null)
        {
            if (clip == null) return;

            _queue.Enqueue(new AudioRequest
            {
                Clip = clip,
                OnComplete = onComplete
            });

            if (_playRoutine == null)
            {
                _playRoutine = StartCoroutine(ProcessQueue());
            }
        }

        /// <summary>
        /// Queue with priority (plays next after current clip).
        /// </summary>
        public void EnqueuePriority(AudioClip clip, Action onComplete = null)
        {
            if (clip == null) return;

            var priorityQueue = new Queue<AudioRequest>();
            priorityQueue.Enqueue(new AudioRequest
            {
                Clip = clip,
                OnComplete = onComplete
            });

            while (_queue.Count > 0)
            {
                priorityQueue.Enqueue(_queue.Dequeue());
            }

            _queue = priorityQueue;

            if (_playRoutine == null)
            {
                _playRoutine = StartCoroutine(ProcessQueue());
            }
        }

        /// <summary>
        /// Stop current playback and clear queue.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            narrationSource.Stop();
            _isPlaying = false;

            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }
        }

        /// <summary>
        /// Stop current clip but continue queue.
        /// </summary>
        public void Skip()
        {
            narrationSource.Stop();
        }

        private IEnumerator ProcessQueue()
        {
            while (_queue.Count > 0)
            {
                var request = _queue.Dequeue();
                _isPlaying = true;

                narrationSource.clip = request.Clip;
                narrationSource.Play();

                // Wait for clip to finish
                while (narrationSource.isPlaying)
                {
                    yield return null;
                }

                request.OnComplete?.Invoke();

                // Delay between clips for comprehension
                yield return new WaitForSeconds(delayBetweenClips);
            }

            _isPlaying = false;
            _playRoutine = null;
        }

        private struct AudioRequest
        {
            public AudioClip Clip;
            public Action OnComplete;
        }
    }
}
