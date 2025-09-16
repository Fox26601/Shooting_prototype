using UnityEngine;

namespace ShootingSystem
{
    public class RestartButtonSpawner : MonoBehaviour
    {
        [Header("Button Spawn Settings")]
        [SerializeField] private GameObject restartButtonPrefab;
        [SerializeField] private Transform cameraTransform;
        
        private GameObject currentButton;
        
        private void Start()
        {
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }
            
            LoadPrefabIfNeeded();
            SpawnRestartButton();
        }
        
        private void LoadPrefabIfNeeded()
        {
            if (restartButtonPrefab == null)
            {
                // Try to load prefab from Prefabs folder
                restartButtonPrefab = Resources.Load<GameObject>("Prefabs/Restart Button Prefab");
                
                if (restartButtonPrefab == null)
                {
                    Debug.LogError("❌ Restart Button Prefab not found in Resources/Prefabs/");
                }
                else
                {
                    Debug.Log("✅ Restart Button Prefab loaded from Resources/Prefabs/");
                }
            }
        }
        
        private void SpawnRestartButton()
        {
            if (restartButtonPrefab == null || cameraTransform == null) return;
            
            Vector3 spawnPosition = GetSpawnPosition();
            
            currentButton = Instantiate(restartButtonPrefab, spawnPosition, Quaternion.identity);
            currentButton.name = "Restart Button";
            
            Debug.Log($"🔴 Restart Button spawned at position: {spawnPosition}");
        }
        
        private Vector3 GetSpawnPosition()
        {
            // Fixed spawn position as specified in Inspector
            Vector3 spawnPosition = new Vector3(0f, 0.234f, -1.855f);
            
            Debug.Log($"🔴 Restart Button spawn position set to: {spawnPosition}");
            
            return spawnPosition;
        }
        
        private void OnDrawGizmosSelected()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            
            // Draw spawn position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPosition, 0.5f);
            
            // Draw line from origin to spawn position
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Vector3.zero, spawnPosition);
        }
    }
}
