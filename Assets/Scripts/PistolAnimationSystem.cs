using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    /// <summary>
    /// Advanced pistol animation system with different reload types and visual effects
    /// </summary>
    public class PistolAnimationSystem : MonoBehaviour
    {
        [Header("Animation Types")]
        [SerializeField] private ReloadType reloadType = ReloadType.FullReload;
        [SerializeField] private bool enableVisualEffects = true;
        [SerializeField] private bool enableParticleEffects = true;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject shellEjectPrefab;
        [SerializeField] private GameObject magazineEjectPrefab;
        [SerializeField] private Transform muzzleFlashPoint;
        [SerializeField] private Transform shellEjectPoint;
        
        [Header("Animation Timing")]
        [SerializeField] private float slideBackSpeed = 0.5f; // Замедлено для лучшей видимости
        [SerializeField] private float slideForwardSpeed = 0.3f; // Замедлено для лучшей видимости
        [SerializeField] private float magazineEjectSpeed = 3f;
        [SerializeField] private float magazineInsertSpeed = 2f;
        
        // Pistol parts
        private Transform slide;
        private Transform magazine;
        private Transform trigger;
        private Transform hammer;
        private Transform bullet;
        
        // Animation state
        private bool isAnimating = false;
        private Coroutine currentAnimation;
        
        public enum ReloadType
        {
            FullReload,      // Complete reload sequence
            TacticalReload,  // Quick magazine change
            EmergencyReload, // Fast reload without slide manipulation
            SlideRelease     // Just release slide
        }
        
        private void Start()
        {
            FindPistolParts();
            SetupVisualEffects();
        }
        
        private void FindPistolParts()
        {
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null) 
            {
                Debug.LogError("❌ Pistol Instance not found! Animations will not work.");
                return;
            }
            
            slide = FindTransformByName(pistolInstance.transform, "slide");
            magazine = FindTransformByName(pistolInstance.transform, "mag");
            trigger = FindTransformByName(pistolInstance.transform, "trigger");
            hammer = FindTransformByName(pistolInstance.transform, "hammer");
            bullet = FindTransformByName(pistolInstance.transform, "bullet");
            
            // Debug found parts
            Debug.Log($"🔍 Found pistol parts - Slide: {slide != null}, Magazine: {magazine != null}, Trigger: {trigger != null}, Hammer: {hammer != null}, Bullet: {bullet != null}");
            
            // Set up effect points
            muzzleFlashPoint = bullet;
            shellEjectPoint = slide;
        }
        
        private void SetupVisualEffects()
        {
            if (!enableVisualEffects) return;
            
            // Create muzzle flash effect point
            if (muzzleFlashPoint != null && muzzleFlashPrefab == null)
            {
                GameObject flashPoint = new GameObject("MuzzleFlashPoint");
                flashPoint.transform.SetParent(muzzleFlashPoint);
                flashPoint.transform.localPosition = Vector3.forward * 0.1f;
                muzzleFlashPoint = flashPoint.transform;
            }
            
            // Create shell eject effect point
            if (shellEjectPoint != null && shellEjectPrefab == null)
            {
                GameObject ejectPoint = new GameObject("ShellEjectPoint");
                ejectPoint.transform.SetParent(shellEjectPoint);
                ejectPoint.transform.localPosition = Vector3.right * 0.05f;
                shellEjectPoint = ejectPoint.transform;
            }
        }
        
        /// <summary>
        /// Play shooting animation with visual effects
        /// </summary>
        public void PlayShootingAnimation()
        {
            if (isAnimating) return;
            
            if (slide == null)
            {
                Debug.LogWarning("⚠️ Slide not found! Shooting animation cannot play.");
                return;
            }
            
            Debug.Log("🎬 Playing shooting animation...");
            StartCoroutine(ShootingAnimationSequence());
        }
        
        private IEnumerator ShootingAnimationSequence()
        {
            isAnimating = true;
            
            // Muzzle flash effect
            if (enableVisualEffects && muzzleFlashPoint != null)
            {
                CreateMuzzleFlash();
            }
            
            // Slide back animation
            yield return StartCoroutine(AnimateSlideBack(slideBackSpeed));
            
            // Shell eject effect
            if (enableVisualEffects && shellEjectPoint != null)
            {
                CreateShellEject();
            }
            
            // Slide forward animation
            yield return StartCoroutine(AnimateSlideForward(slideForwardSpeed));
            
            isAnimating = false;
        }
        
        /// <summary>
        /// Play reload animation based on reload type
        /// </summary>
        public void PlayReloadAnimation(ReloadType type = ReloadType.FullReload)
        {
            if (isAnimating) return;
            
            reloadType = type;
            StartCoroutine(ReloadAnimationSequence());
        }
        
        private IEnumerator ReloadAnimationSequence()
        {
            isAnimating = true;
            Debug.Log($"🎬 Starting {reloadType} reload animation...");
            
            switch (reloadType)
            {
                case ReloadType.FullReload:
                    yield return StartCoroutine(FullReloadSequence());
                    break;
                case ReloadType.TacticalReload:
                    yield return StartCoroutine(TacticalReloadSequence());
                    break;
                case ReloadType.EmergencyReload:
                    yield return StartCoroutine(EmergencyReloadSequence());
                    break;
                case ReloadType.SlideRelease:
                    yield return StartCoroutine(SlideReleaseSequence());
                    break;
            }
            
            Debug.Log($"✅ {reloadType} reload animation complete!");
            isAnimating = false;
        }
        
        private IEnumerator FullReloadSequence()
        {
            // Step 1: Slide back
            yield return StartCoroutine(AnimateSlideBack(slideBackSpeed));
            
            // Step 2: Magazine eject
            yield return StartCoroutine(AnimateMagazineEject());
            
            // Step 3: Magazine insert
            yield return StartCoroutine(AnimateMagazineInsert());
            
            // Step 4: Slide forward
            yield return StartCoroutine(AnimateSlideForward(slideForwardSpeed));
        }
        
        private IEnumerator TacticalReloadSequence()
        {
            // Step 1: Quick magazine eject
            yield return StartCoroutine(AnimateMagazineEject(magazineEjectSpeed * 1.5f));
            
            // Step 2: Quick magazine insert
            yield return StartCoroutine(AnimateMagazineInsert(magazineInsertSpeed * 1.5f));
            
            // Step 3: Slide forward
            yield return StartCoroutine(AnimateSlideForward(slideForwardSpeed));
        }
        
        private IEnumerator EmergencyReloadSequence()
        {
            // Step 1: Magazine eject
            yield return StartCoroutine(AnimateMagazineEject(magazineEjectSpeed * 2f));
            
            // Step 2: Magazine insert
            yield return StartCoroutine(AnimateMagazineInsert(magazineInsertSpeed * 2f));
        }
        
        private IEnumerator SlideReleaseSequence()
        {
            // Just release slide
            yield return StartCoroutine(AnimateSlideForward(slideForwardSpeed));
        }
        
        private IEnumerator AnimateSlideBack(float speed)
        {
            if (slide == null) yield break;
            
            Vector3 startPos = slide.localPosition;
            Vector3 endPos = startPos + Vector3.back * 0.5f; // Увеличено до 0.5f для лучшей видимости
            Debug.Log($"🎬 Starting slide back animation... From: {startPos} To: {endPos}");
            
            float duration = 1f / speed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                slide.localPosition = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            slide.localPosition = endPos;
            Debug.Log("🎬 Slide back animation completed!");
        }
        
        private IEnumerator AnimateSlideForward(float speed)
        {
            if (slide == null) yield break;
            
            Vector3 startPos = slide.localPosition;
            Vector3 endPos = startPos + Vector3.forward * 0.5f; // Увеличено до 0.5f для лучшей видимости
            Debug.Log($"🎬 Starting slide forward animation... From: {startPos} To: {endPos}");
            
            float duration = 1f / speed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                slide.localPosition = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            slide.localPosition = endPos;
            Debug.Log("🎬 Slide forward animation completed!");
        }
        
        private IEnumerator AnimateMagazineEject(float speed = 3f)
        {
            if (magazine == null) yield break;
            
            Vector3 startPos = magazine.localPosition;
            Vector3 endPos = startPos + Vector3.down * 0.1f;
            
            float duration = 1f / speed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                magazine.localPosition = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            magazine.localPosition = endPos;
        }
        
        private IEnumerator AnimateMagazineInsert(float speed = 2f)
        {
            if (magazine == null) yield break;
            
            Vector3 startPos = magazine.localPosition;
            Vector3 endPos = startPos + Vector3.up * 0.1f;
            
            float duration = 1f / speed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                magazine.localPosition = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            magazine.localPosition = endPos;
        }
        
        private void CreateMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && muzzleFlashPoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzleFlashPoint.position, muzzleFlashPoint.rotation);
                Destroy(flash, 0.1f);
            }
            else if (enableParticleEffects)
            {
                // Create simple muzzle flash effect
                GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flash.transform.position = muzzleFlashPoint.position;
                flash.transform.localScale = Vector3.one * 0.1f;
                flash.GetComponent<Renderer>().material.color = Color.yellow;
                Destroy(flash, 0.1f);
            }
        }
        
        private void CreateShellEject()
        {
            if (shellEjectPrefab != null && shellEjectPoint != null)
            {
                GameObject shell = Instantiate(shellEjectPrefab, shellEjectPoint.position, shellEjectPoint.rotation);
                Destroy(shell, 2f);
            }
            else if (enableParticleEffects)
            {
                // Create simple shell eject effect
                GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                shell.transform.position = shellEjectPoint.position;
                shell.transform.localScale = Vector3.one * 0.05f;
                shell.GetComponent<Renderer>().material.color = Color.gray;
                Destroy(shell, 2f);
            }
        }
        
        /// <summary>
        /// Set reload type
        /// </summary>
        public void SetReloadType(ReloadType type)
        {
            reloadType = type;
        }
        
        /// <summary>
        /// Check if animation is playing
        /// </summary>
        public bool IsAnimating => isAnimating;
        
        private Transform FindTransformByName(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
                
            foreach (Transform child in parent)
            {
                Transform found = FindTransformByName(child, name);
                if (found != null)
                    return found;
            }
            
            return null;
        }
        
        /// <summary>
        /// Test different reload types
        /// </summary>
        [ContextMenu("Test Full Reload")]
        public void TestFullReload() => PlayReloadAnimation(ReloadType.FullReload);
        
        [ContextMenu("Test Tactical Reload")]
        public void TestTacticalReload() => PlayReloadAnimation(ReloadType.TacticalReload);
        
        [ContextMenu("Test Emergency Reload")]
        public void TestEmergencyReload() => PlayReloadAnimation(ReloadType.EmergencyReload);
        
        [ContextMenu("Test Slide Release")]
        public void TestSlideRelease() => PlayReloadAnimation(ReloadType.SlideRelease);
    }
}
