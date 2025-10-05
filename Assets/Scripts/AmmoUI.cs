using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShootingSystem
{
    /// <summary>
    /// UI component for displaying ammo counter and reload status
    /// Shows current ammo, max ammo, and reload progress
    /// 
    /// NOTE: UI elements (ammoText, reloadText, etc.) are created by SceneSetup
    /// This component only manages the display logic, not the UI creation
    /// </summary>
    public class AmmoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI reloadText;
        [SerializeField] private Slider reloadProgressBar;
        [SerializeField] private Image ammoBackground;
        
        [Header("UI Settings")]
        [SerializeField] private Color normalAmmoColor = Color.white;
        [SerializeField] private Color lowAmmoColor = Color.yellow;
        [SerializeField] private Color outOfAmmoColor = Color.red;
        [SerializeField] private Color reloadingColor = Color.cyan;
        
        [Header("Animation Settings")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        
        [Header("Text Size Settings")]
        [SerializeField] private float ammoTextSize = 24f;
        [SerializeField] private float reloadTextSize = 18f;
        
        private StaticCameraController cameraController;
        
        private void Start()
        {
            // Find camera controller (only required in Play Mode)
            if (Application.isPlaying)
            {
                cameraController = FindFirstObjectByType<StaticCameraController>();
            }
            
            // Initialize UI
            UpdateAmmoDisplay();
            
            Debug.Log("✅ AmmoUI initialized");
        }

        
        [ContextMenu("Force Recreate UI Elements")]
        public void ForceRecreateUIElements()
        {
            Debug.Log("🔧 Force recreating UI elements via context menu...");
            DestroyExistingElements();
            CreateUIElements();
            UpdateAmmoDisplay();
            Debug.Log("✅ UI elements force recreated!");
        }
        
        private void DestroyExistingElements()
        {
            if (ammoText != null)
            {
                if (Application.isPlaying) Destroy(ammoText.gameObject); else DestroyImmediate(ammoText.gameObject);
                ammoText = null;
            }
            if (reloadText != null)
            {
                if (Application.isPlaying) Destroy(reloadText.gameObject); else DestroyImmediate(reloadText.gameObject);
                reloadText = null;
            }
            if (reloadProgressBar != null)
            {
                if (Application.isPlaying) Destroy(reloadProgressBar.gameObject); else DestroyImmediate(reloadProgressBar.gameObject);
                reloadProgressBar = null;
            }
            if (ammoBackground != null)
            {
                if (Application.isPlaying) Destroy(ammoBackground.gameObject); else DestroyImmediate(ammoBackground.gameObject);
                ammoBackground = null;
            }
            Debug.Log("🗑️ Destroyed existing UI elements");
        }
        
        private void CreateUIElements()
        {
            Debug.Log("🔧 Starting UI elements creation...");
            
            // Find the main UI Canvas
            Canvas mainCanvas = FindMainUICanvas();
            if (mainCanvas == null)
            {
                Debug.LogError("❌ No UI Canvas found! Cannot create UI elements.");
                return;
            }
            
            Debug.Log($"✅ Found Canvas: {mainCanvas.name} with render mode: {mainCanvas.renderMode}");
            
            // Move AmmoUI GameObject to be a child of the Canvas
            if (transform.parent != mainCanvas.transform)
            {
                Debug.Log("🔧 Moving AmmoUI to Canvas hierarchy...");
                transform.SetParent(mainCanvas.transform);
            }
            
            // Create ammo text if missing
            if (ammoText == null)
            {
                GameObject ammoTextObj = new GameObject("Ammo Text");
                ammoTextObj.transform.SetParent(mainCanvas.transform);
                
                ammoText = ammoTextObj.AddComponent<TextMeshProUGUI>();
                ammoText.text = "5/5";
                ammoText.fontSize = ammoTextSize;
                ammoText.color = normalAmmoColor;
                ammoText.alignment = TextAlignmentOptions.Center;
                
                // Position in top-right corner
                RectTransform ammoTextRect = ammoText.GetComponent<RectTransform>();
                ammoTextRect.anchorMin = new Vector2(1f, 1f);
                ammoTextRect.anchorMax = new Vector2(1f, 1f);
                ammoTextRect.anchoredPosition = new Vector2(-50f, -30f);
                ammoTextRect.sizeDelta = new Vector2(100f, 30f);
                
                Debug.Log("✅ Ammo Text created");
            }
            
            // Create reload text if missing
            if (reloadText == null)
            {
                GameObject reloadTextObj = new GameObject("Reload Text");
                reloadTextObj.transform.SetParent(mainCanvas.transform);
                
                reloadText = reloadTextObj.AddComponent<TextMeshProUGUI>();
                reloadText.text = "";
                reloadText.fontSize = reloadTextSize;
                reloadText.color = reloadingColor;
                reloadText.alignment = TextAlignmentOptions.Center;
                
                // Position below ammo text
                RectTransform reloadTextRect = reloadText.GetComponent<RectTransform>();
                reloadTextRect.anchorMin = new Vector2(1f, 1f);
                reloadTextRect.anchorMax = new Vector2(1f, 1f);
                reloadTextRect.anchoredPosition = new Vector2(-50f, -60f);
                reloadTextRect.sizeDelta = new Vector2(100f, 25f);
                
                Debug.Log("✅ Reload Text created");
            }
            
            // Create progress bar if missing
            if (reloadProgressBar == null)
            {
                GameObject progressBarObj = new GameObject("Reload Progress Bar");
                progressBarObj.transform.SetParent(mainCanvas.transform);
                
                reloadProgressBar = progressBarObj.AddComponent<Slider>();
                reloadProgressBar.minValue = 0f;
                reloadProgressBar.maxValue = 1f;
                reloadProgressBar.value = 0f;
                
                // Create Fill Area for the slider
                GameObject fillAreaObj = new GameObject("Fill Area");
                fillAreaObj.transform.SetParent(progressBarObj.transform);
                RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;
                fillAreaRect.offsetMin = new Vector2(10f, 0f);
                fillAreaRect.offsetMax = new Vector2(-10f, 0f);
                
                // Create Fill for the slider
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(fillAreaObj.transform);
                Image fillImage = fillObj.AddComponent<Image>();
                fillImage.color = Color.green;
                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                
                // Set the fill image to the slider
                reloadProgressBar.fillRect = fillRect;
                
                // Position below reload text
                RectTransform progressBarRect = reloadProgressBar.GetComponent<RectTransform>();
                progressBarRect.anchorMin = new Vector2(1f, 1f);
                progressBarRect.anchorMax = new Vector2(1f, 1f);
                progressBarRect.anchoredPosition = new Vector2(-50f, -90f);
                progressBarRect.sizeDelta = new Vector2(100f, 20f);
                
                Debug.Log("✅ Progress Bar created with Fill Area");
            }
            
            // Create background if missing
            if (ammoBackground == null)
            {
                GameObject backgroundObj = new GameObject("Ammo Background");
                backgroundObj.transform.SetParent(mainCanvas.transform);
                backgroundObj.transform.SetAsFirstSibling();
                
                ammoBackground = backgroundObj.AddComponent<Image>();
                ammoBackground.color = new Color(0f, 0f, 0f, 0.5f);
                
                // Position behind ammo elements
                RectTransform backgroundRect = ammoBackground.GetComponent<RectTransform>();
                backgroundRect.anchorMin = new Vector2(1f, 1f);
                backgroundRect.anchorMax = new Vector2(1f, 1f);
                backgroundRect.anchoredPosition = new Vector2(-50f, -50f);
                backgroundRect.sizeDelta = new Vector2(120f, 80f);
                
                Debug.Log("✅ Background created");
            }
        }
        
        private Canvas FindMainUICanvas()
        {
            // Look for "Game UI Canvas" first
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            Debug.Log($"🔍 Found {allCanvases.Length} Canvas(es) in scene:");
            foreach (Canvas canvas in allCanvases)
            {
                Debug.Log($"  - {canvas.name} (Render Mode: {canvas.renderMode})");
            }
            
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.name == "Game UI Canvas" || canvas.name.Contains("Game UI Canvas"))
                {
                    Debug.Log($"✅ Found exact Game UI Canvas: {canvas.name}");
                    return canvas;
                }
            }
            
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.name.Contains("UI Canvas"))
                {
                    Debug.Log($"✅ Found UI Canvas: {canvas.name}");
                    return canvas;
                }
            }
            
            // Look for Canvas with ScreenSpaceOverlay render mode
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Debug.Log($"✅ Found ScreenSpaceOverlay Canvas: {canvas.name}");
                    return canvas;
                }
            }
            
            // If not found, use the first canvas
            if (allCanvases.Length > 0)
            {
                Debug.Log($"✅ Using first available Canvas: {allCanvases[0].name}");
                return allCanvases[0];
            }
            
            return null;
        }
        
        
        private void Update()
        {
            if (cameraController == null) return;
            
            // Update ammo display
            UpdateAmmoDisplay();
            
            // Update reload status
            UpdateReloadStatus();
            
            // Handle animations
            HandleAnimations();
        }
        
        
        private void UpdateAmmoDisplay()
        {
            if (ammoText == null) return;
            
            // Get current ammo from camera controller
            int currentAmmo = GetCurrentAmmo();
            int maxAmmo = GetMaxAmmo();
            
            // Update text
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
            
            // Update font size
            ammoText.fontSize = ammoTextSize;
            
            // Update color based on ammo status
            if (currentAmmo == 0)
            {
                ammoText.color = outOfAmmoColor;
            }
            else if (currentAmmo <= 2)
            {
                ammoText.color = lowAmmoColor;
            }
            else
            {
                ammoText.color = normalAmmoColor;
            }
        }
        
        private void UpdateReloadStatus()
        {
            if (reloadText == null || reloadProgressBar == null) return;
            
            bool reloading = IsReloading();
            float progress = GetReloadProgress();
            
            if (reloading)
            {
                reloadText.text = "RELOADING...";
                reloadText.fontSize = reloadTextSize;
                reloadText.color = reloadingColor;
                reloadProgressBar.value = progress;
                reloadProgressBar.gameObject.SetActive(true);
            }
            else
            {
                reloadText.text = "";
                reloadProgressBar.gameObject.SetActive(false);
            }
        }
        
        private void HandleAnimations()
        {
            if (ammoText == null) return;
            
            // Pulse animation when out of ammo
            if (GetCurrentAmmo() == 0)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
                ammoText.transform.localScale = Vector3.one * pulse;
            }
            else
            {
                ammoText.transform.localScale = Vector3.one;
            }
        }
        
        private int GetCurrentAmmo()
        {
            if (cameraController != null)
            {
                return cameraController.GetCurrentAmmo();
            }
            
            return 5; // Default fallback
        }
        
        private int GetMaxAmmo()
        {
            if (cameraController != null)
            {
                return cameraController.GetMaxAmmo();
            }
            
            return 5; // Default fallback
        }
        
        private bool IsReloading()
        {
            if (cameraController != null)
            {
                return cameraController.IsReloading();
            }
            
            return false; // Default fallback
        }
        
        private float GetReloadProgress()
        {
            if (cameraController != null)
            {
                return cameraController.GetReloadProgress();
            }
            
            return 0f; // Default fallback
        }
        
        /// <summary>
        /// Update ammo display manually (called by StaticCameraController)
        /// </summary>
        /// <param name="currentAmmo">Current ammo count</param>
        /// <param name="maxAmmo">Maximum ammo count</param>
        /// <param name="isReloading">Is currently reloading</param>
        /// <param name="reloadProgress">Reload progress (0-1)</param>
        public void UpdateAmmoDisplay(int currentAmmo, int maxAmmo, bool isReloading, float reloadProgress)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
                ammoText.fontSize = ammoTextSize;
                
                // Update color based on ammo status
                if (currentAmmo == 0)
                {
                    ammoText.color = outOfAmmoColor;
                }
                else if (currentAmmo <= 2)
                {
                    ammoText.color = lowAmmoColor;
                }
                else
                {
                    ammoText.color = normalAmmoColor;
                }
            }
            
            if (reloadText != null)
            {
                if (isReloading)
                {
                    reloadText.text = "RELOADING...";
                    reloadText.fontSize = reloadTextSize;
                    reloadText.color = reloadingColor;
                }
                else
                {
                    reloadText.text = "";
                }
            }
            
            if (reloadProgressBar != null)
            {
                reloadProgressBar.value = reloadProgress;
                reloadProgressBar.gameObject.SetActive(isReloading);
            }
        }
    }
}
