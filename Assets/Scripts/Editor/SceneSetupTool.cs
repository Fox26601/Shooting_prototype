using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ShootingSystem;

namespace ShootingSystem.Editor
{
    public class SceneSetupTool : EditorWindow
    {
        [Header("Setup Settings")]
        private bool createGround = true;
        private bool createWalls = true;
        private bool createLighting = true;
        private bool createUI = true;
        private bool autoStartGame = true;
        
        [Header("Scene Configuration")]
        private Vector3 cameraPosition = new Vector3(0, 1.6f, 0);
        private Vector3 spawnerPosition = new Vector3(0, 1f, 0f);
        private Vector3 spawnerRotation = new Vector3(0, 180f, 0);
        
        [Header("Game Settings")]
        private int maxTargets = 5;
        
        [MenuItem("Tools/Shooting System/Setup Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupTool>("Shooting Scene Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Shooting Range Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Setup Settings
            GUILayout.Label("Setup Settings", EditorStyles.boldLabel);
            createGround = EditorGUILayout.Toggle("Create Ground", createGround);
            createWalls = EditorGUILayout.Toggle("Create Walls", createWalls);
            createLighting = EditorGUILayout.Toggle("Create Lighting", createLighting);
            createUI = EditorGUILayout.Toggle("Create UI", createUI);
            autoStartGame = EditorGUILayout.Toggle("Auto Start Game", autoStartGame);
            
            GUILayout.Space(10);
            
            // Scene Configuration
            GUILayout.Label("Scene Configuration", EditorStyles.boldLabel);
            cameraPosition = EditorGUILayout.Vector3Field("Camera Position", cameraPosition);
            spawnerPosition = EditorGUILayout.Vector3Field("Spawner Position", spawnerPosition);
            spawnerRotation = EditorGUILayout.Vector3Field("Spawner Rotation", spawnerRotation);
            
            GUILayout.Space(10);
            
            // Game Settings
            GUILayout.Label("Game Settings", EditorStyles.boldLabel);
            maxTargets = EditorGUILayout.IntField("Max Targets", maxTargets);
            
            GUILayout.Space(20);
            
            // Buttons
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🚀 Setup Scene", GUILayout.Height(30)))
            {
                SetupScene();
            }
            
            if (GUILayout.Button("🧹 Clear Scene", GUILayout.Height(30)))
            {
                ClearScene();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Quick Setup Button
            if (GUILayout.Button("⚡ Quick Setup (Default Settings)", GUILayout.Height(25)))
            {
                QuickSetup();
            }
            
            GUILayout.Space(10);
            
            // Help
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Configure settings above");
            GUILayout.Label("2. Click 'Setup Scene' to create everything");
            GUILayout.Label("3. Press Play to start shooting!");
            GUILayout.Label("4. Use 'Clear Scene' to remove all objects");
        }
        
        private void SetupScene()
        {
            Debug.Log("Starting smart scene setup from Tools menu...");
            
            // Check and create environment
            if (createGround) CheckAndCreateGround();
            if (createWalls) CheckAndCreateWalls();
            if (createLighting) CheckAndCreateLighting();
            
            // Check and create game systems
            CheckAndCreateStaticCamera();
            CheckAndCreatePools();
            CheckAndCreateSpawner();
            
            // Check and create UI
            if (createUI)
            {
                CheckAndCreateUI();
                CheckAndCreateCrosshair();
            }
            
            // Check and create game manager
            CheckAndCreateGameManager();
            
            // Link all references
            LinkAllReferences();
            
            // Auto-start game if enabled
            if (autoStartGame)
            {
                EditorApplication.delayCall += () => {
                    var gameManager = FindFirstObjectByType<GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.StartGame();
                        Debug.Log("🎯 Game started automatically!");
                    }
                };
            }
            
            Debug.Log("✅ Smart scene setup completed successfully!");
            EditorUtility.DisplayDialog("Setup Complete", "Shooting range scene has been set up successfully!", "OK");
        }
        
        private void QuickSetup()
        {
            // Reset to default values
            createGround = true;
            createWalls = true;
            createLighting = true;
            createUI = true;
            autoStartGame = true;
            cameraPosition = new Vector3(0, 1.6f, 0);
            spawnerPosition = new Vector3(0, 1f, 0f);
            spawnerRotation = new Vector3(0, 180f, 0);
            maxTargets = 5;
            
            SetupScene();
        }
        
        private void ClearScene()
        {
            if (EditorUtility.DisplayDialog("Clear Scene", 
                "This will remove all shooting system objects. Continue?", "Yes", "Cancel"))
            {
                // Find all shooting system objects
                string[] objectNames = {
                    "Static Camera", "Bullet Pool", "Target Pool", "Target Spawner",
                    "Game Manager", "Game UI Canvas", "Directional Light",
                    "Ground", "Back Wall", "Left Wall", "Right Wall",
                    "Bullet Prefab", "Target Prefab"
                };
                
                foreach (string objName in objectNames)
                {
                    GameObject obj = GameObject.Find(objName);
                    if (obj != null)
                    {
                        DestroyImmediate(obj);
                    }
                }
                
                Debug.Log("🧹 Scene cleared!");
                EditorUtility.DisplayDialog("Scene Cleared", "All shooting system objects have been removed.", "OK");
            }
        }
        
        // Smart creation methods that check for existing objects
        private void CheckAndCreateGround()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(10f, 1f, 10f);
                ground.layer = LayerMask.NameToLayer("Default");
                
                Renderer renderer = ground.GetComponent<Renderer>();
                renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                Debug.Log("✅ Ground created");
            }
            else
            {
                Debug.Log("ℹ️ Ground already exists, skipping creation");
            }
        }
        
        private void CheckAndCreateWalls()
        {
            string[] wallNames = { "Back Wall", "Left Wall", "Right Wall" };
            Vector3[] positions = { new Vector3(0, 2.5f, -25f), new Vector3(-25f, 2.5f, 0f), new Vector3(25f, 2.5f, 0f) };
            Vector3[] scales = { new Vector3(50f, 5f, 1f), new Vector3(1f, 5f, 50f), new Vector3(1f, 5f, 50f) };
            
            for (int i = 0; i < wallNames.Length; i++)
            {
                GameObject wall = GameObject.Find(wallNames[i]);
                if (wall == null)
                {
                    wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.name = wallNames[i];
                    wall.transform.position = positions[i];
                    wall.transform.localScale = scales[i];
                    wall.layer = LayerMask.NameToLayer("Default");
                    Debug.Log($"✅ {wallNames[i]} created");
                }
                else
                {
                    Debug.Log($"ℹ️ {wallNames[i]} already exists, skipping creation");
                }
            }
        }
        
        private void CheckAndCreateLighting()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                light.intensity = 1f;
                light.shadows = LightShadows.Soft;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                Debug.Log("✅ Directional Light created");
            }
            else
            {
                Debug.Log("ℹ️ Directional Light already exists, skipping creation");
            }
        }
        
        private void CheckAndCreateStaticCamera()
        {
            GameObject cameraObj = GameObject.Find("Static Camera");
            StaticCameraController controller = null;
            
            if (cameraObj == null)
            {
                cameraObj = new GameObject("Static Camera");
                cameraObj.transform.position = cameraPosition;
                
                Camera camera = cameraObj.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.fieldOfView = 60f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 1000f;
                
                // Add Audio Listener for 3D audio
                cameraObj.AddComponent<AudioListener>();
                
                controller = cameraObj.AddComponent<StaticCameraController>();
                Debug.Log("✅ Static Camera created");
            }
            else
            {
                // Check if StaticCameraController is missing
                controller = cameraObj.GetComponent<StaticCameraController>();
                if (controller == null)
                {
                    controller = cameraObj.AddComponent<StaticCameraController>();
                    Debug.Log("✅ StaticCameraController added to existing camera");
                }
                else
                {
                    Debug.Log("ℹ️ Static Camera already exists with controller, skipping creation");
                }
            }
            
            // Configure camera settings for professional FPS feel
            if (controller != null)
            {
                var mouseSensitivityField = typeof(StaticCameraController).GetField("mouseSensitivity", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var smoothingField = typeof(StaticCameraController).GetField("smoothing", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxLookAngleField = typeof(StaticCameraController).GetField("maxLookAngle", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var normalFOVField = typeof(StaticCameraController).GetField("normalFOV", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var aimFOVField = typeof(StaticCameraController).GetField("aimFOV", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (mouseSensitivityField != null) mouseSensitivityField.SetValue(controller, 1.5f);
                if (smoothingField != null) smoothingField.SetValue(controller, 8f);
                if (maxLookAngleField != null) maxLookAngleField.SetValue(controller, 80f);
                if (normalFOVField != null) normalFOVField.SetValue(controller, 60f);
                if (aimFOVField != null) aimFOVField.SetValue(controller, 45f);
            }
        }
        
        
        
        private void CheckAndCreateUI()
        {
            GameObject canvasObj = GameObject.Find("Game UI Canvas");
            if (canvasObj == null)
            {
                // Create Canvas
                canvasObj = new GameObject("Game UI Canvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Create Game UI component
                GameObject gameUIObj = new GameObject("Game UI");
                gameUIObj.transform.SetParent(canvasObj.transform);
                gameUIObj.AddComponent<GameUI>();
                
                Debug.Log("✅ Game UI Canvas created");
            }
            else
            {
                // Check if GameUI component exists
                GameUI gameUI = canvasObj.GetComponentInChildren<GameUI>();
                if (gameUI == null)
                {
                    GameObject gameUIObj = new GameObject("Game UI");
                    gameUIObj.transform.SetParent(canvasObj.transform);
                    gameUIObj.AddComponent<GameUI>();
                    Debug.Log("✅ GameUI component added to existing canvas");
                }
                else
                {
                    Debug.Log("ℹ️ Game UI Canvas already exists with GameUI component, skipping creation");
                }
            }
            
            // Create UI elements
            CreateUIElements(canvasObj);
        }
        
        private void CreateUIElements(GameObject canvas)
        {
            // Create time display
            CreateUIElementIfNotExists(canvas, "Time Text", () => {
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
                return timeObj;
            });
            
            // Create targets count display
            CreateUIElementIfNotExists(canvas, "Targets Text", () => {
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
                return targetsObj;
            });
            
            // Create game status display
            CreateUIElementIfNotExists(canvas, "Status Text", () => {
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
                return statusObj;
            });
            
            // Create restart button
            CreateUIElementIfNotExists(canvas, "Restart Button", () => {
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
                
                return buttonObj;
            });
        }
        
        private void CreateUIElementIfNotExists(GameObject parent, string elementName, System.Func<GameObject> createFunction)
        {
            Transform existingElement = parent.transform.Find(elementName);
            if (existingElement == null)
            {
                GameObject newElement = createFunction();
                Debug.Log($"✅ {elementName} created");
            }
            else
            {
                Debug.Log($"ℹ️ {elementName} already exists, skipping creation");
            }
        }
        
        private void CheckAndCreateCrosshair()
        {
            GameObject canvas = GameObject.Find("Game UI Canvas");
            if (canvas == null) return;
            
            Transform existingCrosshair = canvas.transform.Find("Crosshair");
            if (existingCrosshair == null)
            {
                GameObject crosshairObj = new GameObject("Crosshair");
                crosshairObj.transform.SetParent(canvas.transform);
                
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
            else
            {
                // Check if Crosshair component is missing
                Crosshair crosshairComponent = existingCrosshair.GetComponent<Crosshair>();
                if (crosshairComponent == null)
                {
                    existingCrosshair.gameObject.AddComponent<Crosshair>();
                    Debug.Log("✅ Crosshair component added to existing object");
                }
                else
                {
                    Debug.Log("ℹ️ Crosshair already exists with component, skipping creation");
                }
            }
        }
        
        private void CheckAndCreateGameManager()
        {
            GameObject gameManagerObj = GameObject.Find("Game Manager");
            if (gameManagerObj == null)
            {
                gameManagerObj = new GameObject("Game Manager");
                GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
                Debug.Log("✅ Game Manager created");
            }
            else
            {
                // Check if GameManager component is missing
                GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
                if (gameManager == null)
                {
                    gameManagerObj.AddComponent<GameManager>();
                    Debug.Log("✅ GameManager component added to existing object");
                }
                else
                {
                    Debug.Log("ℹ️ Game Manager already exists with component, skipping creation");
                }
            }
        }
        
        private void CheckAndCreatePools()
        {
            // Check and create Bullet Pool
            GameObject bulletPoolObj = GameObject.Find("Bullet Pool");
            BulletPool bulletPool = null;
            
            if (bulletPoolObj == null)
            {
                bulletPoolObj = new GameObject("Bullet Pool");
                bulletPool = bulletPoolObj.AddComponent<BulletPool>();
                Debug.Log("✅ Bullet Pool created");
            }
            else
            {
                bulletPool = bulletPoolObj.GetComponent<BulletPool>();
                if (bulletPool == null)
                {
                    bulletPool = bulletPoolObj.AddComponent<BulletPool>();
                    Debug.Log("✅ BulletPool component added to existing object");
                }
                else
                {
                    Debug.Log("ℹ️ Bullet Pool already exists with component, skipping creation");
                }
            }
            
            // Check and create Target Pool
            GameObject targetPoolObj = GameObject.Find("Target Pool");
            TargetPool targetPool = null;
            
            if (targetPoolObj == null)
            {
                targetPoolObj = new GameObject("Target Pool");
                targetPool = targetPoolObj.AddComponent<TargetPool>();
                Debug.Log("✅ Target Pool created");
            }
            else
            {
                targetPool = targetPoolObj.GetComponent<TargetPool>();
                if (targetPool == null)
                {
                    targetPool = targetPoolObj.AddComponent<TargetPool>();
                    Debug.Log("✅ TargetPool component added to existing object");
                }
                else
                {
                    Debug.Log("ℹ️ Target Pool already exists with component, skipping creation");
                }
            }
            
            // Pools now load prefabs automatically from Assets/Prefabs/ folder
        }
        
        private void CheckAndCreateSpawner()
        {
            GameObject spawnerObj = GameObject.Find("Target Spawner");
            TargetSpawner spawner = null;
            
            if (spawnerObj == null)
            {
                spawnerObj = new GameObject("Target Spawner");
                spawnerObj.transform.position = spawnerPosition;
                spawnerObj.transform.rotation = Quaternion.Euler(spawnerRotation);
                
                spawner = spawnerObj.AddComponent<TargetSpawner>();
                Debug.Log("✅ Target Spawner created");
            }
            else
            {
                spawner = spawnerObj.GetComponent<TargetSpawner>();
                if (spawner == null)
                {
                    spawner = spawnerObj.AddComponent<TargetSpawner>();
                    Debug.Log("✅ TargetSpawner component added to existing object");
                }
                else
                {
                    Debug.Log("ℹ️ Target Spawner already exists with component, skipping creation");
                }
            }
            
            // Configure spawner settings for line spawning
            if (spawner != null)
            {
                var spawnDistanceField = typeof(TargetSpawner).GetField("spawnDistance", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var spawnHeightField = typeof(TargetSpawner).GetField("spawnHeight", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var maxTargetsPerSpawnField = typeof(TargetSpawner).GetField("maxTargetsPerSpawn", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var targetSpacingField = typeof(TargetSpawner).GetField("targetSpacing", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lineLengthField = typeof(TargetSpawner).GetField("lineLength", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (spawnDistanceField != null) spawnDistanceField.SetValue(spawner, 20f);
                if (spawnHeightField != null) spawnHeightField.SetValue(spawner, 1f);
                if (maxTargetsPerSpawnField != null) maxTargetsPerSpawnField.SetValue(spawner, 5);
                if (targetSpacingField != null) targetSpacingField.SetValue(spawner, 2.5f);
                if (lineLengthField != null) lineLengthField.SetValue(spawner, 10f);
            }
        }
        
        private void LinkAllReferences()
        {
            // Prefab references are now linked in CheckAndCreatePools
            
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
                var maxTargetsField = typeof(GameManager).GetField("maxTargets", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (bulletPoolField != null) bulletPoolField.SetValue(gameManager, FindFirstObjectByType<BulletPool>());
                if (targetPoolField != null) targetPoolField.SetValue(gameManager, FindFirstObjectByType<TargetPool>());
                if (targetSpawnerField != null) targetSpawnerField.SetValue(gameManager, FindFirstObjectByType<TargetSpawner>());
                if (cameraControllerField != null) cameraControllerField.SetValue(gameManager, FindFirstObjectByType<StaticCameraController>());
                if (maxTargetsField != null) maxTargetsField.SetValue(gameManager, maxTargets);
            }
            
            // Link GameUI references
            GameUI gameUI = FindFirstObjectByType<GameUI>();
            if (gameUI != null)
            {
                var timeTextField = typeof(GameUI).GetField("timeText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var targetsTextField = typeof(GameUI).GetField("targetsText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var gameStatusTextField = typeof(GameUI).GetField("gameStatusText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var restartButtonField = typeof(GameUI).GetField("restartButton", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (timeTextField != null) timeTextField.SetValue(gameUI, GameObject.Find("Time Text")?.GetComponent<TextMeshProUGUI>());
                if (targetsTextField != null) targetsTextField.SetValue(gameUI, GameObject.Find("Targets Text")?.GetComponent<TextMeshProUGUI>());
                if (gameStatusTextField != null) gameStatusTextField.SetValue(gameUI, GameObject.Find("Status Text")?.GetComponent<TextMeshProUGUI>());
                if (restartButtonField != null) restartButtonField.SetValue(gameUI, GameObject.Find("Restart Button")?.GetComponent<Button>());
            }
        }
    }
}
