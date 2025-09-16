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
        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private StaticCameraController cameraController;
        
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
            // Find components if not assigned
            if (bulletPool == null)
                bulletPool = FindFirstObjectByType<BulletPool>();
            
            if (targetPool == null)
                targetPool = FindFirstObjectByType<TargetPool>();
            
            if (targetSpawner == null)
                targetSpawner = FindFirstObjectByType<TargetSpawner>();
            
            if (cameraController == null)
                cameraController = FindFirstObjectByType<StaticCameraController>();
            
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
            
            if (targetSpawner == null)
            {
                Debug.LogError("❌ TargetSpawner not found! No targets will appear.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ TargetSpawner found and ready");
            }
            
            if (cameraController == null)
            {
                Debug.LogError("❌ StaticCameraController not found! Camera controls will not work.");
                allComponentsValid = false;
            }
            else
            {
                Debug.Log("✅ StaticCameraController found and ready");
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
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
