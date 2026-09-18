using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeastClad.Audio
{
    /// <summary>
    /// Central audio manager service for BeastClad.
    /// Provides pooled 2D audio channels for combat SFX, dedicated BGM streaming with crossfades,
    /// UI sounds, and synthetic fallback cues. Safe for editor tests and scene transitions.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<AudioManager>();
                    if (instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("[AudioManager]");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Volume Buses (0.0 to 1.0)")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.9f;
        [Range(0f, 1f)] [SerializeField] private float uiVolume = 0.8f;

        [Header("Pool Configuration")]
        [SerializeField] private int sfxPoolSize = 12;

        private AudioSource musicSource;
        private AudioSource uiSource;
        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private int nextSfxIndex = 0;

        // Cached synthetic procedural clips for prototyping
        private AudioClip syntheticEquipBeep;
        private AudioClip syntheticHitBeep;
        private AudioClip syntheticCooldownBeep;

        public float MasterVolume => masterVolume;
        public float SFXVolume => sfxVolume;
        public float MusicVolume => musicVolume;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeAudioSources();
            CreateSyntheticFallbackClips();
        }

        private void InitializeAudioSources()
        {
            // BGM Source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // 2D

            // UI Source
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
            uiSource.spatialBlend = 0f;

            // SFX Pool
            sfxPool.Clear();
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                sfxPool.Add(src);
            }
        }

        #region Playback API

        /// <summary>
        /// Plays a sound effect with optional pitch variation.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1.0f, float pitchVariation = 0.05f)
        {
            if (clip == null) return;

            var src = GetNextSfxSource();
            if (src == null) return;

            src.pitch = 1.0f + (pitchVariation > 0f ? Random.Range(-pitchVariation, pitchVariation) : 0f);
            src.volume = masterVolume * sfxVolume * Mathf.Clamp01(volumeMultiplier);
            src.clip = clip;
            src.Play();
        }

        /// <summary>
        /// Plays a UI sound effect without pitch randomization.
        /// </summary>
        public void PlayUI(AudioClip clip, float volumeMultiplier = 1.0f)
        {
            if (clip == null || uiSource == null) return;

            uiSource.pitch = 1.0f;
            uiSource.volume = masterVolume * uiVolume * Mathf.Clamp01(volumeMultiplier);
            uiSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Plays background music with smooth crossfading.
        /// </summary>
        public void PlayMusic(AudioClip clip, float fadeDuration = 0.6f)
        {
            if (musicSource == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            StopAllCoroutines();
            StartCoroutine(FadeMusicRoutine(clip, fadeDuration));
        }

        private IEnumerator FadeMusicRoutine(AudioClip newClip, float duration)
        {
            float targetVol = masterVolume * musicVolume;
            float halfDuration = Mathf.Max(0.05f, duration * 0.5f);

            // Fade out current
            if (musicSource.isPlaying)
            {
                float startVol = musicSource.volume;
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    musicSource.volume = Mathf.Lerp(startVol, 0f, t / halfDuration);
                    yield return null;
                }
            }

            musicSource.Stop();
            musicSource.clip = newClip;

            if (newClip != null)
            {
                musicSource.Play();
                // Fade in new
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    musicSource.volume = Mathf.Lerp(0f, targetVol, t / halfDuration);
                    yield return null;
                }
                musicSource.volume = targetVol;
            }
        }

        private AudioSource GetNextSfxSource()
        {
            if (sfxPool.Count == 0) return null;

            // Find an idle source
            for (int i = 0; i < sfxPool.Count; i++)
            {
                int idx = (nextSfxIndex + i) % sfxPool.Count;
                if (!sfxPool[idx].isPlaying)
                {
                    nextSfxIndex = (idx + 1) % sfxPool.Count;
                    return sfxPool[idx];
                }
            }

            // If all busy, steal the oldest
            var stolen = sfxPool[nextSfxIndex];
            nextSfxIndex = (nextSfxIndex + 1) % sfxPool.Count;
            return stolen;
        }

        #endregion

        #region Synthetic Prototype Cues

        public void PlaySyntheticEquipCue()
        {
            if (syntheticEquipBeep != null)
            {
                PlaySFX(syntheticEquipBeep, 0.6f, 0.08f);
            }
        }

        public void PlaySyntheticHitCue()
        {
            if (syntheticHitBeep != null)
            {
                PlaySFX(syntheticHitBeep, 0.8f, 0.12f);
            }
        }

        public void PlaySyntheticCooldownReadyCue()
        {
            if (syntheticCooldownBeep != null)
            {
                PlaySFX(syntheticCooldownBeep, 0.4f, 0.0f);
            }
        }

        private void CreateSyntheticFallbackClips()
        {
            // Equip snap (chirp)
            syntheticEquipBeep = GenerateProceduralTone("SyntheticEquip", 440, 880, 0.08f);
            // Hit thump
            syntheticHitBeep = GenerateProceduralTone("SyntheticHit", 180, 80, 0.12f);
            // Cooldown ready chime
            syntheticCooldownBeep = GenerateProceduralTone("SyntheticReady", 660, 990, 0.1f);
        }

        private AudioClip GenerateProceduralTone(string name, float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = 1.0f - t; // Linear decay
                samples[i] = Mathf.Sin(2 * Mathf.PI * freq * ((float)i / sampleRate)) * envelope;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion

        public void SetMasterVolume(float vol) => masterVolume = Mathf.Clamp01(vol);
        public void SetSFXVolume(float vol) => sfxVolume = Mathf.Clamp01(vol);
        public void SetMusicVolume(float vol)
        {
            musicVolume = Mathf.Clamp01(vol);
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.volume = masterVolume * musicVolume;
            }
        }
    }
}
