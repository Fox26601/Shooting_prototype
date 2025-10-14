using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private int maxTargets = 5;
        [SerializeField] private float gameTime = 60f; // 1 minute game timer
        
        [Header("References")]
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private TargetPool targetPool;
        [SerializeField] private MovingTargetPool movingTargetPool;
        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private StaticCameraController cameraController;
        [SerializeField] private PistolCameraController pistolCameraController;
        [SerializeField] private PistolIntegration pistolIntegration;
        [SerializeField] private RuntimeAnimationLoader runtimeAnimationLoader; // optional
        
        private bool isGameActive;
        private float currentGameTime;
        private int currentScore;
        private bool gameEnded;
        
        public static GameManager Instance { get; private set; }
        
        public bool IsGameActive => isGameActive;
        public int MaxTargets => maxTargets;
        public float GameTimeRemaining => currentGameTime;
        public int CurrentScore => currentScore;
        public bool GameEnded => gameEnded;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (autoStartGame)
            {
                StartGame();
            }
        }
        
        private void Update()
        {
            if (isGameActive && !gameEnded)
            {
                currentGameTime -= Time.deltaTime;
                
                if (currentGameTime <= 0f)
                {
                    currentGameTime = 0f;
                    CheckGameEndCondition();
                }
            }
        }
        
        private void InitializeGame()
        {
            // Initialize AudioManager first
            AudioManager.Instance?.SetMasterVolume(1f);
            AudioManager.Instance?.SetSFXVolume(1f);
            
            // Find components if not assigned
            if (bulletPool == null)
                bulletPool = FindFirstObjectByType<BulletPool>();
            
            if (targetPool == null)
                targetPool = FindFirstObjectByType<TargetPool>();
            
            if (movingTargetPool == null)
            {
                movingTargetPool = FindFirstObjectByType<MovingTargetPool>();
                if (movingTargetPool == null)
                {
                    // Create MovingTargetPool GameObject if not found
                    GameObject movingTargetPoolObj = new GameObject("Moving Target Pool");
                    movingTargetPool = movingTargetPoolObj.AddComponent<MovingTargetPool>();
                    Debug.Log("✅ MovingTargetPool created automatically");
                }
            }
            
            if (targetSpawner == null)
                targetSpawner = FindFirstObjectByType<TargetSpawner>();
            
            if (cameraController == null)
                cameraController = FindFirstObjectByType<StaticCameraController>();
            
            // Find pistol camera controller
            if (pistolCameraController == null)
                pistolCameraController = FindFirstObjectByType<PistolCameraController>();
            
            
            // Pistol уже настроен в сцене - не нужно искать дополнительные компоненты
            Debug.Log("ℹ️ Pistol already configured in scene - no additional setup needed");
            
			// Создаем недостающие UI компоненты
            CreateAmmoUI();
			CreateRestartButtonSpawner();
            
            // Validate all components for build
            ValidateComponents();
        }
        
        
        private void ValidateComponents()
        {
            bool allComponentsValid = true;
            
            if (bulletPool == null)
            {
                Debug.LogError("❌ BulletPool not found! Shooting will not work.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ BulletPool found and ready");
            }
            
            if (targetPool == null)
            {
                Debug.LogError("❌ TargetPool not found! Targets will not spawn.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ TargetPool found and ready");
            }
            
            if (movingTargetPool == null)
            {
                Debug.LogError("❌ MovingTargetPool not found! Moving targets will not spawn.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ MovingTargetPool found and ready");
            }
            
            if (targetSpawner == null)
            {
                Debug.LogError("❌ TargetSpawner not found! No targets will appear.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ TargetSpawner found and ready");
            }
            
            if (cameraController == null && pistolCameraController == null)
            {
                Debug.LogWarning("⚠️ No camera controller found! Creating PistolCameraController...");
                CreatePistolCameraController();
            }
            else if (pistolCameraController != null)
            {
                Debug.Log("✅ PistolCameraController found and ready");
            }
            else if (cameraController != null)
            {
                Debug.Log("✅ StaticCameraController found and ready");
            }
            
            // Pistol уже настроен в сцене - валидация не нужна
            Debug.Log("ℹ️ Pistol validation skipped - pistol already configured in scene");
            
            // Validate AudioManager
            if (AudioManager.Instance == null)
            {
                Debug.LogError("❌ AudioManager not found! Sound effects will not play.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ AudioManager found and ready");
            }
            
            if (allComponentsValid)
            {
                Debug.Log("🎮 All game components validated successfully! Game is ready to play.");
            }
            else
            {
                Debug.LogError("⚠️ Some components are missing! Game may not work properly.");
            }
        }
        
        public void StartGame()
        {
            if (isGameActive) return;
            
            isGameActive = true;
            gameEnded = false;
            currentGameTime = gameTime;
            currentScore = 0;
            
            // Start spawning targets
            if (targetSpawner != null)
            {
                targetSpawner.StartSpawning();
            }
            
            Debug.Log("Game Started! Time: 60 seconds, Score: 0");
        }
        
        public void EndGame()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            gameEnded = true;
            
            // Stop spawning targets
            if (targetSpawner != null)
            {
                targetSpawner.StopSpawning();
            }
            
            // Return all objects to pools
            if (bulletPool != null)
            {
                bulletPool.ReturnAllBullets();
            }
            
            if (targetPool != null)
            {
                targetPool.ReturnAllTargets();
            }
            
            if (movingTargetPool != null)
            {
                movingTargetPool.ReturnAllMovingTargets();
            }
            
            Debug.Log($"Game Ended! Final Score: {currentScore}");
        }
        
        public void RestartGame()
        {
            EndGame();
            StartGame();
        }
        
        private void CheckGameEndCondition()
        {
            if (currentScore == 0)
            {
                Debug.Log("LOSE GAME! Score: 0 points after 1 minute");
                EndGame();
            }
            else
            {
                Debug.Log($"WIN GAME! Score: {currentScore} points");
                EndGame();
            }
        }
        
        public void AddScore(int points)
        {
            if (isGameActive && !gameEnded)
            {
                currentScore += points;
                Debug.Log($"Score: {currentScore} (+{points})");
            }
        }
        
    private void CreatePistolCameraController()
    {
        // Ищем Pistol Instance в сцене
        GameObject pistolInstance = GameObject.Find("Pistol Instance");
        if (pistolInstance != null)
        {
            // Добавляем PistolCameraController к Pistol Instance
            pistolCameraController = pistolInstance.GetComponent<PistolCameraController>();
            if (pistolCameraController == null)
            {
                pistolCameraController = pistolInstance.AddComponent<PistolCameraController>();
                Debug.Log("✅ PistolCameraController created and added to Pistol Instance");
            }
        }
        else
        {
            Debug.LogError("❌ Pistol Instance not found! Cannot create PistolCameraController.");
        }
    }

    private void CreateAmmoUI()
    {
        // Ищем AmmoUI в сцене
        AmmoUI existingAmmoUI = FindFirstObjectByType<AmmoUI>();
        if (existingAmmoUI == null)
        {
            // Создаем новый GameObject для AmmoUI
            GameObject ammoUIObject = new GameObject("AmmoUI");
            ammoUIObject.AddComponent<AmmoUI>();
            Debug.Log("✅ AmmoUI created and added to scene");
        }
        else
        {
            Debug.Log("✅ AmmoUI already exists in scene");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

		private void CreateRestartButtonSpawner()
		{
			var existing = FindFirstObjectByType<RestartButtonSpawner>();
			if (existing == null)
			{
				GameObject spawnerObject = new GameObject("Restart Button Spawner");
				spawnerObject.AddComponent<RestartButtonSpawner>();
			}
		}
    }
}
