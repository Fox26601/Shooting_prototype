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
        
        private int currentHealth;
        private Rigidbody rb;
        private bool isAlive;
        private bool hasFallen;
        private Coroutine returnToPoolCoroutine;
        private Vector3 spawnPosition;
        private bool isOutOfBounds;
        
        public bool IsAlive => isAlive;
        
        private void Update()
        {
            // Check if target has fallen out of bounds
            if (isAlive && !isOutOfBounds)
            {
                float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);
                if (distanceFromSpawn > boundaryCheckDistance)
                {
                    Debug.Log($"Target {name} is out of bounds! Distance: {distanceFromSpawn:F1}m from spawn point");
                    isOutOfBounds = true;
                    
                    // Return to pool immediately for out-of-bounds targets
                    ReturnToPool();
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
            rb.mass = 1f; // Standard mass for targets
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
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            if (returnToPoolCoroutine != null)
            {
                StopCoroutine(returnToPoolCoroutine);
                returnToPoolCoroutine = null;
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
            
            // Return to pool immediately for instant respawn
            ReturnToPool();
        }
        
        
        private void ReturnToPool()
        {
            if (returnToPoolCoroutine != null)
            {
                StopCoroutine(returnToPoolCoroutine);
                returnToPoolCoroutine = null;
            }
            
            gameObject.SetActive(false);
            TargetPool.Instance?.ReturnTarget(this);
            
            // Check if we need to respawn targets immediately
            var targetSpawner = FindFirstObjectByType<TargetSpawner>();
            if (targetSpawner != null)
            {
                targetSpawner.CheckAndRespawnTargets();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasFallen && other.gameObject.layer == Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2)))
            {
                // Target hit the ground, return to pool immediately
                ReturnToPool();
            }
        }
    }
}
