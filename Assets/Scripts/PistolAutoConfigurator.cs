using UnityEngine;

namespace ShootingSystem
{
    /// <summary>
    /// Automatically configures the pistol system based on the detected structure
    /// </summary>
    public class PistolAutoConfigurator : MonoBehaviour
    {
        [Header("Auto Configuration")]
        [SerializeField] private bool autoConfigureOnStart = true;
        [SerializeField] private bool createMissingComponents = true;
        
        private void Start()
        {
            if (autoConfigureOnStart)
            {
                AutoConfigurePistolSystem();
            }
        }
        
        [ContextMenu("Auto Configure Pistol System")]
        public void AutoConfigurePistolSystem()
        {
            Debug.Log("🔧 Starting automatic pistol system configuration...");
            
            // Find pistol instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found! Cannot configure system.");
                return;
            }
            
            // Configure camera system
            ConfigureCameraSystem(pistolInstance);
            
            // Configure shooting system
            ConfigureShootingSystem(pistolInstance);
            
            // Configure audio system
            ConfigureAudioSystem(pistolInstance);
            
            // Configure pistol controller
            ConfigurePistolController(pistolInstance);
            
            Debug.Log("✅ Automatic pistol system configuration complete!");
        }
        
        private void ConfigureCameraSystem(GameObject pistolInstance)
        {
            Debug.Log("📷 Configuring camera system...");
            
            // Find camera
            Camera camera = pistolInstance.GetComponentInChildren<Camera>();
            if (camera != null)
            {
                // Set as main camera
                camera.tag = "MainCamera";
                Debug.Log($"✅ Camera configured: {camera.name}");
                
                // Ensure AudioListener
                if (camera.GetComponent<AudioListener>() == null)
                {
                    camera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("✅ AudioListener added to camera");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No camera found in pistol structure");
            }
        }
        
        private void ConfigureShootingSystem(GameObject pistolInstance)
        {
            Debug.Log("🎯 Configuring shooting system...");
            
            // Find potential fire point (bullet transform)
            Transform bullet = FindTransformByName(pistolInstance.transform, "bullet");
            if (bullet != null)
            {
                Debug.Log($"✅ Fire point found: {bullet.name} at {bullet.localPosition}");
                
                // Create fire point marker if needed
                if (bullet.childCount == 0)
                {
                    GameObject firePointMarker = new GameObject("FirePointMarker");
                    firePointMarker.transform.SetParent(bullet);
                    firePointMarker.transform.localPosition = Vector3.forward * 0.1f;
                    Debug.Log("✅ Fire point marker created");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No bullet/fire point found in pistol structure");
            }
        }
        
        private void ConfigureAudioSystem(GameObject pistolInstance)
        {
            Debug.Log("🔊 Configuring audio system...");
            
            // Ensure AudioListener exists
            AudioListener audioListener = pistolInstance.GetComponentInChildren<AudioListener>();
            if (audioListener == null)
            {
                Camera camera = pistolInstance.GetComponentInChildren<Camera>();
                if (camera != null)
                {
                    camera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("✅ AudioListener added to camera");
                }
                else
                {
                    Debug.LogWarning("⚠️ No camera found to attach AudioListener");
                }
            }
            else
            {
                Debug.Log($"✅ AudioListener already exists: {audioListener.name}");
            }
        }
        
        private void ConfigurePistolController(GameObject pistolInstance)
        {
            Debug.Log("🎮 Configuring pistol controller...");
            
            // Add PistolController if not present
            PistolController controller = pistolInstance.GetComponent<PistolController>();
            if (controller == null && createMissingComponents)
            {
                controller = pistolInstance.AddComponent<PistolController>();
                Debug.Log("✅ PistolController added to pistol instance");
            }
            
            // Configure controller with detected structure
            if (controller != null)
            {
                ConfigureControllerWithStructure(controller, pistolInstance);
            }
        }
        
        private void ConfigureControllerWithStructure(PistolController controller, GameObject pistolInstance)
        {
            Debug.Log("🔧 Configuring controller with detected structure...");
            
            // Find camera for controller
            Camera camera = pistolInstance.GetComponentInChildren<Camera>();
            if (camera != null)
            {
                // Use reflection to set camera
                var cameraField = typeof(PistolController).GetField("playerCamera", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cameraField?.SetValue(controller, camera);
                Debug.Log("✅ Camera configured in PistolController");
            }
            
            // Find fire point for controller
            Transform bullet = FindTransformByName(pistolInstance.transform, "bullet");
            if (bullet != null)
            {
                // Use reflection to set fire point
                var firePointField = typeof(PistolController).GetField("firePoint", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                firePointField?.SetValue(controller, bullet);
                Debug.Log("✅ Fire point configured in PistolController");
            }
            
            // Set pistol transform
            var pistolTransformField = typeof(PistolController).GetField("pistolTransform", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pistolTransformField?.SetValue(controller, pistolInstance.transform);
            Debug.Log("✅ Pistol transform configured in PistolController");
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
        /// Validate the configured pistol system
        /// </summary>
        [ContextMenu("Validate Pistol Configuration")]
        public void ValidatePistolConfiguration()
        {
            Debug.Log("🔍 Validating pistol configuration...");
            
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found!");
                return;
            }
            
            // Check camera
            Camera camera = pistolInstance.GetComponentInChildren<Camera>();
            if (camera != null)
            {
                Debug.Log($"✅ Camera: {camera.name} (Tag: {camera.tag})");
            }
            else
            {
                Debug.LogError("❌ No camera found!");
            }
            
            // Check AudioListener
            AudioListener audioListener = pistolInstance.GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                Debug.Log($"✅ AudioListener: {audioListener.name}");
            }
            else
            {
                Debug.LogError("❌ No AudioListener found!");
            }
            
            // Check PistolController
            PistolController controller = pistolInstance.GetComponent<PistolController>();
            if (controller != null)
            {
                Debug.Log($"✅ PistolController: {controller.name}");
            }
            else
            {
                Debug.LogError("❌ No PistolController found!");
            }
            
            // Check fire point
            Transform bullet = FindTransformByName(pistolInstance.transform, "bullet");
            if (bullet != null)
            {
                Debug.Log($"✅ Fire Point: {bullet.name} at {bullet.localPosition}");
            }
            else
            {
                Debug.LogWarning("⚠️ No fire point found!");
            }
            
            Debug.Log("✅ Pistol configuration validation complete!");
        }
    }
}

