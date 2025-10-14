using UnityEngine;

namespace ShootingSystem
{
    public class RestartButton : MonoBehaviour
    {
        [Header("Button Settings")]
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private Color hitColor = Color.green;
        [SerializeField] private Color normalColor = Color.red;
		[SerializeField] private float colliderExtraPercent = 1f; // extra size percent for hitbox
        
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
            
            // Configure collider based on button size (1% larger than button)
            ConfigureTriggerZone();

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
            Debug.Log("🔴 Restart Button armed and ready for hits!");
        }
        
        private void ConfigureTriggerZone()
        {
			Collider collider = GetComponent<Collider>();
            if (collider == null) return;
            
            collider.isTrigger = true;
            
            if (collider is BoxCollider boxCollider)
            {
				float scale = 1f + Mathf.Max(0f, colliderExtraPercent) * 0.01f;
				MeshFilter meshFilter = GetComponent<MeshFilter>();
				if (meshFilter == null)
				{
					meshFilter = GetComponentInChildren<MeshFilter>();
				}
				
				if (meshFilter != null && meshFilter.sharedMesh != null)
				{
					Bounds local = meshFilter.sharedMesh.bounds;
					Bounds computed = ComputeBoundsInColliderLocal(boxCollider.transform, meshFilter.transform, local, scale);
					boxCollider.center = computed.center;
					boxCollider.size = computed.size;
				}
				else if (buttonRenderer != null)
				{
					Bounds world = buttonRenderer.bounds;
					Bounds localApprox = BoundsWorldToLocal(boxCollider.transform, world);
					Vector3 center = localApprox.center;
					Vector3 size = localApprox.size * scale;
					boxCollider.center = center;
					boxCollider.size = size;
				}
            }
        }
        
        private Bounds GetButtonBounds()
        {
            if (buttonRenderer != null)
            {
                return buttonRenderer.bounds;
            }
            
            // Fallback: use transform scale
            Vector3 scale = transform.localScale;
            return new Bounds(Vector3.zero, scale);
        }

		private static Bounds ComputeBoundsInColliderLocal(Transform colliderTransform, Transform meshTransform, Bounds meshLocalBounds, float scale)
		{
			Vector3 center = meshLocalBounds.center;
			Vector3 extents = meshLocalBounds.extents * scale;
			Vector3[] corners = new Vector3[8]
			{
				center + new Vector3(-extents.x, -extents.y, -extents.z),
				center + new Vector3(-extents.x, -extents.y,  extents.z),
				center + new Vector3(-extents.x,  extents.y, -extents.z),
				center + new Vector3(-extents.x,  extents.y,  extents.z),
				center + new Vector3( extents.x, -extents.y, -extents.z),
				center + new Vector3( extents.x, -extents.y,  extents.z),
				center + new Vector3( extents.x,  extents.y, -extents.z),
				center + new Vector3( extents.x,  extents.y,  extents.z)
			};
			Bounds localBounds = new Bounds();
			for (int i = 0; i < corners.Length; i++)
			{
				Vector3 world = meshTransform.TransformPoint(corners[i]);
				Vector3 local = colliderTransform.InverseTransformPoint(world);
				if (i == 0) localBounds = new Bounds(local, Vector3.zero);
				else localBounds.Encapsulate(local);
			}
			return localBounds;
		}

		private static Bounds BoundsWorldToLocal(Transform colliderTransform, Bounds worldBounds)
		{
			Vector3 center = worldBounds.center;
			Vector3 extents = worldBounds.extents;
			Vector3[] corners = new Vector3[8]
			{
				center + new Vector3(-extents.x, -extents.y, -extents.z),
				center + new Vector3(-extents.x, -extents.y,  extents.z),
				center + new Vector3(-extents.x,  extents.y, -extents.z),
				center + new Vector3(-extents.x,  extents.y,  extents.z),
				center + new Vector3( extents.x, -extents.y, -extents.z),
				center + new Vector3( extents.x, -extents.y,  extents.z),
				center + new Vector3( extents.x,  extents.y, -extents.z),
				center + new Vector3( extents.x,  extents.y,  extents.z)
			};
			Bounds localBounds = new Bounds();
			for (int i = 0; i < corners.Length; i++)
			{
				Vector3 local = colliderTransform.InverseTransformPoint(corners[i]);
				if (i == 0) localBounds = new Bounds(local, Vector3.zero);
				else localBounds.Encapsulate(local);
			}
			return localBounds;
		}
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"🔍 OnTriggerEnter: {other.name}, isHit: {isHit}, isArmed: {isArmed}");
            
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
