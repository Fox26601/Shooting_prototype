using UnityEngine;

namespace ShootingSystem
{
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 50f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private int damage = 10;
        
        private Rigidbody rb;
        private float currentLifetime;
        private bool isActive;
        
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
            rb.mass = 0.1f; // Light bullet for realistic physics
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                ReturnToPool();
            }
        }
        
        public void Fire(Vector3 direction, Vector3 position)
        {
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction);
            
            rb.linearVelocity = direction * speed;
            currentLifetime = lifetime;
            isActive = true;
            
            gameObject.SetActive(true);
            
            Debug.Log($"Bullet fired! Position: {position}, Direction: {direction}, Speed: {speed}, Velocity: {rb.linearVelocity}");
        }
        
        public void ReturnToPool()
        {
            isActive = false;
            rb.linearVelocity = Vector3.zero;
            gameObject.SetActive(false);
            
            BulletPool.Instance?.ReturnBullet(this);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!isActive) return;
            
            GameObject hitObject = collision.gameObject;
            Debug.Log($"Bullet hit: {hitObject.name}");
            
            var target = hitObject.GetComponent<ITarget>();
            if (target != null)
            {
                Debug.Log($"Target found! Dealing {damage} damage");
                target.TakeDamage(damage);
                
                // Apply physical impact to target
                ApplyImpactToTarget(collision, hitObject);
            }
            else
            {
                Debug.Log($"No ITarget component found on {hitObject.name}");
            }
            
            // Always return to pool after collision
            ReturnToPool();
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
    }
}
