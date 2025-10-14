using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    /// <summary>
    /// Handles pistol reload animations using the detected pistol structure
    /// </summary>
    public class PistolReloadAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float slideBackDuration = 0.3f;
        [SerializeField] private float slideForwardDuration = 0.4f;
        [SerializeField] private float magazineEjectDuration = 0.2f;
        [SerializeField] private float magazineInsertDuration = 0.3f;
        [SerializeField] private float triggerResetDuration = 0.1f;
        
        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve slideBackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve slideForwardCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve magazineEjectCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve magazineInsertCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Animation Offsets")]
        [SerializeField] private Vector3 slideBackOffset = new Vector3(0f, 0f, -0.05f);
        [SerializeField] private Vector3 magazineEjectOffset = new Vector3(0f, -0.1f, 0f);
        [SerializeField] private Vector3 triggerResetOffset = new Vector3(0f, 0f, -0.01f);
        
        // Pistol parts references
        private Transform slide;
        private Transform magazine;
        private Transform trigger;
        private Transform hammer;
        private Transform bullet;
        
        // Original positions
        private Vector3 originalSlidePosition;
        private Vector3 originalMagazinePosition;
        private Vector3 originalTriggerPosition;
        private Vector3 originalHammerPosition;
        
        // Animation state
        private bool isAnimating = false;
        private Coroutine currentAnimation;
        
        private void Start()
        {
            FindPistolParts();
            StoreOriginalPositions();
        }
        
        private void FindPistolParts()
        {
            // Find pistol instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found!");
                return;
            }
            
            // Find pistol parts
            slide = FindTransformByName(pistolInstance.transform, "slide");
            magazine = FindTransformByName(pistolInstance.transform, "mag");
            trigger = FindTransformByName(pistolInstance.transform, "trigger");
            hammer = FindTransformByName(pistolInstance.transform, "hammer");
            bullet = FindTransformByName(pistolInstance.transform, "bullet");
            
            Debug.Log("🔍 Pistol parts found:");
            Debug.Log($"  Slide: {(slide != null ? slide.name : "Not found")}");
            Debug.Log($"  Magazine: {(magazine != null ? magazine.name : "Not found")}");
            Debug.Log($"  Trigger: {(trigger != null ? trigger.name : "Not found")}");
            Debug.Log($"  Hammer: {(hammer != null ? hammer.name : "Not found")}");
            Debug.Log($"  Bullet: {(bullet != null ? bullet.name : "Not found")}");
        }
        
        private void StoreOriginalPositions()
        {
            if (slide != null) originalSlidePosition = slide.localPosition;
            if (magazine != null) originalMagazinePosition = magazine.localPosition;
            if (trigger != null) originalTriggerPosition = trigger.localPosition;
            if (hammer != null) originalHammerPosition = hammer.localPosition;
        }
        
        /// <summary>
        /// Play complete reload animation sequence
        /// </summary>
        [ContextMenu("Play Reload Animation")]
        public void PlayReloadAnimation()
        {
            if (isAnimating)
            {
                Debug.LogWarning("⚠️ Animation already in progress!");
                return;
            }
            
            StartCoroutine(ReloadAnimationSequence());
        }
        
        private IEnumerator ReloadAnimationSequence()
        {
            isAnimating = true;
            Debug.Log("🎬 Starting reload animation sequence...");
            
            // Step 1: Slide back (eject empty casing)
            yield return StartCoroutine(AnimateSlideBack());
            
            // Step 2: Magazine eject
            yield return StartCoroutine(AnimateMagazineEject());
            
            // Step 3: Magazine insert
            yield return StartCoroutine(AnimateMagazineInsert());
            
            // Step 4: Slide forward (chamber new round)
            yield return StartCoroutine(AnimateSlideForward());
            
            // Step 5: Trigger reset
            yield return StartCoroutine(AnimateTriggerReset());
            
            Debug.Log("✅ Reload animation sequence complete!");
            isAnimating = false;
        }
        
        private IEnumerator AnimateSlideBack()
        {
            if (slide == null) yield break;
            
            Debug.Log("🎬 Animating slide back...");
            Vector3 startPos = slide.localPosition;
            Vector3 endPos = startPos + slideBackOffset;
            
            float elapsed = 0f;
            while (elapsed < slideBackDuration)
            {
                float t = elapsed / slideBackDuration;
                float curveValue = slideBackCurve.Evaluate(t);
                slide.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            slide.localPosition = endPos;
        }
        
        private IEnumerator AnimateSlideForward()
        {
            if (slide == null) yield break;
            
            Debug.Log("🎬 Animating slide forward...");
            Vector3 startPos = slide.localPosition;
            Vector3 endPos = originalSlidePosition;
            
            float elapsed = 0f;
            while (elapsed < slideForwardDuration)
            {
                float t = elapsed / slideForwardDuration;
                float curveValue = slideForwardCurve.Evaluate(t);
                slide.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            slide.localPosition = endPos;
        }
        
        private IEnumerator AnimateMagazineEject()
        {
            if (magazine == null) yield break;
            
            Debug.Log("🎬 Animating magazine eject...");
            Vector3 startPos = magazine.localPosition;
            Vector3 endPos = startPos + magazineEjectOffset;
            
            float elapsed = 0f;
            while (elapsed < magazineEjectDuration)
            {
                float t = elapsed / magazineEjectDuration;
                float curveValue = magazineEjectCurve.Evaluate(t);
                magazine.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            magazine.localPosition = endPos;
        }
        
        private IEnumerator AnimateMagazineInsert()
        {
            if (magazine == null) yield break;
            
            Debug.Log("🎬 Animating magazine insert...");
            Vector3 startPos = magazine.localPosition;
            Vector3 endPos = originalMagazinePosition;
            
            float elapsed = 0f;
            while (elapsed < magazineInsertDuration)
            {
                float t = elapsed / magazineInsertDuration;
                float curveValue = magazineInsertCurve.Evaluate(t);
                magazine.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            magazine.localPosition = endPos;
        }
        
        private IEnumerator AnimateTriggerReset()
        {
            if (trigger == null) yield break;
            
            Debug.Log("🎬 Animating trigger reset...");
            Vector3 startPos = trigger.localPosition;
            Vector3 endPos = startPos + triggerResetOffset;
            
            float elapsed = 0f;
            while (elapsed < triggerResetDuration)
            {
                float t = elapsed / triggerResetDuration;
                trigger.localPosition = Vector3.Lerp(startPos, endPos, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            trigger.localPosition = endPos;
        }
        
        /// <summary>
        /// Play slide back animation (for shooting)
        /// </summary>
        [ContextMenu("Play Slide Back")]
        public void PlaySlideBack()
        {
            if (slide == null) return;
            
            StartCoroutine(AnimateSlideBack());
        }
        
        /// <summary>
        /// Play slide forward animation (after reload)
        /// </summary>
        [ContextMenu("Play Slide Forward")]
        public void PlaySlideForward()
        {
            if (slide == null) return;
            
            StartCoroutine(AnimateSlideForward());
        }
        
        /// <summary>
        /// Reset all parts to original positions
        /// </summary>
        [ContextMenu("Reset Pistol Parts")]
        public void ResetPistolParts()
        {
            if (slide != null) slide.localPosition = originalSlidePosition;
            if (magazine != null) magazine.localPosition = originalMagazinePosition;
            if (trigger != null) trigger.localPosition = originalTriggerPosition;
            if (hammer != null) hammer.localPosition = originalHammerPosition;
            
            Debug.Log("✅ Pistol parts reset to original positions");
        }
        
        /// <summary>
        /// Check if animation is currently playing
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
    }
}

