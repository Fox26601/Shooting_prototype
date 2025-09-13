using UnityEngine;
using UnityEngine.InputSystem;

namespace ShootingSystem
{
    public class StaticCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float mouseSensitivity = 1.5f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private float smoothing = 5f;
        [SerializeField] private bool invertY = false;
        
        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float aimFOV = 45f;
        [SerializeField] private float fovTransitionSpeed = 10f;
        
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Camera playerCamera;
        
        private float xRotation = 0f;
        private float yRotation = 0f;
        private bool attackInput;
        private bool isAiming = false;
        
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
            
            // Initialize rotations
            Vector3 currentRotation = transform.eulerAngles;
            yRotation = currentRotation.y;
            xRotation = currentRotation.x;
            
            // Clamp initial X rotation
            if (xRotation > 180f)
                xRotation -= 360f;
        }
        
        private void Update()
        {
            HandleMouseLook();
            HandleFOV();
            HandleShooting();
        }
        
        private void HandleMouseLook()
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            
            // Apply sensitivity and invert Y if needed
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;
            
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
        
        private void HandleShooting()
        {
            if (attackInput && BulletPool.Instance != null)
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
                    Debug.Log("Bullet fired successfully!");
                }
                else
                {
                    Debug.LogError("Failed to get bullet from pool!");
                }
                
                attackInput = false;
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
        
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5f);
        }
        
        public float GetMouseSensitivity()
        {
            return mouseSensitivity;
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
