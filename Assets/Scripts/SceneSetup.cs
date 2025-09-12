using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ShootingSystem
{
    /// <summary>
    /// Complete automatic scene setup for shooting range
    /// Attach this to an empty GameObject and run in play mode to auto-setup everything
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Setup Settings")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private bool createGround = true;
        [SerializeField] private bool createWalls = true;
        [SerializeField] private bool createLighting = true;
        [SerializeField] private bool createUI = true;
        
        [Header("Scene Configuration")]
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 1.6f, 0);
        [SerializeField] private Vector3 spawnerPosition = new Vector3(0, 2f, 20f);
        [SerializeField] private Vector3 spawnerRotation = new Vector3(0, 180f, 0);
        
        [Header("Prefab References (Auto-created if empty)")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject targetPrefab;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupScene();
            }
        }
        
        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("Starting automatic scene setup...");
            
            // Create environment
            if (createGround) CreateGround();
            if (createWalls) CreateWalls();
            if (createLighting) CreateLighting();
            
            // Create prefabs if not assigned
            CreatePrefabsIfNeeded();
            
            // Create game systems
            CreateStaticCamera();
            CreatePools();
            CreateSpawner();
            
            // Create UI
            if (createUI)
            {
                CreateUI();
                CreateCrosshair();
            }
            
            // Create game manager
            CreateGameManager();
            
            // Link all references
            LinkAllReferences();
            
            // Validate setup
            ValidateSetup();
            
            // Auto-start game if enabled
            if (autoStartGame)
            {
                StartCoroutine(AutoStartGame());
            }
            
            Debug.Log("✅ Scene setup completed successfully!");
        }
        
        private void CreateGround()
        {
            if (!createGround) return;
            
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);
            ground.layer = LayerMask.NameToLayer("Default");
            
            // Add a simple material
            Renderer renderer = ground.GetComponent<Renderer>();
            renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        
        private void CreateWalls()
        {
            if (!createWalls) return;
            
            // Create back wall
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "Back Wall";
            backWall.transform.position = new Vector3(0, 2.5f, -25f);
            backWall.transform.localScale = new Vector3(50f, 5f, 1f);
            backWall.layer = LayerMask.NameToLayer("Default");
            
            // Create side walls
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "Left Wall";
            leftWall.transform.position = new Vector3(-25f, 2.5f, 0f);
            leftWall.transform.localScale = new Vector3(1f, 5f, 50f);
            leftWall.layer = LayerMask.NameToLayer("Default");
            
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "Right Wall";
            rightWall.transform.position = new Vector3(25f, 2.5f, 0f);
            rightWall.transform.localScale = new Vector3(1f, 5f, 50f);
            rightWall.layer = LayerMask.NameToLayer("Default");
        }
        
        private void CreateStaticCamera()
        {
            // Check if camera already exists
            if (Camera.main != null)
            {
                Debug.Log("Main camera already exists, skipping camera creation");
                return;
            }
            
            // Create static camera GameObject
            GameObject cameraObj = new GameObject("Static Camera");
            cameraObj.transform.position = cameraPosition;
            
            // Add Camera component
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            
            // Add StaticCameraController
            StaticCameraController staticController = cameraObj.AddComponent<StaticCameraController>();
            
            Debug.Log("✅ Static camera created");
        }
        
        private void CreatePrefabsIfNeeded()
        {
            // Create bullet prefab if not assigned
            if (bulletPrefab == null)
            {
                bulletPrefab = CreateBulletPrefab();
                Debug.Log("✅ Bullet prefab created");
            }
            
            // Create target prefab if not assigned
            if (targetPrefab == null)
            {
                targetPrefab = CreateTargetPrefab();
                Debug.Log("✅ Target prefab created");
            }
        }
        
        private void CreatePools()
        {
            // Create Bullet Pool
            GameObject bulletPoolObj = new GameObject("Bullet Pool");
            BulletPool bulletPool = bulletPoolObj.AddComponent<BulletPool>();
            
            // Create Target Pool
            GameObject targetPoolObj = new GameObject("Target Pool");
            TargetPool targetPool = targetPoolObj.AddComponent<TargetPool>();
            
            Debug.Log("✅ Object pools created");
        }
        
        private GameObject CreateBulletPrefab()
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Bullet Prefab";
            bullet.transform.localScale = Vector3.one * 0.1f;
            
            // Add Rigidbody
            Rigidbody rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0f;
            
            // Add Bullet script
            bullet.AddComponent<Bullet>();
            
            // Add collider
            Collider collider = bullet.GetComponent<Collider>();
            collider.isTrigger = true;
            
            // Set material
            Renderer renderer = bullet.GetComponent<Renderer>();
            renderer.material.color = Color.yellow;
            
            return bullet;
        }
        
        private GameObject CreateTargetPrefab()
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Target Prefab";
            target.transform.localScale = new Vector3(1f, 2f, 0.2f);
            
            // Add Rigidbody
            Rigidbody rb = target.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            
            // Add Target script
            target.AddComponent<Target>();
            
            // Set material
            Renderer renderer = target.GetComponent<Renderer>();
            renderer.material.color = Color.red;
            
            return target;
        }
        
        private void CreateSpawner()
        {
            GameObject spawnerObj = new GameObject("Target Spawner");
            spawnerObj.transform.position = spawnerPosition;
            spawnerObj.transform.rotation = Quaternion.Euler(spawnerRotation);
            
            TargetSpawner spawner = spawnerObj.AddComponent<TargetSpawner>();
            
            // Set spawner settings for immediate spawning
            var spawnIntervalField = typeof(TargetSpawner).GetField("spawnInterval", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxTargetsPerSpawnField = typeof(TargetSpawner).GetField("maxTargetsPerSpawn", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (spawnIntervalField != null) spawnIntervalField.SetValue(spawner, 1f); // Spawn every second
            if (maxTargetsPerSpawnField != null) maxTargetsPerSpawnField.SetValue(spawner, 3); // Max 3 targets per spawn
            
            Debug.Log("✅ Target spawner created with optimized settings");
        }
        
        private void CreateUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("Game UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create UI elements
            CreateUIElements(canvasObj);
            
            Debug.Log("✅ UI system created");
        }
        
        private void CreateUIElements(GameObject canvas)
        {
            // Create time display
            GameObject timeObj = new GameObject("Time Text");
            timeObj.transform.SetParent(canvas.transform);
            TextMeshProUGUI timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "Time: 60.0s";
            timeText.fontSize = 24;
            timeText.color = Color.white;
            
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0, 1);
            timeRect.anchorMax = new Vector2(0, 1);
            timeRect.anchoredPosition = new Vector2(100, -30);
            timeRect.sizeDelta = new Vector2(200, 50);
            
            // Create targets count display
            GameObject targetsObj = new GameObject("Targets Text");
            targetsObj.transform.SetParent(canvas.transform);
            TextMeshProUGUI targetsText = targetsObj.AddComponent<TextMeshProUGUI>();
            targetsText.text = "Active Targets: 0";
            targetsText.fontSize = 24;
            targetsText.color = Color.white;
            
            RectTransform targetsRect = targetsObj.GetComponent<RectTransform>();
            targetsRect.anchorMin = new Vector2(0, 1);
            targetsRect.anchorMax = new Vector2(0, 1);
            targetsRect.anchoredPosition = new Vector2(100, -80);
            targetsRect.sizeDelta = new Vector2(200, 50);
            
            // Create game status display
            GameObject statusObj = new GameObject("Status Text");
            statusObj.transform.SetParent(canvas.transform);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Game Active";
            statusText.fontSize = 32;
            statusText.color = Color.green;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 1);
            statusRect.anchorMax = new Vector2(0.5f, 1);
            statusRect.anchoredPosition = new Vector2(0, -30);
            statusRect.sizeDelta = new Vector2(200, 50);
            
            // Create restart button
            GameObject buttonObj = new GameObject("Restart Button");
            buttonObj.transform.SetParent(canvas.transform);
            Button restartButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.anchoredPosition = new Vector2(-100, -30);
            buttonRect.sizeDelta = new Vector2(150, 50);
            
            // Add button text
            GameObject buttonTextObj = new GameObject("Button Text");
            buttonTextObj.transform.SetParent(buttonObj.transform);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Restart";
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
        }
        
        private void CreateCrosshair()
        {
            // Create crosshair UI
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(GameObject.Find("Game UI Canvas").transform);
            
            Image crosshairImage = crosshairObj.AddComponent<Image>();
            crosshairImage.color = Color.white;
            
            RectTransform rectTransform = crosshairObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(20f, 20f);
            
            crosshairObj.AddComponent<Crosshair>();
            
            Debug.Log("✅ Crosshair created");
        }
        
        private void CreateLighting()
        {
            // Create directional light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
            
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            Debug.Log("✅ Lighting created");
        }
        
        private void CreateGameManager()
        {
            // Check if GameManager already exists
            if (FindFirstObjectByType<GameManager>() != null)
            {
                Debug.Log("GameManager already exists, skipping creation");
                return;
            }
            
            GameObject gameManagerObj = new GameObject("Game Manager");
            GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
            
            Debug.Log("✅ Game Manager created");
        }
        
        private void LinkAllReferences()
        {
            Debug.Log("Linking all component references...");
            
            // Link BulletPool references
            BulletPool bulletPool = FindFirstObjectByType<BulletPool>();
            if (bulletPool != null && bulletPrefab != null)
            {
                // Use reflection to set the bulletPrefab field
                var bulletPrefabField = typeof(BulletPool).GetField("bulletPrefab", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (bulletPrefabField != null)
                {
                    bulletPrefabField.SetValue(bulletPool, bulletPrefab);
                }
            }
            
            // Link TargetPool references
            TargetPool targetPool = FindFirstObjectByType<TargetPool>();
            if (targetPool != null && targetPrefab != null)
            {
                var targetPrefabField = typeof(TargetPool).GetField("targetPrefab", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (targetPrefabField != null)
                {
                    targetPrefabField.SetValue(targetPool, targetPrefab);
                }
            }
            
            // Link GameManager references
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                var bulletPoolField = typeof(GameManager).GetField("bulletPool", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var targetPoolField = typeof(GameManager).GetField("targetPool", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var targetSpawnerField = typeof(GameManager).GetField("targetSpawner", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var cameraControllerField = typeof(GameManager).GetField("cameraController", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (bulletPoolField != null) bulletPoolField.SetValue(gameManager, bulletPool);
                if (targetPoolField != null) targetPoolField.SetValue(gameManager, targetPool);
                if (targetSpawnerField != null) targetSpawnerField.SetValue(gameManager, FindFirstObjectByType<TargetSpawner>());
                if (cameraControllerField != null) cameraControllerField.SetValue(gameManager, FindFirstObjectByType<StaticCameraController>());
            }
            
            // UI elements are created but not linked to any script (standalone UI)
            
            Debug.Log("✅ All references linked");
        }
        
        private void ValidateSetup()
        {
            Debug.Log("Validating scene setup...");
            
            bool isValid = true;
            
            // Check required components
            if (Camera.main == null)
            {
                Debug.LogError("❌ Main camera not found!");
                isValid = false;
            }
            
            if (FindFirstObjectByType<BulletPool>() == null)
            {
                Debug.LogError("❌ BulletPool not found!");
                isValid = false;
            }
            
            if (FindFirstObjectByType<TargetPool>() == null)
            {
                Debug.LogError("❌ TargetPool not found!");
                isValid = false;
            }
            
            if (FindFirstObjectByType<TargetSpawner>() == null)
            {
                Debug.LogError("❌ TargetSpawner not found!");
                isValid = false;
            }
            
            if (FindFirstObjectByType<GameManager>() == null)
            {
                Debug.LogError("❌ GameManager not found!");
                isValid = false;
            }
            
            if (FindFirstObjectByType<StaticCameraController>() == null)
            {
                Debug.LogError("❌ StaticCameraController not found!");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log("✅ Scene validation passed! All systems ready.");
            }
            else
            {
                Debug.LogError("❌ Scene validation failed! Check the errors above.");
            }
        }
        
        private IEnumerator AutoStartGame()
        {
            // Wait a frame to ensure all systems are initialized
            yield return null;
            
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.StartGame();
                Debug.Log("🎯 Game started automatically!");
            }
        }
    }
}
