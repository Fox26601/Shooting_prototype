using UnityEngine;
using System.Collections;

namespace ShootingSystem
{
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 25f; // Increased speed for faster bullet travel
        [SerializeField] private float lifetime = 10f; // Increased lifetime to reach targets
        [SerializeField] private int damage = 3; // Increased damage to destroy targets in one hit
        [SerializeField] private float hitReturnDelay = 5f; // Delay before returning to pool after hit
        [SerializeField] private float bulletMass = 1f; // Bullet mass for physics calculations
        
        private Rigidbody rb;
        private float currentLifetime;
        private bool isActive;
        private bool hasHitTarget;
        private Coroutine returnToPoolCoroutine;
        
        public int Damage => damage;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.mass = bulletMass; // Use serialized bullet mass for more impact
            rb.isKinematic = false; // Ensure rigidbody is not kinematic for collision detection
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection for fast objects
            
            // Force damage to be 3 for instant target destruction
            damage = 3;
            
            // Force speed to be 25 for faster bullet travel
            speed = 25f;
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                Debug.Log($"Bullet lifetime expired at position: {transform.position}");
                ReturnToPool();
            }
            
            // Debug bullet position every second
            if (Mathf.FloorToInt(currentLifetime) != Mathf.FloorToInt(currentLifetime + Time.deltaTime))
            {
                Debug.Log($"Bullet at position: {transform.position}, velocity: {rb.linearVelocity.magnitude:F1}");
            }
        }
        
        public void Fire(Vector3 direction, Vector3 position)
        {
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction);
            
            rb.linearVelocity = direction * speed;
            currentLifetime = lifetime;
            isActive = true;
            hasHitTarget = false;
            
            // Stop any existing return coroutine
            if (returnToPoolCoroutine != null)
            {
                StopCoroutine(returnToPoolCoroutine);
                returnToPoolCoroutine = null;
            }
            
            gameObject.SetActive(true);
            
            Debug.Log($"🔫 Bullet fired! Position: {position}, Direction: {direction}, Speed: {speed}, Damage: {damage}, Velocity: {rb.linearVelocity}");
        }
        
        public void ReturnToPool()
        {
            isActive = false;
            hasHitTarget = false;
            rb.linearVelocity = Vector3.zero;
            
            // Stop any running coroutine
            if (returnToPoolCoroutine != null)
            {
                StopCoroutine(returnToPoolCoroutine);
                returnToPoolCoroutine = null;
            }
            
            gameObject.SetActive(false);
            
            BulletPool.Instance?.ReturnBullet(this);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"🔍 OnTriggerEnter called! Bullet active: {isActive}, hasHitTarget: {hasHitTarget}, Hit object: {other.gameObject.name}");
            
            // Handle restart button triggers immediately
            var restartButton = other.GetComponent<RestartButton>();
            if (restartButton != null)
            {
                Debug.Log("🔴 Bullet hit Restart Button trigger");
                ReturnToPool();
                return;
            }
            
            if (!isActive || hasHitTarget) return;
            
            GameObject hitObject = other.gameObject;
            Debug.Log($"💥 Bullet triggered: {hitObject.name} at position: {transform.position}");
            
            // Check if we hit a target
            var target = hitObject.GetComponent<ITarget>();
            if (target != null)
            {
                Debug.Log($"🎯 TARGET HIT! Dealing {damage} damage to {hitObject.name}");
                target.TakeDamage(damage);
                
                // Play target hit sound effect
                AudioManager.Instance?.PlayTargetHitSound();
                
                // Apply physical impact to target (simplified for trigger)
                ApplyImpactToTarget(hitObject);
                
                // Mark as hit and start delayed return to pool
                hasHitTarget = true;
                isActive = false; // Stop movement
                rb.linearVelocity = Vector3.zero;
                
                // Start coroutine to return to pool after delay
                returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
                
                Debug.Log($"✅ Bullet hit target! Will return to pool in {hitReturnDelay} seconds");
            }
            else
            {
                // Play bullet impact sound when hitting non-target objects
                AudioManager.Instance?.PlayBulletImpactSound();
                
                Debug.Log($"❌ No ITarget component found on {hitObject.name}. Returning to pool immediately.");
                // For non-target objects, return immediately
                ReturnToPool();
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!isActive || hasHitTarget) return;
            
            GameObject hitObject = collision.gameObject;
            Debug.Log($"Bullet collided: {hitObject.name} at position: {collision.contacts[0].point}");
            
            // Check if we hit a target
            var target = hitObject.GetComponent<ITarget>();
            if (target != null)
            {
                Debug.Log($"Target found! Dealing {damage} damage to {hitObject.name}");
                target.TakeDamage(damage);
                
                // Play target hit sound effect
                AudioManager.Instance?.PlayTargetHitSound();
                
                // Apply physical impact to target
                ApplyImpactToTarget(collision, hitObject);
                
                // Mark as hit and start delayed return to pool
                hasHitTarget = true;
                isActive = false; // Stop movement
                rb.linearVelocity = Vector3.zero;
                
                // Start coroutine to return to pool after delay
                returnToPoolCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
                
                Debug.Log($"Bullet hit target! Will return to pool in {hitReturnDelay} seconds");
            }
            else
            {
                // Play bullet impact sound when hitting non-target objects
                AudioManager.Instance?.PlayBulletImpactSound();
                
                Debug.Log($"No ITarget component found on {hitObject.name}. Returning to pool immediately.");
                // For non-target objects, return immediately
                ReturnToPool();
            }
        }
        
        private void ApplyImpactToTarget(Collision collision, GameObject target)
        {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                // Calculate impact force based on bullet velocity
                Vector3 impactForce = rb.linearVelocity * rb.mass;
                
                // Apply force at collision point
                targetRb.AddForceAtPosition(impactForce, collision.contacts[0].point, ForceMode.Impulse);
                
                // Add some torque for realistic falling
                Vector3 torque = Vector3.Cross(impactForce, collision.contacts[0].normal) * 0.1f;
                targetRb.AddTorque(torque, ForceMode.Impulse);
                
                Debug.Log($"Applied impact force: {impactForce.magnitude} to target");
            }
        }
        
        private void ApplyImpactToTarget(GameObject target)
        {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                // Calculate impact force based on bullet velocity (simplified for trigger)
                Vector3 impactForce = rb.linearVelocity * rb.mass;
                
                // Apply force at target center
                targetRb.AddForce(impactForce, ForceMode.Impulse);
                
                // Add some random torque for realistic falling
                Vector3 randomTorque = Random.insideUnitSphere * impactForce.magnitude * 0.1f;
                targetRb.AddTorque(randomTorque, ForceMode.Impulse);
                
                Debug.Log($"Applied trigger impact force: {impactForce.magnitude} to target");
            }
        }
        
        private IEnumerator ReturnToPoolAfterDelay()
        {
            Debug.Log($"Starting {hitReturnDelay} second delay before returning bullet to pool");
            yield return new WaitForSeconds(hitReturnDelay);
            
            Debug.Log("Delay completed, returning bullet to pool");
            ReturnToPool();
        }
    }
}
