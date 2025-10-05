using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    /// <summary>
    /// Moving target with animation system and double points reward
    /// Supports horizontal and diagonal movement patterns
    /// </summary>
    public class MovingTarget : MonoBehaviour, ITarget
    {
        [Header("Moving Target Settings")]
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private float fallForce = 20f;
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private float boundaryCheckDistance = 30f;
        [SerializeField] private float removalDelay = 5f;
        
        [Header("Movement Settings")]
        [SerializeField] private MovementType movementType = MovementType.Horizontal;
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private float movementRange = 4f;
        [SerializeField] private float movementDuration = 3f;
        
        private int currentHealth;
        private Rigidbody rb;
        private bool isAlive;
        private bool hasFallen;
        private Coroutine returnToPoolCoroutine;
        private Coroutine removalDelayCoroutine;
        private Vector3 spawnPosition;
        private bool isOutOfBounds;
        private bool isMarkedForRemoval;
        
        // Movement variables
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float movementTimer;
        private bool isMoving;
        
        public enum MovementType
        {
            Horizontal,
            Diagonal
        }
        
        public bool IsAlive => isAlive;
        public bool IsMoving => isMoving;
        
        private void Update()
        {
            // Handle movement animation
            if (isAlive && isMoving)
            {
                UpdateMovement();
            }
            
            // Check if target has fallen out of bounds
            if (isAlive && !isOutOfBounds && !isMarkedForRemoval)
            {
                float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);
                if (distanceFromSpawn > boundaryCheckDistance)
                {
                    Debug.Log($"Moving target {name} is out of bounds! Distance: {distanceFromSpawn:F1}m from spawn point");
                    isOutOfBounds = true;
                    
                    // Immediately remove from active count and trigger respawn
                    MovingTargetPool.Instance?.RemoveFromActiveCount(this);
                    
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
            rb.mass = 2f;
        }
        
        private void OnEnable()
        {
            ResetTarget();
        }
        
        public void Initialize(Vector3 position)
        {
            transform.position = position;
            spawnPosition = position;
            ResetTarget();
            StartMovement();
        }
        
        private void ResetTarget()
        {
            currentHealth = maxHealth;
            isAlive = true;
            hasFallen = false;
            isOutOfBounds = false;
            isMarkedForRemoval = false;
            isMoving = false;
            
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
        
        private void StartMovement()
        {
            startPosition = transform.position;
            isMoving = true;
            movementTimer = 0f;
            
            // Calculate initial target position based on movement type
            CalculateTargetPosition();
            
            Debug.Log($"🎯 Moving target {name} started {movementType} movement from {startPosition} to {targetPosition}");
        }
        
        private void CalculateTargetPosition()
        {
            Vector3 direction = Vector3.zero;
            
            switch (movementType)
            {
                case MovementType.Horizontal:
                    direction = Vector3.right * movementRange;
                    break;
                case MovementType.Diagonal:
                    direction = (Vector3.right + Vector3.forward) * movementRange;
                    break;
            }
            
            targetPosition = startPosition + direction;
        }
        
        private void UpdateMovement()
        {
            movementTimer += Time.deltaTime;
            
            // Calculate movement progress (0 to 1)
            float progress = (movementTimer % movementDuration) / movementDuration;
            
            // Use sine wave for smooth back-and-forth movement
            float sineProgress = Mathf.Sin(progress * Mathf.PI * 2) * 0.5f + 0.5f;
            
            // Interpolate position
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, sineProgress);
            transform.position = currentPosition;
        }
        
        public void TakeDamage(int damage)
        {
            if (!isAlive) 
            {
                Debug.Log($"Moving target {name} is already dead, ignoring damage");
                return;
            }
            
            Debug.Log($"🎯 MOVING TARGET HIT! {name} taking {damage} damage. Current health: {currentHealth}");
            
            currentHealth -= damage;
            
            // Add double score when moving target is hit
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(20); // 20 points for moving targets (double the normal 10)
            }
            OnHit();
            
            if (currentHealth <= 0)
            {
                Debug.Log($"💥 MOVING TARGET DESTROYED! {name} is destroyed!");
                OnDestroyed();
            }
        }
        
        public void OnHit()
        {
            // Visual/audio feedback for hit
            Debug.Log($"Moving target {name} hit! Health remaining: {currentHealth}");
        }
        
        public void OnDestroyed()
        {
            if (!isAlive) return;
            
            isAlive = false;
            hasFallen = true;
            isMoving = false; // Stop movement when destroyed
            
            // Apply additional fall force for destroyed targets
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
            
            rb.AddForce(randomDirection * fallForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * fallForce * 0.5f, ForceMode.Impulse);
            
            Debug.Log($"💥 MOVING TARGET DESTROYED AND FALLING! {name} destroyed and falling! Force applied: {fallForce}");
            
            // Immediately remove from active count and trigger respawn
            MovingTargetPool.Instance?.RemoveFromActiveCount(this);
            
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
            Debug.Log($"⏰ Moving target {name} marked for removal, waiting {removalDelay} seconds before returning to pool");
            yield return new WaitForSeconds(removalDelay);
            
            Debug.Log($"🗑️ Moving target {name} removal delay completed, returning to pool");
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
                MovingTargetPool.Instance?.ReturnMovingTarget(this);
            }
            else
            {
                // Just add back to pool without affecting active count
                MovingTargetPool.Instance?.AddToPool(this);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasFallen && other.gameObject.layer == Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2)))
            {
                // Play target landing sound effect
                AudioManager.Instance?.PlayTargetLandingSound();
                
                // Target hit the ground, immediately remove from active count and trigger respawn
                MovingTargetPool.Instance?.RemoveFromActiveCount(this);
                
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
        
        /// <summary>
        /// Set movement type for this target
        /// </summary>
        /// <param name="type">Movement type (Horizontal or Diagonal)</param>
        public void SetMovementType(MovementType type)
        {
            movementType = type;
        }
        
        /// <summary>
        /// Set movement parameters
        /// </summary>
        /// <param name="speed">Movement speed</param>
        /// <param name="range">Movement range</param>
        /// <param name="duration">Movement duration</param>
        public void SetMovementParameters(float speed, float range, float duration)
        {
            movementSpeed = speed;
            movementRange = range;
            movementDuration = duration;
        }
    }
}
