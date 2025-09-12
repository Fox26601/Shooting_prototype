using UnityEngine;
using UnityEditor;

namespace ShootingSystem.Editor
{
    public class PrefabFixer : EditorWindow
    {
        [MenuItem("Tools/Shooting System/Fix Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<PrefabFixer>("Prefab Fixer");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Prefab Fixer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This tool will fix missing Mesh Filters and materials in prefabs.");
            GUILayout.Space(10);
            
            if (GUILayout.Button("🔧 Fix Bullet Prefab", GUILayout.Height(30)))
            {
                FixBulletPrefab();
            }
            
            if (GUILayout.Button("🎯 Fix Target Prefab", GUILayout.Height(30)))
            {
                FixTargetPrefab();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🚀 Fix All Prefabs", GUILayout.Height(30)))
            {
                FixBulletPrefab();
                FixTargetPrefab();
            }
        }
        
        private void FixBulletPrefab()
        {
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet Prefab.prefab");
            
            if (bulletPrefab == null)
            {
                Debug.LogError("❌ Bullet Prefab not found at Assets/Prefabs/Bullet Prefab.prefab");
                return;
            }
            
            // Open prefab for editing
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Bullet Prefab.prefab");
            
            try
            {
                // Check and add Mesh Filter
                MeshFilter meshFilter = prefabInstance.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = prefabInstance.AddComponent<MeshFilter>();
                    meshFilter.mesh = GetSphereMesh();
                    Debug.Log("✅ Added Mesh Filter to Bullet Prefab");
                }
                else if (meshFilter.mesh == null)
                {
                    meshFilter.mesh = GetSphereMesh();
                    Debug.Log("✅ Fixed Mesh Filter mesh in Bullet Prefab");
                }
                
                // Check and fix Mesh Renderer
                MeshRenderer meshRenderer = prefabInstance.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = prefabInstance.AddComponent<MeshRenderer>();
                }
                
                // Create or fix material
                Material bulletMaterial = CreateBulletMaterial();
                meshRenderer.material = bulletMaterial;
                
                // Set scale
                prefabInstance.transform.localScale = Vector3.one * 0.1f;
                
                // Check and add Bullet script
                Bullet bulletScript = prefabInstance.GetComponent<Bullet>();
                if (bulletScript == null)
                {
                    bulletScript = prefabInstance.AddComponent<Bullet>();
                    Debug.Log("✅ Added Bullet script to prefab");
                }
                
                // Configure bullet damage
                var damageField = typeof(Bullet).GetField("damage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (damageField != null)
                {
                    damageField.SetValue(bulletScript, 10);
                    Debug.Log("✅ Set bullet damage to 10");
                }
                
                // Check and add Rigidbody
                Rigidbody rb = prefabInstance.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = prefabInstance.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    rb.linearDamping = 0f;
                    rb.mass = 0.1f; // Light bullet
                    Debug.Log("✅ Added Rigidbody to Bullet Prefab");
                }
                else
                {
                    // Configure existing rigidbody
                    rb.useGravity = false;
                    rb.linearDamping = 0f;
                    rb.mass = 0.1f;
                    Debug.Log("✅ Configured Bullet Rigidbody for physics");
                }
                
                // Check and add Collider
                Collider collider = prefabInstance.GetComponent<Collider>();
                if (collider == null)
                {
                    SphereCollider sphereCollider = prefabInstance.AddComponent<SphereCollider>();
                    sphereCollider.isTrigger = false; // Physical collision, not trigger
                    sphereCollider.radius = 0.5f;
                    Debug.Log("✅ Added Sphere Collider to Bullet Prefab");
                }
                else
                {
                    // Ensure it's not a trigger for physical collision
                    collider.isTrigger = false;
                    Debug.Log("✅ Set Bullet collider to physical collision");
                }
                
                // Save prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, "Assets/Prefabs/Bullet Prefab.prefab");
                Debug.Log("✅ Bullet Prefab fixed and saved!");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }
        
        private void FixTargetPrefab()
        {
            GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Target Prefab.prefab");
            
            if (targetPrefab == null)
            {
                Debug.LogError("❌ Target Prefab not found at Assets/Prefabs/Target Prefab.prefab");
                return;
            }
            
            // Open prefab for editing
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Target Prefab.prefab");
            
            try
            {
                // Check and add Mesh Filter
                MeshFilter meshFilter = prefabInstance.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = prefabInstance.AddComponent<MeshFilter>();
                    meshFilter.mesh = GetCubeMesh();
                    Debug.Log("✅ Added Mesh Filter to Target Prefab");
                }
                else if (meshFilter.mesh == null)
                {
                    meshFilter.mesh = GetCubeMesh();
                    Debug.Log("✅ Fixed Mesh Filter mesh in Target Prefab");
                }
                
                // Check and fix Mesh Renderer
                MeshRenderer meshRenderer = prefabInstance.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = prefabInstance.AddComponent<MeshRenderer>();
                }
                
                // Create or fix material
                Material targetMaterial = CreateTargetMaterial();
                meshRenderer.material = targetMaterial;
                
                // Set scale
                prefabInstance.transform.localScale = new Vector3(1f, 2f, 0.2f);
                
                // Check and add Target script
                Target targetScript = prefabInstance.GetComponent<Target>();
                if (targetScript == null)
                {
                    prefabInstance.AddComponent<Target>();
                    Debug.Log("✅ Added Target script to prefab");
                }
                
                // Check and add Rigidbody
                Rigidbody rb = prefabInstance.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = prefabInstance.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.linearDamping = 0.5f;
                    rb.angularDamping = 0.5f;
                    rb.mass = 1f; // Standard mass for targets
                    Debug.Log("✅ Added Rigidbody to Target Prefab");
                }
                else
                {
                    // Configure existing rigidbody
                    rb.useGravity = true;
                    rb.linearDamping = 0.5f;
                    rb.angularDamping = 0.5f;
                    rb.mass = 1f;
                    Debug.Log("✅ Configured Target Rigidbody for physics");
                }
                
                // Check and add Collider
                Collider collider = prefabInstance.GetComponent<Collider>();
                if (collider == null)
                {
                    BoxCollider boxCollider = prefabInstance.AddComponent<BoxCollider>();
                    Debug.Log("✅ Added Box Collider to Target Prefab");
                }
                
                // Save prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, "Assets/Prefabs/Target Prefab.prefab");
                Debug.Log("✅ Target Prefab fixed and saved!");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }
        
        private Mesh GetSphereMesh()
        {
            // Create a simple sphere mesh
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempSphere);
            return sphereMesh;
        }
        
        private Mesh GetCubeMesh()
        {
            // Create a simple cube mesh
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempCube);
            return cubeMesh;
        }
        
        private Material CreateBulletMaterial()
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.yellow;
            material.name = "Bullet Material";
            return material;
        }
        
        private Material CreateTargetMaterial()
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = Color.red;
            material.name = "Target Material";
            return material;
        }
    }
}
