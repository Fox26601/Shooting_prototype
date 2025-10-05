using UnityEngine;
using UnityEngine.InputSystem;

namespace ShootingSystem
{
    public class StaticCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float mouseSensitivityX = 0.6f; // Horizontal sensitivity (lower = less sensitive)
        [SerializeField] private float mouseSensitivityY = 0.4f; // Vertical sensitivity (even less sensitive)
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private float smoothing = 8f; // Increased for smoother movement
        [SerializeField] private bool invertY = false;
        
        
        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float aimFOV = 45f;
        [SerializeField] private float fovTransitionSpeed = 10f;
        
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Camera playerCamera;
        
        [Header("Reload System")]
        [SerializeField] private int maxAmmo = 5;
        [SerializeField] private float reloadTime = 2f;
        
        private float xRotation = 0f;
        private float yRotation = 0f;
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
        
        // Input System - using direct Input System instead of generated actions
        private InputAction lookAction;
        private InputAction attackAction;
        
        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = transform;
            }
            
            if (playerCamera == null)
            {
                playerCamera = GetComponent<Camera>();
                if (playerCamera == null)
                {
                    playerCamera = cameraTransform.GetComponent<Camera>();
                }
            }
            
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(cameraTransform);
                firePointObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                firePoint = firePointObj.transform;
            }
            
            // Create input actions manually
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
            
            // Initialize camera settings
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = normalFOV;
            }
            
            // Face the targets at scene start (assumes targets are on negative Z)
            AlignToTargetsDirection();

            // Initialize rotations
            Vector3 currentRotation = transform.eulerAngles;
            yRotation = currentRotation.y;
            xRotation = currentRotation.x;
            
            // Clamp initial X rotation
            if (xRotation > 180f)
                xRotation -= 360f;
            
            // Initialize reload system
            currentAmmo = maxAmmo;
            Debug.Log($"🔫 Ammo system initialized: {currentAmmo}/{maxAmmo} bullets");
            
            // Find AmmoUI
            ammoUI = FindFirstObjectByType<AmmoUI>();
            if (ammoUI != null)
            {
                Debug.Log("✅ AmmoUI found and connected");
            }
        }

        private void AlignToTargetsDirection()
        {
            // Compute yaw/pitch to look towards world Z = -10 (targets line) from current position
            Vector3 cameraPos = cameraTransform.position;
            Vector3 targetPos = new Vector3(0f, cameraPos.y, -10f);
            Vector3 dir = (targetPos - cameraPos).normalized;

            // Horizontal yaw
            Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z).normalized;
            float yaw = dirXZ.sqrMagnitude > 0f ? Mathf.Atan2(dirXZ.x, dirXZ.z) * Mathf.Rad2Deg : 0f;

            // Vertical pitch
            float pitch = Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

            if (cameraTransform != transform)
            {
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
            else
            {
                cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // Seed smoothing targets so Update starts from this facing
            currentMouseX = targetMouseX = yaw;
            currentMouseY = targetMouseY = pitch;
        }
        
        private void Update()
        {
            HandleMouseLook();
            HandleFOV();
            HandleReload();
            // Fallback shooting input (in case Input System binding fails)
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                attackInput = true;
            }
#else
            if (Input.GetMouseButtonDown(0)) attackInput = true;
#endif
            HandleShooting();
        }
        
        private void HandleMouseLook()
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            
            // Apply sensitivity and invert Y if needed
            float mouseX = lookInput.x * mouseSensitivityX;
            float mouseY = lookInput.y * mouseSensitivityY;
            
            if (invertY)
                mouseY = -mouseY;
            
            // Update target rotations
            targetMouseX += mouseX;
            targetMouseY -= mouseY;
            
            // Clamp vertical rotation
            targetMouseY = Mathf.Clamp(targetMouseY, -maxLookAngle, maxLookAngle);
            
            // Smooth the rotation
            currentMouseX = Mathf.Lerp(currentMouseX, targetMouseX, smoothing * Time.deltaTime);
            currentMouseY = Mathf.Lerp(currentMouseY, targetMouseY, smoothing * Time.deltaTime);
            
            // Apply rotations - horizontal to parent, vertical to camera
            if (cameraTransform != transform)
            {
                // Camera is child of another object
                transform.rotation = Quaternion.Euler(0f, currentMouseX, 0f);
                cameraTransform.localRotation = Quaternion.Euler(currentMouseY, 0f, 0f);
            }
            else
            {
                // Camera is the main object - apply both rotations to it
                cameraTransform.rotation = Quaternion.Euler(currentMouseY, currentMouseX, 0f);
            }
        }
        
        private void HandleFOV()
        {
            if (playerCamera == null) return;
            
            float targetFOV = isAiming ? aimFOV : normalFOV;
            float currentFOV = playerCamera.fieldOfView;
            
            // Smoothly transition FOV
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, fovTransitionSpeed * Time.deltaTime);
            playerCamera.fieldOfView = newFOV;
        }
        
        private void HandleReload()
        {
            if (isReloading)
            {
                reloadTimer += Time.deltaTime;
                
                if (reloadTimer >= reloadTime)
                {
                    // Reload complete
                    currentAmmo = maxAmmo;
                    isReloading = false;
                    reloadTimer = 0f;
                    Debug.Log($"🔫 Reload complete! Ammo: {currentAmmo}/{maxAmmo}");
                }
                
                // Update UI
                UpdateAmmoUI();
            }
        }
        
        private void UpdateAmmoUI()
        {
            if (ammoUI != null)
            {
                float reloadProgress = isReloading ? (reloadTimer / reloadTime) : 0f;
                ammoUI.UpdateAmmoDisplay(currentAmmo, maxAmmo, isReloading, reloadProgress);
            }
        }
        
        private void HandleShooting()
        {
            if (attackInput && BulletPool.Instance != null && !isReloading)
            {
                if (currentAmmo > 0)
                {
                    Vector3 shootDirection = cameraTransform.forward;
                    Vector3 shootPosition = firePoint.position;
                    
                    // Calculate where bullet will hit at target distance
                    float targetDistance = 10f; // Distance to targets (further reduced for better accuracy)
                    Vector3 targetHitPoint = shootPosition + shootDirection * targetDistance;
                    
                    Debug.Log($"🎯 SHOOTING! Direction: {shootDirection}, Position: {shootPosition}");
                    Debug.Log($"🎯 Expected hit point at distance {targetDistance}: {targetHitPoint}");
                    Debug.Log($"🎯 Camera forward: {cameraTransform.forward}, Camera position: {cameraTransform.position}");
                    
                    Bullet bullet = BulletPool.Instance.GetBullet();
                    if (bullet != null)
                    {
                        bullet.Fire(shootDirection, shootPosition);
                        
                        // Play shooting sound effect
                        AudioManager.Instance?.PlayShootingSound();
                        
                        // Consume ammo
                        currentAmmo--;
                        Debug.Log($"🔫 Shot fired! Ammo: {currentAmmo}/{maxAmmo}");
                        
                        // Update UI
                        UpdateAmmoUI();
                        
                        // Start reload if out of ammo
                        if (currentAmmo <= 0)
                        {
                            StartReload();
                        }
                        
                        Debug.Log("Bullet fired successfully!");
                    }
                    else
                    {
                        Debug.LogError("Failed to get bullet from pool!");
                    }
                }
                else
                {
                    Debug.Log("🔫 Out of ammo! Reloading...");
                    StartReload();
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
                Debug.Log($"🔫 Starting reload... {reloadTime}s");
                
                // Play reload sound
                AudioManager.Instance?.PlayReloadSound();
                
                // Update UI
                UpdateAmmoUI();
            }
        }
        
        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
        }
        
        public bool IsAiming()
        {
            return isAiming;
        }
        
        // Public methods for AmmoUI access
        public int GetCurrentAmmo()
        {
            return currentAmmo;
        }
        
        public int GetMaxAmmo()
        {
            return maxAmmo;
        }
        
        public bool IsReloading()
        {
            return isReloading;
        }
        
        public float GetReloadProgress()
        {
            return isReloading ? (reloadTimer / reloadTime) : 0f;
        }
        
        public void SetMouseSensitivity(float sensitivity)
        {
            float clamped = Mathf.Clamp(sensitivity, 0.1f, 5f);
            mouseSensitivityX = clamped;
            mouseSensitivityY = clamped * 0.7f;
        }
        
        public float GetMouseSensitivity()
        {
            return (mouseSensitivityX + mouseSensitivityY) * 0.5f;
        }

        public void SetMouseSensitivityX(float sensitivity)
        {
            mouseSensitivityX = Mathf.Clamp(sensitivity, 0.05f, 5f);
        }

        public void SetMouseSensitivityY(float sensitivity)
        {
            mouseSensitivityY = Mathf.Clamp(sensitivity, 0.05f, 5f);
        }
        
        // Input callbacks
        private void OnAttackInput(InputAction.CallbackContext context)
        {
            attackInput = true;
        }
        
        public Transform GetFirePoint()
        {
            return firePoint;
        }
        
        public Vector3 GetShootDirection()
        {
            return cameraTransform.forward;
        }
    }
}
