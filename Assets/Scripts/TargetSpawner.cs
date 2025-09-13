using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistance = 10f; // Further reduced distance for easier targeting
        [SerializeField] private float spawnHeight = 1f;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxTargetsPerSpawn = 5; // Spawn all targets in one row
        [SerializeField] private float targetSpacing = 2.5f;
        
        [Header("Line Spawn")]
        [SerializeField] private float lineLength = 10f;
        
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        
        private Coroutine spawnCoroutine;
        private bool isSpawning;
        
        private void Start()
        {
            if (cameraTransform == null)
            {
                var camera = FindFirstObjectByType<StaticCameraController>();
                if (camera != null)
                {
                    cameraTransform = camera.transform;
                }
            }
            
            StartSpawning();
        }
        
        public void StartSpawning()
        {
            if (isSpawning) return;
            
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnTargetsCoroutine());
        }
        
        public void StopSpawning()
        {
            if (!isSpawning) return;
            
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
        
        private IEnumerator SpawnTargetsCoroutine()
        {
            while (isSpawning)
            {
                yield return new WaitForSeconds(spawnInterval);
                
                if (TargetPool.Instance != null && cameraTransform != null)
                {
                    SpawnTargets();
                }
            }
        }
        
        private void SpawnTargets()
        {
            // Check if we've reached the maximum number of targets
            if (GameManager.Instance != null && TargetPool.Instance != null)
            {
                int currentTargets = TargetPool.Instance.GetActiveTargetCount();
                int maxTargets = GameManager.Instance.MaxTargets;
                
                Debug.Log($"Spawn check: Current targets: {currentTargets}, Max targets: {maxTargets}");
                
                if (currentTargets >= maxTargets)
                {
                    Debug.Log("Target limit reached, not spawning more targets");
                    return; // Don't spawn more targets if we've reached the limit
                }
                
                // Only spawn if we have no targets
                if (currentTargets == 0)
                {
                    // Spawn a complete row of targets
                    int targetsToSpawn = maxTargets;
                    
                    Debug.Log($"Spawning complete row of {targetsToSpawn} targets");
                    
                    // Always spawn in a line
                    SpawnTargetsInLine(targetsToSpawn);
                }
                else if (currentTargets > maxTargets)
                {
                    // Clean up excess targets
                    Debug.Log($"Too many targets ({currentTargets}), cleaning up excess");
                    TargetPool.Instance.ReturnAllTargets();
                }
                else
                {
                    Debug.Log($"Targets already exist ({currentTargets}), not spawning more");
                }
            }
        }
        
        private void SpawnTargetsInLine(int targetCount)
        {
            Vector3 lineCenter = GetSpawnPosition();
            
            // Calculate total line width
            float totalLineWidth = (targetCount - 1) * targetSpacing;
            float startOffset = -totalLineWidth * 0.5f;
            
            // Always use Vector3.right for consistent horizontal line
            Vector3 lineDirection = Vector3.right;
            
            Debug.Log($"🎯 Spawning {targetCount} targets in line. Center: {lineCenter}, Direction: {lineDirection}");
            
            for (int i = 0; i < targetCount; i++)
            {
                // Calculate position along the line using fixed right direction
                float offset = startOffset + (i * targetSpacing);
                Vector3 targetPosition = lineCenter + lineDirection * offset;
                
                TargetPool.Instance.GetTarget(targetPosition);
                Debug.Log($"🎯 Target {i + 1} spawned at position: {targetPosition}");
            }
        }
        
        private Vector3 GetSpawnPosition()
        {
            if (cameraTransform == null) return transform.position;
            
            // Get camera position
            Vector3 cameraPosition = cameraTransform.position;
            
            // Always spawn targets in front of camera (negative Z direction)
            // This ensures consistent positioning regardless of camera rotation
            // Force spawnDistance to be 10 for closer targets
            float actualSpawnDistance = 10f;
            Vector3 spawnPosition = cameraPosition + Vector3.forward * (-actualSpawnDistance);
            spawnPosition.y = cameraPosition.y + spawnHeight; // Keep relative to camera height
            
            // Round to avoid floating point precision issues
            spawnPosition.x = Mathf.Round(spawnPosition.x * 100f) / 100f;
            spawnPosition.y = Mathf.Round(spawnPosition.y * 100f) / 100f;
            spawnPosition.z = Mathf.Round(spawnPosition.z * 100f) / 100f;
            
            Debug.Log($"🎯 Spawn position calculated: Camera pos: {cameraPosition}, Fixed forward: Vector3.forward * (-{actualSpawnDistance}), Spawn pos: {spawnPosition}");
            
            return spawnPosition;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (cameraTransform == null) return;
            
            // Draw spawn line in front of camera
            Gizmos.color = Color.yellow;
            Vector3 lineCenter = GetSpawnPosition();
            
            // Always use Vector3.right for consistent line
            Vector3 lineDirection = Vector3.right;
            
            // Draw the spawn line
            Vector3 lineStart = lineCenter - lineDirection * (lineLength * 0.5f);
            Vector3 lineEnd = lineCenter + lineDirection * (lineLength * 0.5f);
            Gizmos.DrawLine(lineStart, lineEnd);
            
            // Draw spawn distance from camera
            Gizmos.color = Color.red;
            Gizmos.DrawLine(cameraTransform.position, lineCenter);
            
            // Draw spawn distance sphere
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cameraTransform.position, spawnDistance);
            
            // Draw target positions preview
            Gizmos.color = Color.green;
            for (int i = 0; i < maxTargetsPerSpawn; i++)
            {
                float offset = (i - (maxTargetsPerSpawn - 1) * 0.5f) * targetSpacing;
                Vector3 targetPos = lineCenter + lineDirection * offset;
                Gizmos.DrawWireCube(targetPos, Vector3.one * 0.5f);
            }
        }
        
        private void OnDestroy()
        {
            StopSpawning();
        }
    }
}
