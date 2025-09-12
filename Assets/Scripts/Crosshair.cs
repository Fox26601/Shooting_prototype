using UnityEngine;
using UnityEngine.UI;

namespace ShootingSystem
{
    public class Crosshair : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color targetColor = Color.red;
        [SerializeField] private float size = 20f;
        [SerializeField] private float targetDetectionRange = 100f;
        [SerializeField] private LayerMask targetLayer = 1;
        
        private Camera playerCamera;
        private RectTransform crosshairRect;
        
        private void Start()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindFirstObjectByType<Camera>();
            }
            
            if (crosshairImage == null)
            {
                crosshairImage = GetComponent<Image>();
            }
            
            crosshairRect = crosshairImage.rectTransform;
            crosshairRect.sizeDelta = new Vector2(size, size);
        }
        
        private void Update()
        {
            UpdateCrosshair();
        }
        
        private void UpdateCrosshair()
        {
            if (playerCamera == null) return;
            
            // Check if we're aiming at a target
            bool isAimingAtTarget = CheckForTarget();
            
            // Update crosshair color
            crosshairImage.color = isAimingAtTarget ? targetColor : normalColor;
            
            // Update crosshair size (optional: make it smaller when aiming at target)
            float currentSize = isAimingAtTarget ? size * 0.8f : size;
            crosshairRect.sizeDelta = new Vector2(currentSize, currentSize);
        }
        
        private bool CheckForTarget()
        {
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);
            
            if (Physics.Raycast(ray, out RaycastHit hit, targetDetectionRange, targetLayer))
            {
                return hit.collider.GetComponent<ITarget>() != null;
            }
            
            return false;
        }
    }
}
