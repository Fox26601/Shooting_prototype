using UnityEngine;
using System.Collections.Generic;

namespace ShootingSystem
{
    /// <summary>
    /// Centralized audio management system using Singleton pattern
    /// Handles all sound effects with object pooling for performance
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSourcePrefab;
        [SerializeField] private int audioSourcePoolSize = 10;
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float maxSoundDuration = 1.5f; // Maximum sound duration in seconds
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip shootingSound;
        [SerializeField] private AudioClip targetHitSound;
        [SerializeField] private AudioClip targetLandingSound;
        [SerializeField] private AudioClip bulletImpactSound;
        [SerializeField] private AudioClip reloadSound;
        
        private static AudioManager instance;
        private Queue<AudioSource> audioSourcePool;
        private List<AudioSource> activeAudioSources;
        
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject audioManagerObject = new GameObject("AudioManager");
                        instance = audioManagerObject.AddComponent<AudioManager>();
                        DontDestroyOnLoad(audioManagerObject);
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSystem();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeAudioSystem()
        {
            // Ensure AudioListener exists in the scene
            EnsureAudioListenerExists();
            
            // Load sound clips from Resources
            LoadSoundClips();
            
            // Initialize audio source pool
            audioSourcePool = new Queue<AudioSource>();
            activeAudioSources = new List<AudioSource>();
            
            // Create audio source prefab if not assigned
            if (audioSourcePrefab == null)
            {
                CreateAudioSourcePrefab();
            }
            
            // Pre-populate pool
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                AudioSource audioSource = CreateAudioSource();
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        private void EnsureAudioListenerExists()
        {
            // Check if AudioListener already exists in the scene
            AudioListener existingListener = FindFirstObjectByType<AudioListener>();
            
            if (existingListener == null)
            {
                // Create AudioListener on the main camera or create a new one
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    AudioListener listener = mainCamera.GetComponent<AudioListener>();
                    if (listener == null)
                    {
                        listener = mainCamera.gameObject.AddComponent<AudioListener>();
                        Debug.Log("✅ AudioListener added to main camera");
                    }
                }
                else
                {
                    // Create a dedicated AudioListener if no main camera exists
                    GameObject audioListenerObject = new GameObject("AudioListener");
                    audioListenerObject.AddComponent<AudioListener>();
                    Debug.Log("✅ AudioListener created on dedicated GameObject");
                }
            }
            else
            {
                Debug.Log("✅ AudioListener already exists in scene");
            }
        }
        
        private void LoadSoundClips()
        {
            shootingSound = Resources.Load<AudioClip>("SoundPack/ELECSprk_Anime Spark 1");
            targetHitSound = Resources.Load<AudioClip>("SoundPack/EXPLDsgn_Anime Explosion 2");
            targetLandingSound = Resources.Load<AudioClip>("SoundPack/FGHTBf_Anime Land 7");
            bulletImpactSound = Resources.Load<AudioClip>("SoundPack/FGHTImpt_Anime Melee 2");
            reloadSound = Resources.Load<AudioClip>("SoundPack/MAGSpel_Anime Ability Release 5");
            
            if (shootingSound == null) 
            {
                Debug.LogError("❌ Shooting sound not found at path: SoundPack/ELECSprk_Anime Spark 1");
            }
            else
            {
                Debug.Log("✅ Shooting sound loaded successfully");
            }
            
            if (targetHitSound == null) 
            {
                Debug.LogError("❌ Target hit sound not found at path: SoundPack/EXPLDsgn_Anime Explosion 2");
            }
            else
            {
                Debug.Log("✅ Target hit sound loaded successfully");
            }
            
            if (targetLandingSound == null) 
            {
                Debug.LogError("❌ Target landing sound not found at path: SoundPack/FGHTBf_Anime Land 7");
            }
            else
            {
                Debug.Log("✅ Target landing sound loaded successfully");
            }
            
            if (bulletImpactSound == null) 
            {
                Debug.LogError("❌ Bullet impact sound not found at path: SoundPack/FGHTImpt_Anime Melee 2");
            }
            else
            {
                Debug.Log("✅ Bullet impact sound loaded successfully");
            }
            
            if (reloadSound == null) 
            {
                Debug.LogError("❌ Reload sound not found at path: SoundPack/MAGSpel_Anime Ability Release 5");
            }
            else
            {
                Debug.Log("✅ Reload sound loaded successfully");
            }
        }
        
        private void CreateAudioSourcePrefab()
        {
            GameObject prefabObject = new GameObject("AudioSource");
            audioSourcePrefab = prefabObject.AddComponent<AudioSource>();
            audioSourcePrefab.playOnAwake = false;
            audioSourcePrefab.spatialBlend = 0f; // 2D sound
            audioSourcePrefab.volume = sfxVolume;
        }
        
        private AudioSource CreateAudioSource()
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab, transform);
            audioSource.gameObject.SetActive(false);
            return audioSource;
        }
        
        private AudioSource GetAudioSource()
        {
            AudioSource audioSource;
            
            if (audioSourcePool == null)
            {
                // Pool not initialized yet, create structures defensively
                audioSourcePool = new Queue<AudioSource>();
                activeAudioSources = activeAudioSources ?? new List<AudioSource>();
            }
            
            if (audioSourcePool.Count > 0)
            {
                audioSource = audioSourcePool.Dequeue();
            }
            else
            {
                audioSource = CreateAudioSource();
            }
            
            activeAudioSources.Add(audioSource);
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }
        
        private void ReturnAudioSource(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            // Ensure collections exist
            if (activeAudioSources == null) activeAudioSources = new List<AudioSource>();
            if (audioSourcePool == null) audioSourcePool = new Queue<AudioSource>();
            
            activeAudioSources.Remove(audioSource);
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSource);
        }
        
        /// <summary>
        /// Play shooting sound effect
        /// </summary>
        public void PlayShootingSound()
        {
            Debug.Log("🔊 Attempting to play shooting sound...");
            PlaySound(shootingSound);
        }
        
        /// <summary>
        /// Play target hit sound effect
        /// </summary>
        public void PlayTargetHitSound()
        {
            Debug.Log("🔊 Attempting to play target hit sound...");
            PlaySound(targetHitSound);
        }
        
        /// <summary>
        /// Play target landing sound effect
        /// </summary>
        public void PlayTargetLandingSound()
        {
            Debug.Log("🔊 Attempting to play target landing sound...");
            PlaySound(targetLandingSound);
        }
        
        /// <summary>
        /// Play bullet impact sound effect when bullet hits any surface
        /// </summary>
        public void PlayBulletImpactSound()
        {
            Debug.Log("🔊 Attempting to play bullet impact sound...");
            PlaySound(bulletImpactSound);
        }
        
        /// <summary>
        /// Play reload sound effect
        /// </summary>
        public void PlayReloadSound()
        {
            Debug.Log("🔊 Attempting to play reload sound...");
            PlaySound(reloadSound);
        }
        
        /// <summary>
        /// Generic method to play any sound clip
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="volume">Volume multiplier (0-1)</param>
        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) 
            {
                Debug.LogWarning("❌ AudioClip is null, cannot play sound");
                return;
            }
            
            AudioSource audioSource = GetAudioSource();
            audioSource.clip = clip;
            audioSource.volume = sfxVolume * masterVolume * volume;
            
            Debug.Log($"🔊 Playing sound: {clip.name}, Volume: {audioSource.volume}, Duration: {clip.length}s");
            audioSource.Play();
            
            // Return to pool when finished or after max duration
            StartCoroutine(ReturnAudioSourceWhenFinished(audioSource));
        }
        
        private System.Collections.IEnumerator ReturnAudioSourceWhenFinished(AudioSource audioSource)
        {
            float elapsedTime = 0f;
            
            // Wait until sound finishes or max duration is reached
            while (audioSource.isPlaying && elapsedTime < maxSoundDuration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Stop the sound if it's still playing after max duration
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            ReturnAudioSource(audioSource);
        }
        
        /// <summary>
        /// Set master volume
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllAudioSourceVolumes();
        }
        
        /// <summary>
        /// Set SFX volume
        /// </summary>
        /// <param name="volume">Volume level (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateAllAudioSourceVolumes();
        }
        
        private void UpdateAllAudioSourceVolumes()
        {
            foreach (AudioSource audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.volume = sfxVolume * masterVolume;
                }
            }
        }
        
        /// <summary>
        /// Stop all currently playing sounds
        /// </summary>
        public void StopAllSounds()
        {
            if (activeAudioSources == null || activeAudioSources.Count == 0) return;
            
            // Iterate over a snapshot to avoid modifying the collection while iterating
            var snapshot = new List<AudioSource>(activeAudioSources);
            foreach (AudioSource audioSource in snapshot)
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                    ReturnAudioSource(audioSource);
                }
            }
        }
        
        private void OnDestroy()
        {
            // Stop and release safely
            StopAllSounds();
            
            // Clear references defensively
            if (activeAudioSources != null) activeAudioSources.Clear();
            if (audioSourcePool != null) audioSourcePool.Clear();
        }
    }
}
