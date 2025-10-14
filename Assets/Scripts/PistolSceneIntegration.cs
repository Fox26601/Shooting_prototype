using UnityEngine;

namespace ShootingSystem
{
    /// <summary>
    /// Интеграция с уже существующим pistol в сцене
    /// Не создает новые экземпляры, работает с существующими
    /// </summary>
    public class PistolSceneIntegration : MonoBehaviour
    {
        [Header("Scene Integration")]
        [SerializeField] private bool autoConfigureOnStart = true;
        [SerializeField] private bool findExistingPistol = true;
        
        [Header("Pistol References")]
        [SerializeField] private GameObject pistolInstance;
        [SerializeField] private PistolController pistolController;
        [SerializeField] private Camera pistolCamera;
        [SerializeField] private AudioListener pistolAudioListener;
        
        private void Start()
        {
            if (autoConfigureOnStart)
            {
                ConfigureExistingPistol();
            }
        }
        
        [ContextMenu("Configure Existing Pistol")]
        public void ConfigureExistingPistol()
        {
            Debug.Log("🔧 Configuring existing pistol in scene...");
            
            // Find existing pistol instance
            if (findExistingPistol)
            {
                FindExistingPistol();
            }
            
            if (pistolInstance != null)
            {
                ConfigurePistolComponents();
                ConfigureCameraSystem();
                ConfigureAudioSystem();
                ConfigureAnimationSystem();
                
                Debug.Log("✅ Existing pistol configured successfully!");
            }
            else
            {
                Debug.LogWarning("⚠️ No pistol instance found in scene!");
            }
        }
        
        private void FindExistingPistol()
        {
            // Find pistol instance by name
            pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                // Try to find by tag or component
                PistolController[] controllers = FindObjectsByType<PistolController>(FindObjectsSortMode.None);
                if (controllers.Length > 0)
                {
                    pistolInstance = controllers[0].gameObject;
                    Debug.Log($"✅ Found pistol instance: {pistolInstance.name}");
                }
            }
            else
            {
                Debug.Log($"✅ Found pistol instance: {pistolInstance.name}");
            }
        }
        
        private void ConfigurePistolComponents()
        {
            if (pistolInstance == null) return;
            
            // Get or add PistolController
            pistolController = pistolInstance.GetComponent<PistolController>();
            if (pistolController == null)
            {
                pistolController = pistolInstance.AddComponent<PistolController>();
                Debug.Log("✅ PistolController added to existing pistol");
            }
            else
            {
                Debug.Log("✅ PistolController found on existing pistol");
            }
        }
        
        private void ConfigureCameraSystem()
        {
            if (pistolInstance == null) return;
            
            // Find camera in pistol hierarchy
            pistolCamera = pistolInstance.GetComponentInChildren<Camera>();
            if (pistolCamera != null)
            {
                // Set as main camera
                pistolCamera.tag = "MainCamera";
                Debug.Log($"✅ Camera configured: {pistolCamera.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ No camera found in pistol hierarchy");
            }
        }
        
        private void ConfigureAudioSystem()
        {
            if (pistolInstance == null) return;
            
            // Find or add AudioListener
            pistolAudioListener = pistolInstance.GetComponentInChildren<AudioListener>();
            if (pistolAudioListener == null)
            {
                if (pistolCamera != null)
                {
                    pistolAudioListener = pistolCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("✅ AudioListener added to pistol camera");
                }
                else
                {
                    Debug.LogWarning("⚠️ No camera found to attach AudioListener");
                }
            }
            else
            {
                Debug.Log($"✅ AudioListener found: {pistolAudioListener.name}");
            }
        }
        
        private void ConfigureAnimationSystem()
        {
            if (pistolInstance == null) return;
            
            // Add animation components if needed
            PistolReloadAnimator reloadAnimator = pistolInstance.GetComponent<PistolReloadAnimator>();
            if (reloadAnimator == null)
            {
                reloadAnimator = pistolInstance.AddComponent<PistolReloadAnimator>();
                Debug.Log("✅ PistolReloadAnimator added");
            }
            
            PistolAnimationSystem animationSystem = pistolInstance.GetComponent<PistolAnimationSystem>();
            if (animationSystem == null)
            {
                animationSystem = pistolInstance.AddComponent<PistolAnimationSystem>();
                Debug.Log("✅ PistolAnimationSystem added");
            }
        }
        
        /// <summary>
        /// Get the configured pistol instance
        /// </summary>
        public GameObject GetPistolInstance()
        {
            return pistolInstance;
        }
        
        /// <summary>
        /// Get the pistol controller
        /// </summary>
        public PistolController GetPistolController()
        {
            return pistolController;
        }
        
        /// <summary>
        /// Get the pistol camera
        /// </summary>
        public Camera GetPistolCamera()
        {
            return pistolCamera;
        }
        
        /// <summary>
        /// Validate pistol configuration
        /// </summary>
        [ContextMenu("Validate Pistol Configuration")]
        public void ValidatePistolConfiguration()
        {
            Debug.Log("🔍 Validating pistol configuration...");
            
            bool allValid = true;
            
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol instance not found!");
                allValid = false;
            }
            else
            {
                Debug.Log($"✅ Pistol instance: {pistolInstance.name}");
            }
            
            if (pistolController == null)
            {
                Debug.LogError("❌ PistolController not found!");
                allValid = false;
            }
            else
            {
                Debug.Log($"✅ PistolController: {pistolController.name}");
            }
            
            if (pistolCamera == null)
            {
                Debug.LogError("❌ Pistol camera not found!");
                allValid = false;
            }
            else
            {
                Debug.Log($"✅ Pistol camera: {pistolCamera.name} (Tag: {pistolCamera.tag})");
            }
            
            if (pistolAudioListener == null)
            {
                Debug.LogError("❌ Pistol AudioListener not found!");
                allValid = false;
            }
            else
            {
                Debug.Log($"✅ Pistol AudioListener: {pistolAudioListener.name}");
            }
            
            if (allValid)
            {
                Debug.Log("🎯 Pistol configuration validation passed!");
            }
            else
            {
                Debug.LogError("❌ Pistol configuration validation failed!");
            }
        }
    }
}

