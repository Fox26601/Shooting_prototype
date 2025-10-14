using UnityEngine;

namespace ShootingSystem
{
    /// <summary>
    /// Использует готовые анимации из pistol prefab через Unity Animator
    /// </summary>
    public class PistolPrefabAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator pistolAnimator;
        [SerializeField] private Animation pistolAnimation;
        [SerializeField] private SimplePistolAnimator simpleAnimator;
        [SerializeField] private RuntimeAnimationLoader runtimeLoader;
        
        [Header("Animation States")]
        [SerializeField] private string idleState = "Idle";
        [SerializeField] private string shootingState = "Shooting";
        [SerializeField] private string reloadingState = "Reloading";
        
        private void Start()
        {
            Debug.Log("🎬 PistolPrefabAnimator Start() called");
            FindPistolAnimator();
            FindRuntimeLoader();
            InitializeSimpleAnimator();
            Debug.Log("🎬 PistolPrefabAnimator initialization complete");
        }
        
        private void FindPistolAnimator()
        {
            // Ищем Animator в Pistol Instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance != null)
            {
                pistolAnimator = pistolInstance.GetComponent<Animator>();
                if (pistolAnimator == null)
                {
                    // Ищем Animator в дочерних объектах
                    pistolAnimator = pistolInstance.GetComponentInChildren<Animator>();
                }
                
                if (pistolAnimator != null)
                {
                    Debug.Log("✅ Found pistol Animator! Available parameters:");
                    LogAnimatorParameters();
                }
                else
                {
                    Debug.LogWarning("⚠️ No Animator found in pistol prefab. Using fallback animations.");
                }
            }
            else
            {
                Debug.LogError("❌ Pistol Instance not found!");
            }
        }
        
        private void FindRuntimeLoader()
        {
            // Ищем RuntimeAnimationLoader в сцене
            runtimeLoader = FindFirstObjectByType<RuntimeAnimationLoader>();
            if (runtimeLoader != null)
            {
                Debug.Log("✅ Found RuntimeAnimationLoader!");
            }
            else
            {
                Debug.LogWarning("⚠️ RuntimeAnimationLoader not found!");
            }
        }
        
        private void InitializeSimpleAnimator()
        {
            // Ищем Pistol Instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found!");
                return;
            }

            // Ищем Simple Animator
            simpleAnimator = pistolInstance.GetComponent<SimplePistolAnimator>();
            if (simpleAnimator == null)
            {
                simpleAnimator = pistolInstance.AddComponent<SimplePistolAnimator>();
                Debug.Log("✅ Added SimplePistolAnimator for animations");
            }
        }
        
        private void LogAnimatorParameters()
        {
            if (pistolAnimator == null) return;
            
            Debug.Log("🎬 Animator Parameters:");
            foreach (AnimatorControllerParameter param in pistolAnimator.parameters)
            {
                Debug.Log($"  - {param.name} ({param.type})");
            }
        }
        
        private void LogAnimationClips()
        {
            if (pistolAnimation == null) return;
            
            Debug.Log("🎬 Animation Clips:");
            foreach (AnimationState state in pistolAnimation)
            {
                if (state.clip != null)
                {
                    Debug.Log($"  - {state.clip.name} (Duration: {state.clip.length}s)");
                }
            }
        }
        
        /// <summary>
        /// Воспроизвести анимацию выстрела
        /// </summary>
        public void PlayShootingAnimation()
        {
            Debug.Log("🎬 PlayShootingAnimation called!");
            Debug.Log($"🎬 pistolAnimation: {pistolAnimation != null}");
            Debug.Log($"🎬 pistolAnimator: {pistolAnimator != null}");
            Debug.Log($"🎬 simpleAnimator: {simpleAnimator != null}");
            Debug.Log($"🎬 runtimeLoader: {runtimeLoader != null}");
            
            // Сначала пробуем использовать RuntimeAnimationLoader
            if (runtimeLoader != null)
            {
                Debug.Log("🎬 Using RuntimeAnimationLoader for shooting...");
                runtimeLoader.PlayShootingAnimation();
                return;
            }
            
            // Затем пробуем использовать Animation компонент
            if (pistolAnimation != null)
            {
                Debug.Log("🎬 Using Animation component for shooting...");
                if (pistolAnimation.GetClip("Shoot") != null)
                {
                    var clip = pistolAnimation.GetClip("Shoot");
                    if (clip != null) clip.wrapMode = WrapMode.Once;
                    var state = pistolAnimation["Shoot"];
                    if (state != null) state.wrapMode = WrapMode.Once;
                    pistolAnimation.Stop("Shoot");
                    pistolAnimation.Play("Shoot");
                    Debug.Log("✅ Playing 'Shoot' animation via Animation component");
                    return;
                }
                else if (pistolAnimation.GetClip("Shooting") != null)
                {
                    var clip = pistolAnimation.GetClip("Shooting");
                    if (clip != null) clip.wrapMode = WrapMode.Once;
                    var state = pistolAnimation["Shooting"];
                    if (state != null) state.wrapMode = WrapMode.Once;
                    pistolAnimation.Stop("Shooting");
                    pistolAnimation.Play("Shooting");
                    Debug.Log("✅ Playing 'Shooting' animation via Animation component");
                    return;
                }
                else if (pistolAnimation.GetClip("Pistol_FIRE") != null)
                {
                    var clip = pistolAnimation.GetClip("Pistol_FIRE");
                    if (clip != null) clip.wrapMode = WrapMode.Once;
                    var state = pistolAnimation["Pistol_FIRE"];
                    if (state != null) state.wrapMode = WrapMode.Once;
                    pistolAnimation.Stop("Pistol_FIRE");
                    pistolAnimation.Play("Pistol_FIRE");
                    Debug.Log("✅ Playing 'Pistol_FIRE' animation via Animation component");
                    return;
                }
            }
            
            // Затем пробуем Animator
            if (pistolAnimator != null)
            {
                
                if (HasParameter("Shoot"))
                {
                    pistolAnimator.SetTrigger("Shoot");
                    Debug.Log("✅ Triggered 'Shoot' parameter");
                    return;
                }
                else if (HasParameter("Fire"))
                {
                    pistolAnimator.SetTrigger("Fire");
                    Debug.Log("✅ Triggered 'Fire' parameter");
                    return;
                }
                else if (HasParameter("Pistol_FIRE"))
                {
                    pistolAnimator.SetTrigger("Pistol_FIRE");
                    Debug.Log("✅ Triggered 'Pistol_FIRE' parameter");
                    return;
                }
                else
                {
                    Debug.LogWarning("⚠️ No suitable animation parameters found in Animator");
                }
            }
            
            // Используем Simple Animator
            if (simpleAnimator != null)
            {
                Debug.Log("🎬 Using Simple Animator for shooting...");
                simpleAnimator.PlayShootingAnimation();
                Debug.Log("✅ Playing shooting animation via Simple Animator");
                return;
            }
            
            // Fallback на простые анимации
            Debug.LogWarning("⚠️ No ready animations found, using fallback animation");
            StartCoroutine(PlayFallbackShootingAnimation());
        }

        /// <summary>
        /// Play shooting once, stopping previous state if needed. Returns true and outputs duration if known.
        /// </summary>
        public bool TryPlayShootingOnce(out float duration)
        {
            duration = 0f;
            // Prefer Animation component with explicit clip
            if (pistolAnimation != null)
            {
                AnimationClip clip = pistolAnimation.GetClip("Pistol_FIRE")
                    ?? pistolAnimation.GetClip("Shoot")
                    ?? pistolAnimation.GetClip("Shooting");
                if (clip != null)
                {
                    clip.wrapMode = WrapMode.Once;
                    var state = pistolAnimation[clip.name];
                    if (state != null) state.wrapMode = WrapMode.Once;
                    pistolAnimation.Stop(clip.name);
                    pistolAnimation.Play(clip.name);
                    duration = clip.length;
                    return true;
                }
            }
            // Runtime loader
            if (runtimeLoader != null && runtimeLoader.HasAnimation("Pistol_FIRE"))
            {
                runtimeLoader.PlayAnimation("Pistol_FIRE");
                var list = runtimeLoader.GetLoadedClips();
                var found = list.Find(c => c != null && c.name == "Pistol_FIRE");
                if (found != null) duration = found.length;
                if (duration <= 0f) duration = 0.75f;
                return true;
            }
            // Animator as last resort - can't get duration reliably
            if (pistolAnimator != null && (HasParameter("Shoot") || HasParameter("Fire") || HasParameter("Pistol_FIRE")))
            {
                if (HasParameter("Shoot")) pistolAnimator.SetTrigger("Shoot");
                else if (HasParameter("Fire")) pistolAnimator.SetTrigger("Fire");
                else pistolAnimator.SetTrigger("Pistol_FIRE");
                duration = 0.75f;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Play reload once, stopping previous state if needed. Returns true and outputs duration if known.
        /// </summary>
        public bool TryPlayReloadOnce(out float duration)
        {
            duration = 0f;
            if (pistolAnimation != null)
            {
                var clip = pistolAnimation.GetClip("Pistol_RELOAD") ?? pistolAnimation.GetClip("Reload");
                if (clip != null)
                {
                    clip.wrapMode = WrapMode.Once;
                    var state = pistolAnimation[clip.name];
                    if (state != null) state.wrapMode = WrapMode.Once;
                    pistolAnimation.Stop(clip.name);
                    pistolAnimation.Play(clip.name);
                    duration = clip.length;
                    return true;
                }
            }
            if (runtimeLoader != null && runtimeLoader.HasAnimation("Pistol_RELOAD"))
            {
                runtimeLoader.PlayAnimation("Pistol_RELOAD");
                var list = runtimeLoader.GetLoadedClips();
                var found = list.Find(c => c != null && c.name == "Pistol_RELOAD");
                if (found != null) duration = found.length;
                if (duration <= 0f) duration = 1.0f;
                return true;
            }
            if (pistolAnimator != null && HasParameter("Reload"))
            {
                pistolAnimator.SetTrigger("Reload");
                duration = 1.0f;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Воспроизвести анимацию перезарядки
        /// </summary>
        public void PlayReloadAnimation()
        {
            // Используем Simple Animator
            if (simpleAnimator != null)
            {
                Debug.Log("🔄 Using Simple Animator for reload...");
                simpleAnimator.PlayReloadAnimation();
                Debug.Log("✅ Playing reload animation via Simple Animator");
                return;
            }
            
            // Fallback на простые анимации
            Debug.LogWarning("⚠️ No Simple Animator found, using fallback animation");
            StartCoroutine(PlayFallbackReloadAnimation());
        }
        
        /// <summary>
        /// Анимация затвора назад
        /// </summary>
        public void PlaySlideBackAnimation()
        {
            if (pistolAnimator == null) return;
            
            if (HasParameter("SlideBack"))
            {
                pistolAnimator.SetTrigger("SlideBack");
                Debug.Log("✅ Triggered 'SlideBack' parameter");
            }
            else if (HasParameter("SlideBack"))
            {
                pistolAnimator.SetTrigger("SlideBack");
                Debug.Log("✅ Triggered 'SlideBack' parameter");
            }
        }
        
        /// <summary>
        /// Анимация затвора вперед
        /// </summary>
        public void PlaySlideForwardAnimation()
        {
            if (pistolAnimator == null) return;
            
            if (HasParameter("SlideForward"))
            {
                pistolAnimator.SetTrigger("SlideForward");
                Debug.Log("✅ Triggered 'SlideForward' parameter");
            }
            else if (HasParameter("SlideForward"))
            {
                pistolAnimator.SetTrigger("SlideForward");
                Debug.Log("✅ Triggered 'SlideForward' parameter");
            }
        }
        
        /// <summary>
        /// Проверить, есть ли параметр в Animator
        /// </summary>
        private bool HasParameter(string parameterName)
        {
            if (pistolAnimator == null) return false;
            
            foreach (AnimatorControllerParameter param in pistolAnimator.parameters)
            {
                if (param.name == parameterName)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Получить текущее состояние анимации
        /// </summary>
        public string GetCurrentAnimationState()
        {
            if (pistolAnimator == null) return "No Animator";
            
            AnimatorStateInfo stateInfo = pistolAnimator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(idleState) ? "Idle" : 
                   stateInfo.IsName(shootingState) ? "Shooting" : 
                   stateInfo.IsName(reloadingState) ? "Reloading" : "Unknown";
        }
        
        /// <summary>
        /// Проверить, играет ли анимация
        /// </summary>
        public bool IsAnimating()
        {
            if (pistolAnimator == null) return false;
            
            AnimatorStateInfo stateInfo = pistolAnimator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime < 1.0f;
        }
        
        /// <summary>
        /// Fallback анимация выстрела (простое движение slide)
        /// </summary>
        private System.Collections.IEnumerator PlayFallbackShootingAnimation()
        {
            Debug.Log("🎬 Playing fallback shooting animation...");
            
            // Находим slide в pistol instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null) yield break;
            
            Transform slide = FindTransformByName(pistolInstance.transform, "slide");
            if (slide == null) yield break;
            
            Vector3 startPos = slide.localPosition;
            Vector3 endPos = startPos + Vector3.back * 1.0f; // Slide назад (увеличено для видимости)
            
            Debug.Log($"🎬 Fallback slide animation: {startPos} -> {endPos}");
            
            // Анимация slide назад
            float duration = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                slide.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            // Небольшая пауза
            yield return new WaitForSeconds(0.1f);
            
            // Анимация slide вперед
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                slide.localPosition = Vector3.Lerp(endPos, startPos, t);
                yield return null;
            }
            
            Debug.Log("🎬 Fallback shooting animation completed!");
        }
        
        /// <summary>
        /// Fallback анимация перезарядки
        /// </summary>
        private System.Collections.IEnumerator PlayFallbackReloadAnimation()
        {
            Debug.Log("🔄 Playing fallback reload animation...");
            
            // Находим части пистолета
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null) yield break;
            
            Transform slide = FindTransformByName(pistolInstance.transform, "slide");
            Transform magazine = FindTransformByName(pistolInstance.transform, "mag");
            
            if (slide != null)
            {
                // Анимация slide назад и вперед
                Vector3 slideStart = slide.localPosition;
                Vector3 slideEnd = slideStart + Vector3.back * 1.2f; // Увеличено для видимости
                
                // Slide назад
                float duration = 0.3f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    slide.localPosition = Vector3.Lerp(slideStart, slideEnd, t);
                    yield return null;
                }
                
                yield return new WaitForSeconds(0.2f);
                
                // Slide вперед
                elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    slide.localPosition = Vector3.Lerp(slideEnd, slideStart, t);
                    yield return null;
                }
            }
            
            Debug.Log("🔄 Fallback reload animation completed!");
        }
        
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
        /// Тестовые методы для проверки анимаций
        /// </summary>
        [ContextMenu("Test Shooting Animation")]
        public void TestShootingAnimation() => PlayShootingAnimation();
        
        [ContextMenu("Test Reload Animation")]
        public void TestReloadAnimation() => PlayReloadAnimation();
        
        [ContextMenu("Test Slide Back")]
        public void TestSlideBack() => PlaySlideBackAnimation();
        
        [ContextMenu("Test Slide Forward")]
        public void TestSlideForward() => PlaySlideForwardAnimation();
    }
}
