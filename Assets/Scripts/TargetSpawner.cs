using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
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
            // Spawn initial targets immediately
            SpawnTargets();
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
        
        public void CheckAndRespawnTargets()
        {
            if (GameManager.Instance != null && TargetPool.Instance != null)
            {
                int currentTargets = TargetPool.Instance.GetActiveTargetCount();
                int maxTargets = GameManager.Instance.MaxTargets;
                
                // If no targets, spawn immediately
                if (currentTargets == 0)
                {
                    Debug.Log($"🎯 No targets left! Spawning {maxTargets} targets immediately");
                    SpawnTargetsInLine(maxTargets);
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
            // Always use fixed spawn position for consistency
            // This ensures targets spawn in the same position every time
            Vector3 spawnPosition = new Vector3(0f, 0.7f, -10f);
            
            Debug.Log($"🎯 Fixed spawn position: {spawnPosition}");
            
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
            Gizmos.DrawWireSphere(cameraTransform.position, 10f);
            
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
