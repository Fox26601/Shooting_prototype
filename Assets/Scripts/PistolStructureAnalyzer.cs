using UnityEngine;

namespace ShootingSystem
{
    /// <summary>
    /// Analyzes the current pistol structure in the scene and provides detailed information
    /// </summary>
    public class PistolStructureAnalyzer : MonoBehaviour
    {
        [Header("Analysis Settings")]
        [SerializeField] private bool logTransformHierarchy = true;
        
        private void Start()
        {
            AnalyzeCurrentPistolStructure();
        }
        
        [ContextMenu("Analyze Current Pistol Structure")]
        public void AnalyzeCurrentPistolStructure()
        {
            Debug.Log("🔍 Analyzing current pistol structure in scene...");
            
            // Find pistol instance
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found in scene!");
                return;
            }
            
            Debug.Log($"✅ Found Pistol Instance: {pistolInstance.name}");
            
            // Analyze structure
            AnalyzePistolHierarchy(pistolInstance.transform);
            AnalyzePistolComponents(pistolInstance);
            AnalyzeSpecificParts(pistolInstance);
            
            Debug.Log("✅ Pistol structure analysis complete!");
        }
        
        private void AnalyzePistolHierarchy(Transform parent, int depth = 0)
        {
            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}📁 {parent.name}");
            
            if (logTransformHierarchy)
            {
                foreach (Transform child in parent)
                {
                    AnalyzePistolHierarchy(child, depth + 1);
                }
            }
        }
        
        private void AnalyzePistolComponents(GameObject pistolInstance)
        {
            Debug.Log("🔧 Analyzing pistol components...");
            
            // Check for Camera
            Camera camera = pistolInstance.GetComponentInChildren<Camera>();
            if (camera != null)
            {
                Debug.Log($"✅ Camera found: {camera.name} at {camera.transform.localPosition}");
            }
            else
            {
                Debug.LogWarning("⚠️ No Camera found in pistol structure");
            }
            
            // Check for AudioListener
            AudioListener audioListener = pistolInstance.GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                Debug.Log($"✅ AudioListener found: {audioListener.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ No AudioListener found in pistol structure");
            }
            
            // Check for specific pistol parts
            string[] partNames = { "bullet", "mag", "slide", "trigger", "hammer", "base", "Arms" };
            foreach (string partName in partNames)
            {
                Transform part = FindTransformByName(pistolInstance.transform, partName);
                if (part != null)
                {
                    Debug.Log($"✅ {partName} found: {part.name} at {part.localPosition}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {partName} not found in pistol structure");
                }
            }
        }
        
        private void AnalyzeSpecificParts(GameObject pistolInstance)
        {
            Debug.Log("🎯 Analyzing specific pistol parts...");
            
            // Find potential fire points
            Transform bullet = FindTransformByName(pistolInstance.transform, "bullet");
            if (bullet != null)
            {
                Debug.Log($"🎯 Potential FirePoint: {bullet.name} at {bullet.localPosition}");
                Debug.Log($"   World Position: {bullet.position}");
                Debug.Log($"   Forward Direction: {bullet.forward}");
            }
            
            // Find camera
            Transform camera = FindTransformByName(pistolInstance.transform, "Camera_01");
            if (camera != null)
            {
                Debug.Log($"📷 Camera found: {camera.name} at {camera.localPosition}");
                Debug.Log($"   World Position: {camera.position}");
                Debug.Log($"   Forward Direction: {camera.forward}");
            }
            
            // Find magazine
            Transform mag = FindTransformByName(pistolInstance.transform, "mag");
            if (mag != null)
            {
                Debug.Log($"📦 Magazine found: {mag.name} at {mag.localPosition}");
            }
            
            // Find slide
            Transform slide = FindTransformByName(pistolInstance.transform, "slide");
            if (slide != null)
            {
                Debug.Log($"🔫 Slide found: {slide.name} at {slide.localPosition}");
            }
            
            // Find trigger
            Transform trigger = FindTransformByName(pistolInstance.transform, "trigger");
            if (trigger != null)
            {
                Debug.Log($"🎯 Trigger found: {trigger.name} at {trigger.localPosition}");
            }
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
        /// Get detailed information about the pistol structure
        /// </summary>
        [ContextMenu("Get Pistol Structure Info")]
        public void GetPistolStructureInfo()
        {
            GameObject pistolInstance = GameObject.Find("Pistol Instance");
            if (pistolInstance == null)
            {
                Debug.LogError("❌ Pistol Instance not found!");
                return;
            }
            
            Debug.Log("📊 Pistol Structure Information:");
            Debug.Log($"   Total Transforms: {CountTransforms(pistolInstance.transform)}");
            Debug.Log($"   Total Components: {CountComponents(pistolInstance)}");
            Debug.Log($"   Has Camera: {pistolInstance.GetComponentInChildren<Camera>() != null}");
            Debug.Log($"   Has AudioListener: {pistolInstance.GetComponentInChildren<AudioListener>() != null}");
        }
        
        private int CountTransforms(Transform parent)
        {
            int count = 1; // Count self
            foreach (Transform child in parent)
            {
                count += CountTransforms(child);
            }
            return count;
        }
        
        private int CountComponents(GameObject obj)
        {
            int count = obj.GetComponents<Component>().Length;
            foreach (Transform child in obj.transform)
            {
                count += CountComponents(child.gameObject);
            }
            return count;
        }
    }
}
