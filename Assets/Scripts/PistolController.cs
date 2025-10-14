using UnityEngine;
using UnityEngine.InputSystem;

namespace ShootingSystem
{
    /// <summary>
    /// Controls the pistol prefab with integrated camera system.
    /// Manages camera movement, shooting mechanics, and pistol positioning.
    /// </summary>
    public class PistolController : MonoBehaviour
    {
        [Header("Pistol Settings")]
        [SerializeField] private Transform pistolTransform;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Vector3 pistolOffset = new Vector3(0.3f, -0.2f, 0.5f);
        
        [Header("Integration")]
        [SerializeField] private PistolIntegration pistolIntegration;
        [SerializeField] private PistolReloadAnimator reloadAnimator;
        [SerializeField] private PistolAnimationSystem animationSystem;
        
        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivityX = 0.6f;
        [SerializeField] private float mouseSensitivityY = 0.4f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private float smoothing = 8f;
        [SerializeField] private bool invertY = false;
        
        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float aimFOV = 45f;
        [SerializeField] private float fovTransitionSpeed = 10f;
        
        [Header("Reload System")]
        [SerializeField] private int maxAmmo = 5;
        [SerializeField] private float reloadTime = 2f;
        
        // Removed unused xRotation/yRotation to avoid warnings; we use smoothed targets instead
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
            // Get or create camera - DISABLED to preserve manual setup
            if (playerCamera == null)
            {
                playerCamera = GetComponent<Camera>();
                // if (playerCamera == null)
                // {
                //     playerCamera = gameObject.AddComponent<Camera>();
                // }
            }
            
            // Setup pistol transform - DISABLED to preserve manual setup
            if (pistolTransform == null)
            {
                pistolTransform = transform; // Use current transform instead of creating new
                // GameObject pistolObj = new GameObject("Pistol");
                // pistolObj.transform.SetParent(transform);
                // pistolTransform = pistolObj.transform;
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
                // firePointObj.transform.SetParent(pistolTransform);
                // firePointObj.transform.localPosition = Vector3.forward * 0.5f;
                // firePoint = firePointObj.transform;
            }
            
            // Create input actions
            lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
        }
        
        private void OnEnable()
        {
            lookAction.Enable();
            attackAction.Enable();
            attackAction.performed += OnAttackInput;
        }
        
        private void OnDisable()
        {
            lookAction.Disable();
            attackAction.Disable();
            attackAction.performed -= OnAttackInput;
        }
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Initialize ammo
            currentAmmo = maxAmmo;
            
            // Find AmmoUI
            ammoUI = FindFirstObjectByType<AmmoUI>();
            if (ammoUI == null)
            {
                Debug.LogWarning("⚠️ AmmoUI not found! Ammo display will not work.");
            }
            
            // Initialize integration
            InitializeIntegration();
            
            // Initialize reload animator
            InitializeReloadAnimator();
            
            // Initialize animation system
            InitializeAnimationSystem();
            
            // Position pistol relative to camera - DISABLED to preserve manual positioning
            // PositionPistol();
            
            // Align camera to face targets
            AlignToTargetsDirection();
            
            // Update UI
            UpdateAmmoUI();
        }
        
        private void InitializeIntegration()
        {
            // Find or create PistolIntegration - DISABLED to preserve manual setup
            if (pistolIntegration == null)
            {
                pistolIntegration = GetComponent<PistolIntegration>();
                if (pistolIntegration == null)
                {
                    // pistolIntegration = gameObject.AddComponent<PistolIntegration>();
                    Debug.Log("⚠️ PistolIntegration not found - manual setup required");
                }
            }
            
            // Auto-configure based on integration analysis
            if (pistolIntegration != null)
            {
                ConfigureFromIntegration();
            }
        }
        
        private void ConfigureFromIntegration()
        {
            if (pistolIntegration == null) return;
            
            var analysis = pistolIntegration.GetAnalysis();
            if (analysis == null) return;
            
            // Configure fire point from analysis
            if (analysis.hasFirePoint && firePoint == null)
            {
                Transform foundFirePoint = pistolIntegration.GetPistolTransform("FirePoint") ?? 
                                        pistolIntegration.GetPistolTransform("Muzzle") ?? 
                                        pistolIntegration.GetPistolTransform("Barrel");
                if (foundFirePoint != null)
                {
                    firePoint = foundFirePoint;
                    Debug.Log("✅ Fire point configured from pistol structure");
                }
            }
            
            // Configure camera from analysis
            if (analysis.hasCamera && playerCamera == null)
            {
                playerCamera = pistolIntegration.GetPistolComponent<Camera>();
                if (playerCamera != null)
                {
                    Debug.Log("✅ Camera configured from pistol structure");
                }
            }
            
            // Configure pistol transform
            if (pistolTransform == null)
            {
                pistolTransform = transform;
            }
        }
        
        private void PositionPistol()
        {
            if (pistolTransform != null)
            {
                pistolTransform.localPosition = pistolOffset;
                pistolTransform.localRotation = Quaternion.identity;
            }
        }
        
        private void AlignToTargetsDirection()
        {
            // Compute yaw/pitch to look towards world Z = -10 (targets line) from current position
            Vector3 cameraPos = transform.position;
            Vector3 targetPos = new Vector3(0f, cameraPos.y, -10f);
            Vector3 dir = (targetPos - cameraPos).normalized;

            // Horizontal yaw
            Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z).normalized;
            float yaw = dirXZ.sqrMagnitude > 0f ? Mathf.Atan2(dirXZ.x, dirXZ.z) * Mathf.Rad2Deg : 0f;

            // Vertical pitch
            float pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

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
            
            // Apply rotation to camera
            transform.rotation = Quaternion.Euler(currentMouseY, currentMouseX, 0f);
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
            if (attackInput && BulletPool.Instance != null && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    Vector3 shootDirection = transform.forward;
                    Vector3 shootPosition = firePoint.position;
                    
                    Bullet bullet = BulletPool.Instance.GetBullet();
                    if (bullet != null)
                    {
                        bullet.Fire(shootDirection, shootPosition);
                        
                        // Play shooting sound
                        AudioManager.Instance?.PlayShootingSound();
                        
                        // Play shooting animation
                        if (animationSystem != null)
                        {
                            animationSystem.PlayShootingAnimation();
                        }
                        else if (reloadAnimator != null)
                        {
                            reloadAnimator.PlaySlideBack();
                        }
                        
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
                if (animationSystem != null)
                {
                    animationSystem.PlayReloadAnimation(PistolAnimationSystem.ReloadType.FullReload);
                }
                else if (reloadAnimator != null)
                {
                    reloadAnimator.PlayReloadAnimation();
                }
                
                UpdateAmmoUI();
                Debug.Log("🔄 Starting reload...");
            }
        }
        
        private void InitializeReloadAnimator()
        {
            // Find or create PistolReloadAnimator
            if (reloadAnimator == null)
            {
                reloadAnimator = GetComponent<PistolReloadAnimator>();
                if (reloadAnimator == null)
                {
                    // reloadAnimator = gameObject.AddComponent<PistolReloadAnimator>();
                    // Debug.Log("✅ PistolReloadAnimator added to pistol controller");
                    Debug.Log("⚠️ PistolReloadAnimator not found - manual setup required");
                }
            }
        }
        
        private void InitializeAnimationSystem()
        {
            // Find or create PistolAnimationSystem
            if (animationSystem == null)
            {
                animationSystem = GetComponent<PistolAnimationSystem>();
                if (animationSystem == null)
                {
                    // animationSystem = gameObject.AddComponent<PistolAnimationSystem>();
                    // Debug.Log("✅ PistolAnimationSystem added to pistol controller");
                    Debug.Log("⚠️ PistolAnimationSystem not found - manual setup required");
                }
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
    }
}
