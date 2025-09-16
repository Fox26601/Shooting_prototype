using System.Collections.Generic;
using UnityEngine;

namespace ShootingSystem
{
    public class TargetPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private int poolSize = 20;
        [SerializeField] private Transform poolParent;
        
        private Queue<Target> targetPool;
        private List<Target> activeTargets;
        
        public static TargetPool Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadPrefabIfNeeded();
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void LoadPrefabIfNeeded()
        {
            if (targetPrefab == null)
            {
                // Try to load prefab from Resources folder (works in build)
                targetPrefab = Resources.Load<GameObject>("Prefabs/Target Prefab");
                
                if (targetPrefab == null)
                {
                    // Fallback: create target prefab at runtime
                    Debug.LogWarning("⚠️ Target prefab not found in Resources. Creating runtime prefab...");
                    targetPrefab = CreateTargetPrefabRuntime();
                }
                else
                {
                    Debug.Log("✅ Target prefab loaded from Resources");
                }
            }
        }
        
        private GameObject CreateTargetPrefabRuntime()
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Target Prefab";
            // Make target thicker to prevent falling - increased depth from 0.2f to 1f
            target.transform.localScale = new Vector3(1f, 2f, 1f);
            
            // Add Rigidbody
            Rigidbody rb = target.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 2f; // Increased mass for stability
            rb.linearDamping = 0.5f; // Add damping to reduce bouncing
            rb.angularDamping = 0.5f;
            
            // Add Target script
            target.AddComponent<Target>();
            
            // Add collider
            Collider collider = target.GetComponent<Collider>();
            collider.isTrigger = false;
            
            // Set material
            Renderer renderer = target.GetComponent<Renderer>();
            renderer.material.color = Color.red;
            
            return target;
        }
        
        private void InitializePool()
        {
            targetPool = new Queue<Target>();
            activeTargets = new List<Target>();
            
            if (poolParent == null)
            {
                poolParent = transform;
            }
            
            for (int i = 0; i < poolSize; i++)
            {
                CreateTarget();
            }
        }
        
        private void CreateTarget()
        {
            if (targetPrefab == null)
            {
                Debug.LogError("❌ Target prefab is null! Cannot create target.");
                return;
            }
            
            GameObject targetObj = Instantiate(targetPrefab, poolParent);
            Target target = targetObj.GetComponent<Target>();
            
            if (target == null)
            {
                target = targetObj.AddComponent<Target>();
            }
            
            targetObj.SetActive(false);
            targetPool.Enqueue(target);
        }
        
        public Target GetTarget(Vector3 position)
        {
            if (targetPool.Count == 0)
            {
                CreateTarget();
            }
            
            Target target = targetPool.Dequeue();
            target.Initialize(position);
            target.gameObject.SetActive(true);
            activeTargets.Add(target);
            
            // Check if target has proper collider
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null)
            {
                Debug.Log($"✅ Target retrieved from pool at position: {position}. Has collider: {targetCollider.name}, isTrigger: {targetCollider.isTrigger}. Pool size: {targetPool.Count}, Active targets: {activeTargets.Count}");
            }
            else
            {
                Debug.LogError($"❌ Target retrieved from pool at position: {position} but has NO COLLIDER! Pool size: {targetPool.Count}, Active targets: {activeTargets.Count}");
            }
            
            return target;
        }
        
        public void ReturnTarget(Target target)
        {
            if (activeTargets.Contains(target))
            {
                activeTargets.Remove(target);
                targetPool.Enqueue(target);
            }
        }
        
        public void RemoveFromActiveCount(Target target)
        {
            if (activeTargets.Contains(target))
            {
                activeTargets.Remove(target);
                Debug.Log($"🎯 Target {target.name} removed from active count immediately. Active targets: {activeTargets.Count}");
            }
        }
        
        public void AddToPool(Target target)
        {
            // Add target back to pool without affecting active count
            targetPool.Enqueue(target);
            Debug.Log($"🎯 Target {target.name} added back to pool after delay. Pool size: {targetPool.Count}");
        }
        
        public void ReturnAllTargets()
        {
            for (int i = activeTargets.Count - 1; i >= 0; i--)
            {
                activeTargets[i].gameObject.SetActive(false);
                ReturnTarget(activeTargets[i]);
            }
        }
        
        public int GetActiveTargetCount()
        {
            return activeTargets.Count;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
