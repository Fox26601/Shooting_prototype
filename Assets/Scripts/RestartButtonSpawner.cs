using UnityEngine;

namespace ShootingSystem
{
    public class RestartButtonSpawner : MonoBehaviour
    {
        [Header("Button Spawn Settings")]
        [SerializeField] private GameObject restartButtonPrefab;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 staticPosition = new Vector3(0f, 1.3f, -3f);
        [SerializeField] private bool alignToGround = true;
        
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
			if (restartButtonPrefab == null) return;
            
            Vector3 spawnPosition = GetSpawnPosition();
            
            currentButton = Instantiate(restartButtonPrefab, spawnPosition, Quaternion.identity);
            currentButton.name = "Restart Button";
            
            // Ensure required components exist/configured (safe, idempotent)
            var rbComp = currentButton.GetComponent<RestartButton>();
            if (rbComp == null) currentButton.AddComponent<RestartButton>();
            
            var collider = currentButton.GetComponent<Collider>();
            if (collider == null) collider = currentButton.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            // Size will be configured by RestartButton.ConfigureTriggerZone() in Awake()
            
            var rigid = currentButton.GetComponent<Rigidbody>();
            if (rigid == null) rigid = currentButton.AddComponent<Rigidbody>();
            rigid.isKinematic = true;
            rigid.useGravity = false;
            rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            
            Debug.Log($"🔴 Restart Button spawned at position: {spawnPosition}");
        }
        
        private Vector3 GetSpawnPosition()
        {
            if (alignToGround)
            {
                // Raycast straight down from a point above the desired static XZ
                Vector3 from = new Vector3(staticPosition.x, staticPosition.y + 5f, staticPosition.z);
                if (Physics.Raycast(from, Vector3.down, out RaycastHit hit, 20f))
                {
                    Vector3 pos = hit.point + new Vector3(0f, 0.11f, 0f);
                    return pos;
                }
            }
            return staticPosition;
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
