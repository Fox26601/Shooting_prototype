using UnityEngine;
using UnityEngine.InputSystem;

namespace ShootingSystem
{
    /// <summary>
    /// Контроллер камеры для pistol с Camera_01
    /// Заменяет StaticCameraController для работы с pistol prefab
    /// </summary>
    public class PistolCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivityX = 0.6f;
        [SerializeField] private float mouseSensitivityY = 0.4f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private float smoothing = 8f;
        [SerializeField] private bool invertY = false;
        
        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float aimFOV = 45f;
        [SerializeField] private float fovTransitionSpeed = 10f;
        
        [Header("Shooting")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float cooldownMultiplier = 1.0f; // 1.0 = equal to animation length
        private float shotCooldown = 0.5f;
        private float lastShotTime = -999f;
        
        [Header("Reload System")]
        [SerializeField] private int maxAmmo = 5;
        [SerializeField] private float reloadTime = 2f;
        
        [Header("Animation System")]
        [SerializeField] private PistolAnimationSystem animationSystem;
        [SerializeField] private PistolReloadAnimator reloadAnimator;
        [SerializeField] private PistolPrefabAnimator prefabAnimator;
        
        private Camera playerCamera;
        private Transform pistolInstance; // Ссылка на весь Pistol Instance
        private bool attackInput;
        private bool isAiming = false;
        
        // Reload system variables
        private int currentAmmo;
        private bool isReloading = false;
        private float reloadTimer = 0f;
        
        // UI reference
        private AmmoUI ammoUI;
        
        // Smoothing variables
        private float currentMouseX;
        private float currentMouseY;
        private float targetMouseX;
        private float targetMouseY;
        
        // Input System
        private InputAction lookAction;
        private InputAction attackAction;
        
        private void Awake()
        {
            // Get camera component - look in children if not found on this object
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
            {
                // Try to find camera in children
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    Debug.LogError("❌ Camera component not found on PistolCameraController or its children!");
                    return;
                }
                else
                {
                    Debug.Log("✅ Camera component found in children");
                }
            }
            
            // Setup fire point - DISABLED to preserve manual setup
            if (firePoint == null)
            {
                // Try to find existing FirePoint in children
                firePoint = transform.Find("FirePoint");
                if (firePoint == null)
                {
                    firePoint = transform.Find("Muzzle");
                }
                if (firePoint == null)
                {
                    firePoint = transform.Find("Barrel");
                }
                // GameObject firePointObj = new GameObject("FirePoint");
                // firePointObj.transform.SetParent(transform);
                // firePointObj.transform.localPosition = Vector3.forward * firePointDistance;
                // firePoint = firePointObj.transform;
                // Debug.Log("✅ FirePoint created for pistol camera");
            }
            
            // Create input actions
            lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
        }
        
        private void OnEnable()
        {
            if (lookAction != null)
                lookAction.Enable();
            if (attackAction != null)
            {
                attackAction.Enable();
                attackAction.performed += OnAttackInput;
            }
        }
        
        private void OnDisable()
        {
            if (lookAction != null)
                lookAction.Disable();
            if (attackAction != null)
            {
                attackAction.Disable();
                attackAction.performed -= OnAttackInput;
            }
        }
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Find Pistol Instance (parent of this camera)
            // Проверяем, является ли этот объект Pistol Instance
            if (transform.name == "Pistol Instance")
            {
                pistolInstance = transform;
            }
            else
            {
                // Ищем Pistol Instance в родительских объектах или в сцене
                pistolInstance = transform.parent;
                if (pistolInstance == null || pistolInstance.name != "Pistol Instance")
                {
                    pistolInstance = GameObject.Find("Pistol Instance")?.transform;
                }
                
                if (pistolInstance == null)
                {
                    Debug.LogError("❌ Pistol Instance not found! Please ensure Pistol Instance exists in scene.");
                    return;
                }
            }
            
            // Initialize ammo
            currentAmmo = maxAmmo;
            
            // Find AmmoUI
            ammoUI = FindFirstObjectByType<AmmoUI>();
            if (ammoUI == null)
            {
                Debug.LogWarning("⚠️ AmmoUI not found! Ammo display will not work.");
            }
            
            // Initialize animation systems
            InitializeAnimationSystems();

            // Derive cooldown from animation length if available
            UpdateShotCooldownFromAnimation();
            
            // Align camera to face targets - DISABLED to preserve manual positioning
            // AlignToTargetsDirection();
            
            // Update UI
            UpdateAmmoUI();
            
            Debug.Log("✅ PistolCameraController initialized");
        }
        
        private void InitializeAnimationSystems()
        {
            if (pistolInstance == null) return;
            
            // Find or create PistolPrefabAnimator - DISABLED to preserve manual setup
            if (prefabAnimator == null)
            {
                prefabAnimator = pistolInstance.GetComponent<PistolPrefabAnimator>();
                if (prefabAnimator == null)
                {
                    // prefabAnimator = pistolInstance.gameObject.AddComponent<PistolPrefabAnimator>();
                    // Debug.Log("✅ PistolPrefabAnimator added to Pistol Instance");
                    Debug.Log("⚠️ PistolPrefabAnimator not found - manual setup required");
                }
                else
                {
                    Debug.Log("✅ PistolPrefabAnimator found on Pistol Instance");
                }
            }
            
            // Find or create PistolAnimationSystem - DISABLED to preserve manual setup
            if (animationSystem == null)
            {
                animationSystem = pistolInstance.GetComponent<PistolAnimationSystem>();
                if (animationSystem == null)
                {
                    // animationSystem = pistolInstance.gameObject.AddComponent<PistolAnimationSystem>();
                    // Debug.Log("✅ PistolAnimationSystem added to Pistol Instance");
                    Debug.Log("⚠️ PistolAnimationSystem not found - manual setup required");
                }
                else
                {
                    Debug.Log("✅ PistolAnimationSystem found on Pistol Instance");
                }
            }
            
            // Find or create PistolReloadAnimator - DISABLED to preserve manual setup
            if (reloadAnimator == null)
            {
                reloadAnimator = pistolInstance.GetComponent<PistolReloadAnimator>();
                if (reloadAnimator == null)
                {
                    // reloadAnimator = pistolInstance.gameObject.AddComponent<PistolReloadAnimator>();
                    // Debug.Log("✅ PistolReloadAnimator added to Pistol Instance");
                    Debug.Log("⚠️ PistolReloadAnimator not found - manual setup required");
                }
                else
                {
                    Debug.Log("✅ PistolReloadAnimator found on Pistol Instance");
                }
            }
        }
        
        private void AlignToTargetsDirection()
        {
            if (pistolInstance == null) return;
            
            // Compute yaw/pitch to look towards world Z = -10 (targets line) from current position
            Vector3 pistolPos = pistolInstance.position;
            Vector3 targetPos = new Vector3(0f, pistolPos.y, -10f);
            Vector3 dir = (targetPos - pistolPos).normalized;

            // Horizontal yaw
            Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z).normalized;
            float yaw = dirXZ.sqrMagnitude > 0f ? Mathf.Atan2(dirXZ.x, dirXZ.z) * Mathf.Rad2Deg : 0f;

            // Vertical pitch
            float pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

            // Apply rotation to entire Pistol Instance
            pistolInstance.rotation = Quaternion.Euler(pitch, yaw, 0f);

            // Seed smoothing targets
            currentMouseX = targetMouseX = yaw;
            currentMouseY = targetMouseY = pitch;
        }
        
        private void Update()
        {
            HandleMouseLook();
            HandleFOV();
            HandleReload();
            
            // Fallback shooting input
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                attackInput = true;
            }
            
            HandleShooting();
        }
        
        private void HandleMouseLook()
        {
            if (lookAction == null) return;
            
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            
            float mouseX = lookInput.x * mouseSensitivityX;
            float mouseY = lookInput.y * mouseSensitivityY;
            
            if (invertY)
                mouseY = -mouseY;
            
            targetMouseX += mouseX;
            targetMouseY -= mouseY;
            targetMouseY = Mathf.Clamp(targetMouseY, -maxLookAngle, maxLookAngle);
            
            // Smooth rotation
            currentMouseX = Mathf.Lerp(currentMouseX, targetMouseX, smoothing * Time.deltaTime);
            currentMouseY = Mathf.Lerp(currentMouseY, targetMouseY, smoothing * Time.deltaTime);
            
            // Apply rotation to entire Pistol Instance (not just camera)
            if (pistolInstance != null)
            {
                pistolInstance.rotation = Quaternion.Euler(currentMouseY, currentMouseX, 0f);
            }
        }
        
        private void HandleFOV()
        {
            if (playerCamera == null) return;
            
            float targetFOV = isAiming ? aimFOV : normalFOV;
            float currentFOV = playerCamera.fieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, fovTransitionSpeed * Time.deltaTime);
            playerCamera.fieldOfView = newFOV;
        }
        
        private void HandleReload()
        {
            if (isReloading)
            {
                reloadTimer += Time.deltaTime;
                UpdateAmmoUI();
                
                if (reloadTimer >= reloadTime)
                {
                    currentAmmo = maxAmmo;
                    isReloading = false;
                    reloadTimer = 0f;
                    UpdateAmmoUI();
                    Debug.Log("🔄 Reload complete!");
                }
            }
        }
        
        private void HandleShooting()
        {
            if (attackInput && BulletPool.Instance != null && !isReloading && firePoint != null && Time.time >= lastShotTime + shotCooldown)
            {
                if (currentAmmo > 0)
                {
                    // Start shooting animation first; abort if cannot start
                    bool animationStarted = false;
                    float animDuration = 0f;
                    if (prefabAnimator != null)
                    {
                        animationStarted = prefabAnimator.TryPlayShootingOnce(out animDuration);
                    }
                    else
                    {
                        // Fallback direct animation
                        var anim = pistolInstance != null ? pistolInstance.GetComponent<Animation>() : null;
                        if (anim != null)
                        {
                            var clip = anim.GetClip("Pistol_FIRE");
                            if (clip != null)
                            {
                                clip.wrapMode = WrapMode.Once;
                                var state = anim["Pistol_FIRE"]; if (state != null) state.wrapMode = WrapMode.Once;
                                anim.Stop("Pistol_FIRE");
                                anim.Play("Pistol_FIRE");
                                animDuration = clip.length;
                                animationStarted = true;
                            }
                        }
                    }
                    if (!animationStarted)
                    {
                        Debug.LogWarning("⚠️ Shooting blocked: no shooting animation available");
                        attackInput = false;
                        return;
                    }
                    // Update cooldown from animation duration
                    if (animDuration > 0f)
                    {
                        shotCooldown = Mathf.Max(0.05f, animDuration * Mathf.Max(0.1f, cooldownMultiplier));
                    }
                    Vector3 shootDirection = transform.forward;
                    Vector3 shootPosition = firePoint.position;
                    
                    Bullet bullet = BulletPool.Instance.GetBullet();
                    if (bullet != null)
                    {
                        bullet.Fire(shootDirection, shootPosition);
                        
                        // Play shooting sound
                        AudioManager.Instance?.PlayShootingSound();
                        
                        // Play shooting animation
                        lastShotTime = Time.time;
                        
                        // Consume ammo
                        currentAmmo--;
                        UpdateAmmoUI();
                        
                        // Start reload if out of ammo
                        if (currentAmmo <= 0)
                        {
                            StartReload();
                        }
                    }
                }
                else
                {
                    Debug.Log("🔫 Out of ammo! Press R to reload.");
                }
                
                attackInput = false;
            }
        }
        
        private void StartReload()
        {
            if (!isReloading)
            {
                isReloading = true;
                reloadTimer = 0f;
                AudioManager.Instance?.PlayReloadSound();
                
                // Play reload animation
                PlayReloadAnimation();
                
                UpdateAmmoUI();
                Debug.Log("🔄 Starting reload...");
            }
        }
        
        private void UpdateAmmoUI()
        {
            if (ammoUI != null)
            {
                ammoUI.UpdateAmmoDisplay(currentAmmo, maxAmmo, isReloading, GetReloadProgress());
            }
        }
        
        
        private void OnAttackInput(InputAction.CallbackContext context)
        {
            attackInput = context.performed;
        }
        
        // Public methods for AmmoUI access
        public int GetCurrentAmmo() => currentAmmo;
        public int GetMaxAmmo() => maxAmmo;
        public bool IsReloading() => isReloading;
        public float GetReloadProgress() => isReloading ? (reloadTimer / reloadTime) : 0f;
        
        /// <summary>
        /// Play shooting animation using available animation systems
        /// </summary>
        private void PlayShootingAnimation()
        {
            Debug.Log("🎬 Attempting to play shooting animation...");
            
            // Приоритет: готовые анимации из префаба
            if (prefabAnimator != null)
            {
                Debug.Log("✅ Using PistolPrefabAnimator for shooting animation");
                prefabAnimator.PlayShootingAnimation();
            }
            else if (animationSystem != null)
            {
                Debug.Log("✅ Using PistolAnimationSystem for shooting animation");
                animationSystem.PlayShootingAnimation();
            }
            else if (reloadAnimator != null)
            {
                Debug.Log("✅ Using PistolReloadAnimator for shooting animation");
                reloadAnimator.PlaySlideBack();
            }
            else
            {
                Debug.LogWarning("⚠️ No animation system available!");
                // Direct fallback: try Animation component on Pistol Instance
                if (pistolInstance != null)
                {
                    var anim = pistolInstance.GetComponent<Animation>();
                    if (anim != null)
                    {
                        if (anim.GetClip("Pistol_FIRE") != null)
                        {
                            var clip = anim.GetClip("Pistol_FIRE");
                            if (clip != null) clip.wrapMode = WrapMode.Once;
                            var state = anim["Pistol_FIRE"];
                            if (state != null) state.wrapMode = WrapMode.Once;
                            anim.Play("Pistol_FIRE");
                            Debug.Log("✅ Played 'Pistol_FIRE' via Animation fallback");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Play reload animation using available animation systems
        /// </summary>
        private void PlayReloadAnimation()
        {
            Debug.Log("🔄 Attempting to play reload animation...");
            
            // Приоритет: готовые анимации из префаба
            if (prefabAnimator != null)
            {
                Debug.Log("✅ Using PistolPrefabAnimator for reload animation");
                prefabAnimator.PlayReloadAnimation();
            }
            else if (animationSystem != null)
            {
                Debug.Log("✅ Using PistolAnimationSystem for reload animation");
                animationSystem.PlayReloadAnimation(PistolAnimationSystem.ReloadType.FullReload);
            }
            else if (reloadAnimator != null)
            {
                Debug.Log("✅ Using PistolReloadAnimator for reload animation");
                reloadAnimator.PlayReloadAnimation();
            }
            else
            {
                Debug.LogWarning("⚠️ No animation system available for reload!");
                // Direct fallback: try Animation component on Pistol Instance
                if (pistolInstance != null)
                {
                    var anim = pistolInstance.GetComponent<Animation>();
                    if (anim != null)
                    {
                        if (anim.GetClip("Pistol_RELOAD") != null)
                        {
                            var clip = anim.GetClip("Pistol_RELOAD");
                            if (clip != null) clip.wrapMode = WrapMode.Once;
                            var state = anim["Pistol_RELOAD"];
                            if (state != null) state.wrapMode = WrapMode.Once;
                            anim.Play("Pistol_RELOAD");
                            Debug.Log("✅ Played 'Pistol_RELOAD' via Animation fallback");
                        }
                    }
                }
            }
        }

        private void UpdateShotCooldownFromAnimation()
        {
            float duration = 0f;
            if (pistolInstance != null)
            {
                var anim = pistolInstance.GetComponent<Animation>();
                if (anim != null)
                {
                    var clip = anim.GetClip("Pistol_FIRE");
                    if (clip != null)
                    {
                        duration = clip.length;
                    }
                }
            }
            if (duration <= 0f)
            {
                // sensible default if no clip found
                duration = 0.75f;
            }
            shotCooldown = Mathf.Max(0.05f, duration * Mathf.Max(0.1f, cooldownMultiplier));
            Debug.Log($"🎚️ Shot cooldown set to {shotCooldown:F2}s (clip {duration:F2}s, mult {cooldownMultiplier:F2})");
        }
    }
}
