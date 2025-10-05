using UnityEngine;

namespace ShootingSystem
{
    public class RestartButton : MonoBehaviour
    {
        [Header("Button Settings")]
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private Color hitColor = Color.green;
        [SerializeField] private Color normalColor = Color.red;
        
        private Renderer buttonRenderer;
        private bool isHit;
        private bool isArmed;
        
        private void Awake()
        {
            buttonRenderer = GetComponent<Renderer>();
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponentInChildren<Renderer>();
            }
            
            // Set initial bright red color
            if (buttonRenderer != null)
            {
                Material material = buttonRenderer.material;
                material.color = new Color(1f, 0f, 0f, 1f); // Pure bright red
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Smoothness", 0.1f);
                buttonRenderer.material = material;
            }
            
            // Ensure collider is properly configured for fast bullets
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                // Increase collider size for better detection with fast bullets
                if (collider is BoxCollider boxCollider)
                {
                    boxCollider.size = new Vector3(3f, 1.2f, 3f);
                    boxCollider.center = new Vector3(0f, 0.6f, 1.5f);
                }
            }

            // Ensure there is a Rigidbody for trigger callbacks reliability
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
            
            isArmed = false;
            Invoke(nameof(ArmTrigger), 0.05f);
        }
        
        private void ArmTrigger()
        {
            isArmed = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isHit || !isArmed) return;
            
            // Check if hit by bullet (check component instead of tag)
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                Debug.Log($"🔴 Restart Button hit by bullet! Bullet active: {bullet.gameObject.activeInHierarchy}");
                OnButtonHit();
            }
            else
            {
                Debug.Log($"🔴 Triggered by {other.name} but not a bullet");
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isHit || !isArmed) return;
            if (other.GetComponent<Bullet>() != null)
            {
                Debug.Log("🔴 Restart Button registered hit on TriggerStay");
                OnButtonHit();
            }
        }
        
        // Collision handler is not needed when using trigger; keeping only trigger path
        
        private void OnButtonHit()
        {
            if (isHit) return;
            
            isHit = true;
            
            // Change color to indicate hit
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = hitColor;
            }
            
            // Restart the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
                Debug.Log("🔄 Game restarted by button!");
            }
            
            // Respawn button after delay
            Invoke(nameof(RespawnButton), respawnDelay);
        }
        
        private void RespawnButton()
        {
            isHit = false;
            
            // Reset color
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = normalColor;
            }
            
            Debug.Log("🔴 Restart Button respawned!");
        }
    }
}
