using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class Target : MonoBehaviour, ITarget
    {
        [Header("Target Settings")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float fallForce = 20f;
        [SerializeField] private float returnToPoolDelay = 5f;
        [SerializeField] private LayerMask groundLayer = 1;
        
        private int currentHealth;
        private Rigidbody rb;
        private bool isAlive;
        private bool hasFallen;
        private Coroutine returnToPoolCoroutine;
        
        public bool IsAlive => isAlive;
        
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
            ResetTarget();
        }
        
        private void ResetTarget()
        {
            currentHealth = maxHealth;
            isAlive = true;
            hasFallen = false;
            
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
            if (!isAlive) return;
            
            Debug.Log($"Target {name} taking {damage} damage. Current health: {currentHealth}");
            
            currentHealth -= damage;
            OnHit();
            
            if (currentHealth <= 0)
            {
                Debug.Log($"Target {name} destroyed!");
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
            
            Debug.Log($"Target {name} destroyed and falling!");
            
            // Start return to pool timer
            returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }
        
        private IEnumerator ReturnToPoolAfterDelay()
        {
            yield return new WaitForSeconds(returnToPoolDelay);
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
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (hasFallen && other.gameObject.layer == Mathf.RoundToInt(Mathf.Log(groundLayer.value, 2)))
            {
                // Target hit the ground, start return timer
                if (returnToPoolCoroutine == null)
                {
                    returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
                }
            }
        }
    }
}
