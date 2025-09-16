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
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isHit) return;
            
            // Check if hit by bullet (check component instead of tag)
            if (other.GetComponent<Bullet>() != null)
            {
                Debug.Log("🔴 Restart Button hit by bullet!");
                OnButtonHit();
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (isHit) return;
            
            // Check if hit by bullet (check component instead of tag)
            if (collision.gameObject.GetComponent<Bullet>() != null)
            {
                Debug.Log("🔴 Restart Button hit by bullet!");
                OnButtonHit();
            }
        }
        
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
