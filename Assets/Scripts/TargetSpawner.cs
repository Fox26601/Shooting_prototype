using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class TargetSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistance = 20f;
        [SerializeField] private float spawnHeight = 1f;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxTargetsPerSpawn = 5;
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
                
                if (currentTargets >= maxTargets)
                {
                    return; // Don't spawn more targets if we've reached the limit
                }
                
                // Calculate how many targets we can still spawn
                int availableSlots = maxTargets - currentTargets;
                int targetsToSpawn = Mathf.Min(Random.Range(1, maxTargetsPerSpawn + 1), availableSlots);
                
                // Always spawn in a line
                SpawnTargetsInLine(targetsToSpawn);
            }
        }
        
        private void SpawnTargetsInLine(int targetCount)
        {
            Vector3 lineCenter = GetSpawnPosition();
            
            for (int i = 0; i < targetCount; i++)
            {
                // Calculate position along the line
                float offset = (i - (targetCount - 1) * 0.5f) * targetSpacing;
                Vector3 targetPosition = lineCenter + transform.right * offset;
                
                // Ensure targets are within the line length
                if (Mathf.Abs(offset) <= lineLength * 0.5f)
                {
                    TargetPool.Instance.GetTarget(targetPosition);
                    Debug.Log($"Target spawned at position: {targetPosition}");
                }
            }
        }
        
        private Vector3 GetSpawnPosition()
        {
            if (cameraTransform == null) return transform.position;
            
            // Get camera position and forward direction
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 cameraForward = cameraTransform.forward;
            
            // Calculate spawn position directly in front of the camera
            Vector3 spawnPosition = cameraPosition + cameraForward * spawnDistance;
            spawnPosition.y = cameraPosition.y + spawnHeight; // Keep relative to camera height
            
            return spawnPosition;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (cameraTransform == null) return;
            
            // Draw spawn line in front of camera
            Gizmos.color = Color.yellow;
            Vector3 lineCenter = GetSpawnPosition();
            
            // Draw the spawn line
            Vector3 lineStart = lineCenter - transform.right * (lineLength * 0.5f);
            Vector3 lineEnd = lineCenter + transform.right * (lineLength * 0.5f);
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
                Vector3 targetPos = lineCenter + transform.right * offset;
                Gizmos.DrawWireCube(targetPos, Vector3.one * 0.5f);
            }
        }
        
        private void OnDestroy()
        {
            StopSpawning();
        }
    }
}
