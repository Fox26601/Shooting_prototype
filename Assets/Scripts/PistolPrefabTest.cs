using UnityEngine;

namespace ShootingSystem
{
    /// <summary>
    /// Test script to verify pistol prefab loading and structure analysis
    /// </summary>
    public class PistolPrefabTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool showDetailedLogs = true;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                TestPistolPrefab();
            }
        }
        
        [ContextMenu("Test Pistol Prefab")]
        public void TestPistolPrefab()
        {
            Debug.Log("🔍 Starting Pistol Prefab Test...");
            
            // Test 1: Load pistol prefab
            TestPistolPrefabLoading();
            
            // Test 2: Analyze structure
            TestPistolStructureAnalysis();
            
            // Test 3: Integration test
            TestPistolIntegration();
            
            Debug.Log("✅ Pistol Prefab Test Complete!");
        }
        
        private void TestPistolPrefabLoading()
        {
            Debug.Log("📦 Testing pistol prefab loading...");
            
            GameObject pistolPrefab = Resources.Load<GameObject>("Prefabs/Pistol");
            if (pistolPrefab == null)
            {
                Debug.LogError("❌ Failed to load pistol prefab from Resources/Prefabs/Pistol");
                return;
            }
            
            Debug.Log("✅ Pistol prefab loaded successfully");
            
            // Instantiate for testing
            GameObject pistolInstance = Instantiate(pistolPrefab, Vector3.zero, Quaternion.identity);
            pistolInstance.name = "Test Pistol Instance";
            
            Debug.Log("✅ Pistol instance created");
            
            // Clean up test instance
            DestroyImmediate(pistolInstance);
        }
        
        private void TestPistolStructureAnalysis()
        {
            Debug.Log("🔍 Testing pistol structure analysis...");
            
            // Create PistolIntegration for testing
            GameObject testObj = new GameObject("Test Pistol Integration");
            PistolIntegration integration = testObj.AddComponent<PistolIntegration>();
            
            // Run analysis
            integration.ManualAnalysis();
            
            // Get analysis results
            var analysis = integration.GetAnalysis();
            if (analysis != null)
            {
                Debug.Log("📊 Analysis Results:");
                Debug.Log($"  Camera: {analysis.hasCamera}");
                Debug.Log($"  AudioListener: {analysis.hasAudioListener}");
                Debug.Log($"  FirePoint: {analysis.hasFirePoint}");
                Debug.Log($"  MuzzleFlash: {analysis.hasMuzzleFlash}");
                Debug.Log($"  Trigger: {analysis.hasTrigger}");
                Debug.Log($"  Slide: {analysis.hasSlide}");
                Debug.Log($"  Magazine: {analysis.hasMagazine}");
                Debug.Log($"  Available Transforms: {analysis.availableTransforms.Count}");
                Debug.Log($"  Available Components: {analysis.availableComponents.Count}");
                
                if (showDetailedLogs)
                {
                    Debug.Log("📋 Available Transforms:");
                    foreach (string transformName in analysis.availableTransforms)
                    {
                        Debug.Log($"  - {transformName}");
                    }
                    
                    Debug.Log("📋 Available Components:");
                    foreach (string componentName in analysis.availableComponents)
                    {
                        Debug.Log($"  - {componentName}");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ Analysis failed - no results returned");
            }
            
            // Clean up
            DestroyImmediate(testObj);
        }
        
        private void TestPistolIntegration()
        {
            Debug.Log("🔗 Testing pistol integration...");
            
            // Test PistolController integration
            GameObject testController = new GameObject("Test Pistol Controller");
            PistolController controller = testController.AddComponent<PistolController>();
            
            // Test if controller can find integration
            PistolIntegration integration = testController.GetComponent<PistolIntegration>();
            if (integration == null)
            {
                integration = testController.AddComponent<PistolIntegration>();
                Debug.Log("✅ PistolIntegration added to controller");
            }
            
            // Test integration workflow
            integration.ManualAnalysis();
            var analysis = integration.GetAnalysis();
            
            if (analysis != null)
            {
                Debug.Log("✅ Integration analysis successful");
                
                // Test component access
                Transform firePoint = integration.GetPistolTransform("FirePoint");
                if (firePoint != null)
                {
                    Debug.Log($"✅ FirePoint found: {firePoint.name}");
                }
                else
                {
                    Debug.Log("⚠️ FirePoint not found in pistol structure");
                }
                
                Camera camera = integration.GetPistolComponent<Camera>();
                if (camera != null)
                {
                    Debug.Log($"✅ Camera found: {camera.name}");
                }
                else
                {
                    Debug.Log("⚠️ Camera not found in pistol structure");
                }
            }
            else
            {
                Debug.LogError("❌ Integration analysis failed");
            }
            
            // Clean up
            DestroyImmediate(testController);
        }
        
        /// <summary>
        /// Test the complete pistol system setup
        /// </summary>
        [ContextMenu("Test Complete Pistol System")]
        public void TestCompletePistolSystem()
        {
            Debug.Log("🎯 Testing complete pistol system...");
            
            // Test GameManager integration
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("✅ GameManager found - pistol system should be integrated");
            }
            else
            {
                Debug.LogWarning("⚠️ GameManager not found - pistol system may not be properly integrated");
            }
            
            // Test editor tool availability
            Debug.Log("🛠️ Editor tool available at: Tools > Shooting System > Setup Pistol System");
            Debug.Log("   Use 'Analyze Pistol Structure' to see detailed analysis");
            Debug.Log("   Use 'Setup Complete Pistol System' for full setup");
        }
    }
}

