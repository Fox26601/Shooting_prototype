using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    /// <summary>
    /// Простой аниматор для пистолета - находит и использует готовые анимации из префаба
    /// </summary>
    public class SimplePistolAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        // [SerializeField] private float animationSpeed = 1f; // Пока не используется
        
        private Animation pistolAnimation;
        private Animator pistolAnimator;
        private bool hasReadyAnimations = false;
        
        private void Start()
        {
            FindAnimationComponents();
        }
        
        private void FindAnimationComponents()
        {
            // Ищем Animation компонент
            pistolAnimation = GetComponent<Animation>();
            if (pistolAnimation == null)
            {
                pistolAnimation = GetComponentInChildren<Animation>();
            }
            
            // Ищем Animator компонент
            pistolAnimator = GetComponent<Animator>();
            if (pistolAnimator == null)
            {
                pistolAnimator = GetComponentInChildren<Animator>();
            }
            
            // Проверяем, есть ли готовые анимации
            if (pistolAnimation != null)
            {
                Debug.Log("✅ Found Animation component with ready animations!");
                hasReadyAnimations = true;
                LogAvailableAnimations();
            }
            else if (pistolAnimator != null)
            {
                Debug.Log("✅ Found Animator component!");
                hasReadyAnimations = true;
            }
            else
            {
                Debug.Log("⚠️ No ready animations found, will use simple animations");
            }
        }
        
        private void LogAvailableAnimations()
        {
            if (pistolAnimation == null) return;
            
            Debug.Log("🎬 Available animations:");
            foreach (AnimationState state in pistolAnimation)
            {
                if (state.clip != null)
                {
                    Debug.Log($"  - {state.clip.name} ({state.clip.length}s)");
                }
            }
        }
        
        /// <summary>
        /// Воспроизвести анимацию выстрела
        /// </summary>
        public void PlayShootingAnimation()
        {
            if (hasReadyAnimations && pistolAnimation != null)
            {
                // Пробуем разные имена анимаций выстрела
                string[] shootNames = {"Shoot", "Shooting", "Fire", "Gun_Shoot", "Pistol_Shoot"};
                
                foreach (string animName in shootNames)
                {
                    if (pistolAnimation.GetClip(animName) != null)
                    {
                        pistolAnimation.Play(animName);
                        Debug.Log($"🎬 Playing shooting animation: {animName}");
                        return;
                    }
                }
            }
            
            // Fallback - простая анимация
            StartCoroutine(SimpleShootingAnimation());
        }
        
        /// <summary>
        /// Воспроизвести анимацию перезарядки
        /// </summary>
        public void PlayReloadAnimation()
        {
            if (hasReadyAnimations && pistolAnimation != null)
            {
                // Пробуем разные имена анимаций перезарядки
                string[] reloadNames = {"Reload", "Reloading", "Gun_Reload", "Pistol_Reload"};
                
                foreach (string animName in reloadNames)
                {
                    if (pistolAnimation.GetClip(animName) != null)
                    {
                        pistolAnimation.Play(animName);
                        Debug.Log($"🎬 Playing reload animation: {animName}");
                        return;
                    }
                }
            }
            
            // Fallback - простая анимация
            StartCoroutine(SimpleReloadAnimation());
        }
        
        private IEnumerator SimpleShootingAnimation()
        {
            Debug.Log("🎬 Playing simple shooting animation");
            
            // Простая анимация - небольшое движение
            Vector3 originalPos = transform.localPosition;
            Vector3 targetPos = originalPos + Vector3.back * 0.1f;
            
            float duration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localPosition = Vector3.Lerp(originalPos, targetPos, t);
                yield return null;
            }
            
            // Возвращаем обратно
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localPosition = Vector3.Lerp(targetPos, originalPos, t);
                yield return null;
            }
            
            transform.localPosition = originalPos;
        }
        
        private IEnumerator SimpleReloadAnimation()
        {
            Debug.Log("🎬 Playing simple reload animation");
            
            // Простая анимация перезарядки
            Vector3 originalPos = transform.localPosition;
            Vector3 targetPos = originalPos + Vector3.down * 0.2f;
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            // Движение вниз
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localPosition = Vector3.Lerp(originalPos, targetPos, t);
                yield return null;
            }
            
            // Возвращаем обратно
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localPosition = Vector3.Lerp(targetPos, originalPos, t);
                yield return null;
            }
            
            transform.localPosition = originalPos;
        }
        
        /// <summary>
        /// Тестовые методы
        /// </summary>
        [ContextMenu("Test Shooting Animation")]
        public void TestShooting()
        {
            PlayShootingAnimation();
        }
        
        [ContextMenu("Test Reload Animation")]
        public void TestReload()
        {
            PlayReloadAnimation();
        }
    }
}
