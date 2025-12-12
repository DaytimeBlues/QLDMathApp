#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace QLDMathApp.Editor
{
    /// <summary>
    /// EDITOR UTILITY: Generates placeholder AudioClips for testing.
    /// Creates silent clips with appropriate durations.
    /// Menu: Tools > QLD Math App > Audio > Generate Placeholders
    /// </summary>
    public static class AudioPlaceholderGenerator
    {
        private const string AUDIO_PATH = "Assets/_Project/Audio/";
        private const int SAMPLE_RATE = 44100;

        [MenuItem("Tools/QLD Math App/Audio/Generate All Placeholders")]
        public static void GenerateAllPlaceholders()
        {
            GenerateNumberAudio();
            GeneratePhraseAudio();
            GenerateSFXAudio();
            Debug.Log("[AudioGen] All placeholder audio created in " + AUDIO_PATH);
        }

        [MenuItem("Tools/QLD Math App/Audio/Generate Number Audio (1-10)")]
        public static void GenerateNumberAudio()
        {
            EnsureFolderExists("Numbers");
            
            string[] numbers = { "One", "Two", "Three", "Four", "Five",
                                "Six", "Seven", "Eight", "Nine", "Ten" };
            
            for (int i = 0; i < numbers.Length; i++)
            {
                float duration = 0.5f + (i * 0.05f); // Slightly longer for bigger numbers
                CreateSilentClip($"Numbers/Number_{i + 1}_{numbers[i]}", duration);
            }
        }

        [MenuItem("Tools/QLD Math App/Audio/Generate Phrase Audio")]
        public static void GeneratePhraseAudio()
        {
            EnsureFolderExists("Phrases");
            
            // Question phrases
            CreateSilentClip("Phrases/HowManyFireflies", 1.5f);
            CreateSilentClip("Phrases/PackItems", 2.0f);
            CreateSilentClip("Phrases/WhatComesNext", 1.5f);
            CreateSilentClip("Phrases/CountWithMe", 1.2f);
            
            // Feedback phrases
            CreateSilentClip("Phrases/WellDone", 0.8f);
            CreateSilentClip("Phrases/GreatJob", 0.8f);
            CreateSilentClip("Phrases/TryAgain", 1.0f);
            CreateSilentClip("Phrases/LetsCount", 1.2f);
            
            // Explanatory phrases
            CreateSilentClip("Phrases/ThatWasThree", 1.5f);
            CreateSilentClip("Phrases/TheBellyOfFive", 2.0f);
            
            // Welcome
            CreateSilentClip("Phrases/Welcome", 2.5f);
            CreateSilentClip("Phrases/TapToPlay", 1.0f);
        }

        [MenuItem("Tools/QLD Math App/Audio/Generate SFX Audio")]
        public static void GenerateSFXAudio()
        {
            EnsureFolderExists("SFX");
            
            // UI sounds
            CreateSilentClip("SFX/ButtonTap", 0.1f);
            CreateSilentClip("SFX/ButtonRelease", 0.1f);
            
            // Feedback sounds
            CreateSilentClip("SFX/SuccessChime", 0.8f);
            CreateSilentClip("SFX/EncouragementChime", 0.6f);
            CreateSilentClip("SFX/StarCollect", 0.3f);
            
            // Game sounds
            CreateSilentClip("SFX/FireflyAppear", 0.3f);
            CreateSilentClip("SFX/FireflyDisappear", 0.2f);
            CreateSilentClip("SFX/ItemPickup", 0.2f);
            CreateSilentClip("SFX/ItemDrop", 0.2f);
            CreateSilentClip("SFX/PatternPulse", 0.2f);
            
            // Hub sounds
            CreateSilentClip("SFX/AvatarWalk", 1.0f);
            CreateSilentClip("SFX/NodeUnlock", 1.0f);
            
            // Music (longer)
            EnsureFolderExists("Music");
            CreateSilentClip("Music/MenuLoop", 30.0f);
            CreateSilentClip("Music/GameplayLoop", 60.0f);
        }

        private static void CreateSilentClip(string name, float duration)
        {
            int sampleCount = Mathf.CeilToInt(SAMPLE_RATE * duration);
            float[] samples = new float[sampleCount];
            
            // Generate very quiet noise (not completely silent for testing)
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = Random.Range(-0.001f, 0.001f);
            }
            
            // Create AudioClip
            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            
            // Save as WAV asset
            string fullPath = AUDIO_PATH + name + ".wav";
            SaveWav(fullPath, clip);
            
            AssetDatabase.ImportAsset(fullPath);
            Debug.Log($"[AudioGen] Created: {fullPath} ({duration}s)");
        }

        private static void SaveWav(string filepath, AudioClip clip)
        {
            // Ensure directory exists
            string dir = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            float[] samples = new float[clip.samples];
            clip.GetData(samples, 0);

            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // WAV header
                int sampleCount = samples.Length;
                int channels = clip.channels;
                int sampleRate = clip.frequency;
                int byteRate = sampleRate * channels * 2;
                int blockAlign = channels * 2;
                int dataSize = sampleCount * channels * 2;

                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16); // Subchunk1Size
                writer.Write((short)1); // AudioFormat (PCM)
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)blockAlign);
                writer.Write((short)16); // BitsPerSample
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);

                // Write samples
                foreach (float sample in samples)
                {
                    short intSample = (short)(sample * 32767f);
                    writer.Write(intSample);
                }
            }
        }

        private static void EnsureFolderExists(string subfolder = null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Audio"))
                AssetDatabase.CreateFolder("Assets/_Project", "Audio");
            
            if (!string.IsNullOrEmpty(subfolder))
            {
                string path = "Assets/_Project/Audio/" + subfolder;
                if (!AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/Audio", subfolder);
                }
            }
        }
    }
}
#endif
