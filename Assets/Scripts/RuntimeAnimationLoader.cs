using UnityEngine;
using System.Collections.Generic;

namespace ShootingSystem
{
    /// <summary>
    /// Runtime loader for pistol animations
    /// </summary>
    public class RuntimeAnimationLoader : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private string animationFolder = "Animations/Pistol";
        
        private Animation pistolAnimation;
        private List<AnimationClip> loadedClips = new List<AnimationClip>();
        
        private void Start()
        {
            LoadAnimationsAtRuntime();
        }
        
        /// <summary>
        /// Load animations at runtime using Resources
        /// </summary>
        private void LoadAnimationsAtRuntime()
        {
            Debug.Log("🎬 Loading animations at runtime...");
            
            // Find Pistol Instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found!");
                return;
            }
            
            // Get or add Animation component
            pistolAnimation = pistolInstance.GetComponent<Animation>();
            if (pistolAnimation == null)
            {
                pistolAnimation = pistolInstance.AddComponent<Animation>();
                Debug.Log("✅ Added Animation component to Pistol Instance");
            }
            else
            {
                Debug.Log("✅ Animation component already exists on Pistol Instance");
            }
            
            // Load animations from Resources
            LoadAnimationClips();
            
            // Ensure default wrap mode is Once
            if (pistolAnimation != null)
            {
                pistolAnimation.wrapMode = WrapMode.Once;
            }
            
            // Set default animation
            SetDefaultAnimation();
            
            Debug.Log($"✅ Runtime animation loading complete! Loaded {loadedClips.Count} animations.");
        }
        
        /// <summary>
        /// Load animation clips from Resources folder
        /// </summary>
        private void LoadAnimationClips()
        {
            // Load all animation clips from the specified folder
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(animationFolder);
            
            Debug.Log($"🔍 Found {clips.Length} animation clips in Resources/{animationFolder}");
            
            foreach (AnimationClip clip in clips)
            {
                if (clip != null)
                {
                    // Add clip to Animation component
                    pistolAnimation.AddClip(clip, clip.name);
                    loadedClips.Add(clip);
                    
                    Debug.Log($"✅ Loaded animation: {clip.name} (Duration: {clip.length}s)");
                }
            }
        }
        
        /// <summary>
        /// Set default animation
        /// </summary>
        private void SetDefaultAnimation()
        {
            if (loadedClips.Count > 0)
            {
                // Find shooting animation first
                AnimationClip shootingClip = loadedClips.Find(clip => 
                    clip.name.ToLower().Contains("fire") || 
                    clip.name.ToLower().Contains("shoot") ||
                    clip.name.ToLower().Contains("pistol_fire"));
                
                if (shootingClip != null)
                {
                    pistolAnimation.clip = shootingClip;
                    Debug.Log($"✅ Set default animation: {shootingClip.name}");
                }
                else
                {
                    // Use first available animation
                    pistolAnimation.clip = loadedClips[0];
                    Debug.Log($"✅ Set default animation: {loadedClips[0].name}");
                }
            }
        }
        
        /// <summary>
        /// Play animation by name
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            if (pistolAnimation != null)
            {
                if (pistolAnimation.GetClip(animationName) != null)
                {
                    var clip = pistolAnimation.GetClip(animationName);
                    if (clip != null) clip.wrapMode = WrapMode.Once;
                    // Also force state wrap mode
                    var state = pistolAnimation[animationName];
                    if (state != null) state.wrapMode = WrapMode.Once;
                    pistolAnimation.Play(animationName);
                    Debug.Log($"🎬 Playing animation: {animationName}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Animation '{animationName}' not found!");
                }
            }
            else
            {
                Debug.LogError("❌ Animation component not found!");
            }
        }
        
        /// <summary>
        /// Play shooting animation
        /// </summary>
        public void PlayShootingAnimation()
        {
            // Try different shooting animation names
            string[] shootingNames = { "Pistol_FIRE", "Pistol_FIRE_EMPTY", "Shoot", "Shooting", "Fire" };
            
            foreach (string name in shootingNames)
            {
                if (pistolAnimation.GetClip(name) != null)
                {
                    PlayAnimation(name);
                    return;
                }
            }
            
            Debug.LogWarning("⚠️ No shooting animation found!");
        }
        
        /// <summary>
        /// Play reload animation
        /// </summary>
        public void PlayReloadAnimation()
        {
            // Try different reload animation names
            string[] reloadNames = { "Pistol_RELOAD", "Reload", "Reloading" };
            
            foreach (string name in reloadNames)
            {
                if (pistolAnimation.GetClip(name) != null)
                {
                    PlayAnimation(name);
                    return;
                }
            }
            
            Debug.LogWarning("⚠️ No reload animation found!");
        }
        
        /// <summary>
        /// Play idle animation
        /// </summary>
        public void PlayIdleAnimation()
        {
            // Try different idle animation names
            string[] idleNames = { "Pistol_IDLE", "Pistol_IDLE_EMPTY", "Idle" };
            
            foreach (string name in idleNames)
            {
                if (pistolAnimation.GetClip(name) != null)
                {
                    PlayAnimation(name);
                    return;
                }
            }
            
            Debug.LogWarning("⚠️ No idle animation found!");
        }
        
        /// <summary>
        /// Play walk animation
        /// </summary>
        public void PlayWalkAnimation()
        {
            // Try different walk animation names
            string[] walkNames = { "Pistol_WALK", "Pistol_WALK_EMPTY", "Walk" };
            
            foreach (string name in walkNames)
            {
                if (pistolAnimation.GetClip(name) != null)
                {
                    PlayAnimation(name);
                    return;
                }
            }
            
            Debug.LogWarning("⚠️ No walk animation found!");
        }
        
        /// <summary>
        /// Play run animation
        /// </summary>
        public void PlayRunAnimation()
        {
            // Try different run animation names
            string[] runNames = { "Pistol_RUN", "Pistol_RUN_EMPTY", "Run" };
            
            foreach (string name in runNames)
            {
                if (pistolAnimation.GetClip(name) != null)
                {
                    PlayAnimation(name);
                    return;
                }
            }
            
            Debug.LogWarning("⚠️ No run animation found!");
        }
        
        /// <summary>
        /// Get loaded animation clips
        /// </summary>
        public List<AnimationClip> GetLoadedClips()
        {
            return loadedClips;
        }
        
        /// <summary>
        /// Check if animation is loaded
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            return pistolAnimation != null && pistolAnimation.GetClip(animationName) != null;
        }
    }
}
