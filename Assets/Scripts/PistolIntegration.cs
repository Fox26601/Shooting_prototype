using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ShootingSystem
{
    /// <summary>
    /// Advanced pistol integration system that analyzes and integrates with pistol prefab structure.
    /// Uses reflection and component analysis to avoid hardcoded solutions.
    /// </summary>
    public class PistolIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool autoAnalyzeStructure = false; // Отключено - pistol уже настроен в сцене
        // Removed unused flag to avoid warnings (kept behavior default)
        [SerializeField] private bool createMissingComponents = true;
        
        [Header("Analysis Results")]
        [SerializeField] private PistolStructureAnalysis analysis;
        
        private GameObject pistolInstance;
        private Dictionary<string, Transform> pistolTransforms;
        private Dictionary<string, Component> pistolComponents;
        
        [System.Serializable]
        public class PistolStructureAnalysis
        {
            public bool hasCamera;
            public bool hasAudioListener;
            public bool hasFirePoint;
            public bool hasMuzzleFlash;
            public bool hasTrigger;
            public bool hasSlide;
            public bool hasMagazine;
            public List<string> availableTransforms;
            public List<string> availableComponents;
            public Vector3 cameraPosition;
            public Vector3 firePointPosition;
        }
        
        private void Start()
        {
            if (autoAnalyzeStructure)
            {
                AnalyzeAndIntegratePistol();
            }
        }
        
        /// <summary>
        /// Main integration method that analyzes pistol structure and sets up components
        /// </summary>
        public void AnalyzeAndIntegratePistol()
        {
            Debug.Log("🔍 Starting pistol structure analysis...");
            
            // Load pistol prefab
            if (!LoadPistolPrefab())
            {
                Debug.LogError("❌ Failed to load pistol prefab");
                return;
            }
            
            // Analyze structure
            AnalyzePistolStructure();
            
            // Integrate components
            IntegratePistolComponents();
            
            // Setup camera system
            SetupCameraSystem();
            
            // Setup shooting system
            SetupShootingSystem();
            
            Debug.Log("✅ Pistol integration complete!");
        }
        
        private bool LoadPistolPrefab()
        {
            GameObject pistolPrefab = Resources.Load<GameObject>("Prefabs/Pistol");
            if (pistolPrefab == null)
            {
                Debug.LogError("❌ Pistol prefab not found in Resources/Prefabs/");
                return false;
            }
            
            pistolInstance = Instantiate(pistolPrefab, Vector3.zero, Quaternion.identity);
            pistolInstance.name = "Pistol Instance";
            
            Debug.Log("✅ Pistol prefab loaded and instantiated");
            return true;
        }
        
        private void AnalyzePistolStructure()
        {
            if (pistolInstance == null) return;
            
            analysis = new PistolStructureAnalysis();
            analysis.availableTransforms = new List<string>();
            analysis.availableComponents = new List<string>();
            pistolTransforms = new Dictionary<string, Transform>();
            pistolComponents = new Dictionary<string, Component>();
            
            // Analyze all transforms
            AnalyzeTransforms(pistolInstance.transform);
            
            // Analyze all components
            AnalyzeComponents(pistolInstance);
            
            // Check for specific pistol parts
            CheckPistolParts();
            
            // Store analysis results
            StoreAnalysisResults();
            
            Debug.Log($"🔍 Analysis complete: {pistolTransforms.Count} transforms, {pistolComponents.Count} components found");
        }
        
        private void AnalyzeTransforms(Transform parent, string prefix = "")
        {
            string fullName = string.IsNullOrEmpty(prefix) ? parent.name : $"{prefix}/{parent.name}";
            pistolTransforms[fullName] = parent;
            analysis.availableTransforms.Add(fullName);
            
            foreach (Transform child in parent)
            {
                AnalyzeTransforms(child, fullName);
            }
        }
        
        private void AnalyzeComponents(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component component in components)
            {
                string componentName = component.GetType().Name;
                pistolComponents[componentName] = component;
                analysis.availableComponents.Add(componentName);
            }
        }
        
        private void CheckPistolParts()
        {
            // Check for camera
            analysis.hasCamera = pistolComponents.ContainsKey("Camera");
            
            // Check for AudioListener
            analysis.hasAudioListener = pistolComponents.ContainsKey("AudioListener");
            
            // Look for fire point (common names)
            string[] firePointNames = { "FirePoint", "Fire_Point", "Muzzle", "Barrel", "GunTip" };
            foreach (string name in firePointNames)
            {
                if (pistolTransforms.ContainsKey(name) || pistolTransforms.Any(kvp => kvp.Key.Contains(name)))
                {
                    analysis.hasFirePoint = true;
                    break;
                }
            }
            
            // Look for muzzle flash
            string[] muzzleFlashNames = { "MuzzleFlash", "Flash", "Muzzle_Flash" };
            foreach (string name in muzzleFlashNames)
            {
                if (pistolTransforms.ContainsKey(name) || pistolTransforms.Any(kvp => kvp.Key.Contains(name)))
                {
                    analysis.hasMuzzleFlash = true;
                    break;
                }
            }
            
            // Look for trigger
            string[] triggerNames = { "Trigger", "Gun_Trigger" };
            foreach (string name in triggerNames)
            {
                if (pistolTransforms.ContainsKey(name) || pistolTransforms.Any(kvp => kvp.Key.Contains(name)))
                {
                    analysis.hasTrigger = true;
                    break;
                }
            }
            
            // Look for slide
            string[] slideNames = { "Slide", "Gun_Slide", "Top" };
            foreach (string name in slideNames)
            {
                if (pistolTransforms.ContainsKey(name) || pistolTransforms.Any(kvp => kvp.Key.Contains(name)))
                {
                    analysis.hasSlide = true;
                    break;
                }
            }
            
            // Look for magazine
            string[] magazineNames = { "Magazine", "Mag", "Clip" };
            foreach (string name in magazineNames)
            {
                if (pistolTransforms.ContainsKey(name) || pistolTransforms.Any(kvp => kvp.Key.Contains(name)))
                {
                    analysis.hasMagazine = true;
                    break;
                }
            }
        }
        
        private void StoreAnalysisResults()
        {
            // Find camera position
            if (analysis.hasCamera)
            {
                Camera camera = pistolInstance.GetComponent<Camera>();
                if (camera != null)
                {
                    analysis.cameraPosition = camera.transform.localPosition;
                }
            }
            
            // Find fire point position
            if (analysis.hasFirePoint)
            {
                Transform firePoint = FindTransformByName("FirePoint") ?? FindTransformByName("Muzzle") ?? FindTransformByName("Barrel");
                if (firePoint != null)
                {
                    analysis.firePointPosition = firePoint.localPosition;
                }
            }
        }
        
        private Transform FindTransformByName(string name)
        {
            return pistolTransforms.Values.FirstOrDefault(t => t.name == name);
        }
        
        private void IntegratePistolComponents()
        {
            if (pistolInstance == null) return;
            
            // Add PistolController if not present
            PistolController controller = pistolInstance.GetComponent<PistolController>();
            if (controller == null && createMissingComponents)
            {
                controller = pistolInstance.AddComponent<PistolController>();
                Debug.Log("✅ PistolController added");
            }
            
            // Configure controller with analyzed data
            if (controller != null)
            {
                ConfigurePistolController(controller);
            }
        }
        
        private void ConfigurePistolController(PistolController controller)
        {
            if (controller == null || analysis == null) return;
            
            // Use reflection to configure controller based on analysis
            var controllerType = typeof(PistolController);
            
            // Set fire point if found
            if (analysis.hasFirePoint)
            {
                Transform firePoint = FindTransformByName("FirePoint") ?? FindTransformByName("Muzzle");
                if (firePoint != null)
                {
                    var firePointField = controllerType.GetField("firePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    firePointField?.SetValue(controller, firePoint);
                }
            }
            
            // Set pistol transform
            var pistolTransformField = controllerType.GetField("pistolTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pistolTransformField?.SetValue(controller, pistolInstance.transform);
            
            Debug.Log("✅ PistolController configured with analyzed data");
        }
        
        private void SetupCameraSystem()
        {
            if (analysis == null || pistolInstance == null) return;
            
            if (!analysis.hasCamera && createMissingComponents)
            {
                Camera camera = pistolInstance.AddComponent<Camera>();
                if (camera != null)
                {
                    camera.tag = "MainCamera";
                    analysis.hasCamera = true;
                    Debug.Log("✅ Camera component added");
                }
                else
                {
                    Debug.LogError("❌ Failed to add Camera component");
                }
            }
            
            if (analysis.hasCamera)
            {
                Camera camera = pistolInstance.GetComponent<Camera>();
                if (camera != null)
                {
                    camera.tag = "MainCamera";
                    
                    // Add AudioListener if missing
                    if (!analysis.hasAudioListener)
                    {
                        AudioListener audioListener = pistolInstance.AddComponent<AudioListener>();
                        if (audioListener != null)
                        {
                            analysis.hasAudioListener = true;
                            Debug.Log("✅ AudioListener added");
                        }
                        else
                        {
                            Debug.LogError("❌ Failed to add AudioListener component");
                        }
                    }
                }
            }
        }
        
        private void SetupShootingSystem()
        {
            if (analysis == null || pistolInstance == null) return;
            
            // Ensure fire point exists
            if (!analysis.hasFirePoint && createMissingComponents)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(pistolInstance.transform);
                firePointObj.transform.localPosition = Vector3.forward * 0.5f;
                analysis.hasFirePoint = true;
                Debug.Log("✅ FirePoint created");
            }
        }
        
        /// <summary>
        /// Get the analyzed pistol structure
        /// </summary>
        public PistolStructureAnalysis GetAnalysis()
        {
            return analysis;
        }
        
        /// <summary>
        /// Get pistol instance for external access
        /// </summary>
        public GameObject GetPistolInstance()
        {
            return pistolInstance;
        }
        
        /// <summary>
        /// Get specific transform by name
        /// </summary>
        public Transform GetPistolTransform(string name)
        {
            return pistolTransforms.ContainsKey(name) ? pistolTransforms[name] : null;
        }
        
        /// <summary>
        /// Get specific component by type
        /// </summary>
        public T GetPistolComponent<T>() where T : Component
        {
            return pistolInstance?.GetComponent<T>();
        }
        
        /// <summary>
        /// Manual trigger for analysis (useful for editor tools)
        /// </summary>
        [ContextMenu("Analyze Pistol Structure")]
        public void ManualAnalysis()
        {
            AnalyzeAndIntegratePistol();
        }
    }
}
