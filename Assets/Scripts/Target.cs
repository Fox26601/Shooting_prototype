using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class Target : MonoBehaviour, ITarget
    {
        [Header("Target Settings")]
        [SerializeField] private int maxHealth = 1; // Reduced to 1 for instant destruction
        [SerializeField] private float fallForce = 20f;
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private float boundaryCheckDistance = 30f; // Distance from spawn point to consider out of bounds
        [SerializeField] private float removalDelay = 5f; // Delay before removing target from field
        
        private int currentHealth;
        private Rigidbody rb;
        private bool isAlive;
        private bool hasFallen;
        private Coroutine returnToPoolCoroutine;
        private Coroutine removalDelayCoroutine;
        private Vector3 spawnPosition;
        private bool isOutOfBounds;
        private bool isMarkedForRemoval;
        
        public bool IsAlive => isAlive;
        
        private void Update()
        {
            // Check if target has fallen out of bounds
            if (isAlive && !isOutOfBounds && !isMarkedForRemoval)
            {
                float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);
                if (distanceFromSpawn > boundaryCheckDistance)
                {
                    Debug.Log($"Target {name} is out of bounds! Distance: {distanceFromSpawn:F1}m from spawn point");
                    isOutOfBounds = true;
                    
                    // Immediately remove from active count and trigger respawn
                    TargetPool.Instance?.RemoveFromActiveCount(this);
                    
                    // Check if we need to respawn targets immediately
                    var targetSpawner = FindFirstObjectByType<TargetSpawner>();
                    if (targetSpawner != null)
                    {
                        targetSpawner.CheckAndRespawnTargets();
                    }
                    
                    // Start removal delay for out-of-bounds targets
                    StartRemovalDelay();
                }
            }
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            rb.useGravity = true;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.mass = 2f; // Increased mass for stability
        }
        
        private void OnEnable()
        {
            ResetTarget();
        }
        
        public void Initialize(Vector3 position)
        {
            transform.position = position;
            spawnPosition = position; // Store spawn position for boundary checking
            ResetTarget();
        }
        
        private void ResetTarget()
        {
            currentHealth = maxHealth;
            isAlive = true;
            hasFallen = false;
            isOutOfBounds = false;
            isMarkedForRemoval = false;
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            if (returnToPoolCoroutine != null)
            {
                StopCoroutine(returnToPoolCoroutine);
                returnToPoolCoroutine = null;
            }
            
            if (removalDelayCoroutine != null)
            {
                StopCoroutine(removalDelayCoroutine);
                removalDelayCoroutine = null;
            }
        }
        
        public void TakeDamage(int damage)
        {
            if (!isAlive) 
            {
                Debug.Log($"Target {name} is already dead, ignoring damage");
                return;
            }
            
            Debug.Log($"🎯 TARGET HIT! {name} taking {damage} damage. Current health: {currentHealth}");
            
            currentHealth -= damage;
            
            // Add score when target is hit
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(10); // 10 points per hit
            }
            OnHit();
            
            if (currentHealth <= 0)
            {
                Debug.Log($"💥 TARGET DESTROYED! {name} is destroyed!");
                OnDestroyed();
            }
        }
        
        public void OnHit()
        {
            // Visual/audio feedback for hit
            Debug.Log($"Target {name} hit! Health remaining: {currentHealth}");
        }
        
        public void OnDestroyed()
        {
            if (!isAlive) return;
            
            isAlive = false;
            hasFallen = true;
            
            // Apply additional fall force for destroyed targets
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
            
            rb.AddForce(randomDirection * fallForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * fallForce * 0.5f, ForceMode.Impulse);
            
            Debug.Log($"💥 TARGET DESTROYED AND FALLING! {name} destroyed and falling! Force applied: {fallForce}");
            
            // Immediately remove from active count and trigger respawn
            TargetPool.Instance?.RemoveFromActiveCount(this);
            
            // Check if we need to respawn targets immediately
            var targetSpawner = FindFirstObjectByType<TargetSpawner>();
            if (targetSpawner != null)
            {
                targetSpawner.CheckAndRespawnTargets();
            }
            
            // Start delayed visual removal
            StartRemovalDelay();
        }
        
        
        private void StartRemovalDelay()
        {
            if (isMarkedForRemoval) return;
            
            isMarkedForRemoval = true;
            
            // Start delayed removal coroutine
            if (removalDelayCoroutine != null)
            {
                StopCoroutine(removalDelayCoroutine);
            }
            removalDelayCoroutine = StartCoroutine(RemovalDelayCoroutine());
        }
        
        private IEnumerator RemovalDelayCoroutine()
        {
            Debug.Log($"⏰ Target {name} marked for removal, waiting {removalDelay} seconds before returning to pool");
            yield return new WaitForSeconds(removalDelay);
            
            Debug.Log($"🗑️ Target {name} removal delay completed, returning to pool");
            ReturnToPool();
        }
        
        private void ReturnToPool()
        {
            if (returnToPoolCoroutine != null)
            {
                StopCoroutine(returnToPoolCoroutine);
                returnToPoolCoroutine = null;
            }
            
            if (removalDelayCoroutine != null)
            {
                StopCoroutine(removalDelayCoroutine);
                removalDelayCoroutine = null;
            }
            
            gameObject.SetActive(false);
            // Only add back to pool if not already removed from active count
            if (!isMarkedForRemoval)
            {
                TargetPool.Instance?.ReturnTarget(this);
            }
            else
            {
                // Just add back to pool without affecting active count
                TargetPool.Instance?.AddToPool(this);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasFallen && other.gameObject.layer == Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2)))
            {
                // Target hit the ground, immediately remove from active count and trigger respawn
                TargetPool.Instance?.RemoveFromActiveCount(this);
                
                // Check if we need to respawn targets immediately
                var targetSpawner = FindFirstObjectByType<TargetSpawner>();
                if (targetSpawner != null)
                {
                    targetSpawner.CheckAndRespawnTargets();
                }
                
                // Start removal delay
                StartRemovalDelay();
            }
        }
    }
}
