using UnityEngine;

namespace ShootingSystem
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private int maxTargets = 5;
        
        [Header("References")]
        [SerializeField] private BulletPool bulletPool;
        [SerializeField] private TargetPool targetPool;
        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private StaticCameraController cameraController;
        
        private bool isGameActive;
        
        public static GameManager Instance { get; private set; }
        
        public bool IsGameActive => isGameActive;
        public int MaxTargets => maxTargets;
        
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
            // Timer removed - game runs indefinitely
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
        }
        
        public void StartGame()
        {
            if (isGameActive) return;
            
            isGameActive = true;
            
            // Start spawning targets
            if (targetSpawner != null)
            {
                targetSpawner.StartSpawning();
            }
            
            Debug.Log("Game Started!");
        }
        
        public void EndGame()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            
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
            
            Debug.Log("Game Ended!");
        }
        
        public void RestartGame()
        {
            EndGame();
            StartGame();
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
