using System.Collections.Generic;
using UnityEngine;

namespace ShootingSystem
{
    /// <summary>
    /// Object pool for MovingTarget instances
    /// Manages creation, reuse, and cleanup of moving targets
    /// </summary>
    public class MovingTargetPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject movingTargetPrefab;
        [SerializeField] private int poolSize = 10;
        [SerializeField] private Transform poolParent;
        
        private Queue<MovingTarget> movingTargetPool;
        private List<MovingTarget> activeMovingTargets;
        
        public static MovingTargetPool Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializePool()
        {
            movingTargetPool = new Queue<MovingTarget>();
            activeMovingTargets = new List<MovingTarget>();
            
            // Create pool parent if not assigned
            if (poolParent == null)
            {
                GameObject poolParentObj = new GameObject("Moving Target Pool");
                poolParent = poolParentObj.transform;
                poolParent.SetParent(transform);
            }
            
            // Pre-populate pool
            for (int i = 0; i < poolSize; i++)
            {
                MovingTarget movingTarget = CreateMovingTarget();
                movingTargetPool.Enqueue(movingTarget);
            }
            
            Debug.Log($"✅ Moving Target Pool initialized with {poolSize} targets");
        }
        
        private MovingTarget CreateMovingTarget()
        {
            GameObject movingTargetObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            movingTargetObj.name = "Moving Target";
            movingTargetObj.transform.localScale = new Vector3(1f, 2f, 0.2f);
            movingTargetObj.transform.SetParent(poolParent);
            
            // Add Rigidbody
            Rigidbody rb = movingTargetObj.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.mass = 2f;
            
            // Add MovingTarget script
            MovingTarget movingTarget = movingTargetObj.AddComponent<MovingTarget>();
            
            // Set material (blue for moving targets)
            Renderer renderer = movingTargetObj.GetComponent<Renderer>();
            renderer.material.color = Color.blue;
            
            movingTargetObj.SetActive(false);
            return movingTarget;
        }
        
        /// <summary>
        /// Get a moving target from the pool
        /// </summary>
        /// <param name="position">Position to spawn the target</param>
        /// <returns>MovingTarget instance or null if pool is empty</returns>
        public MovingTarget GetMovingTarget(Vector3 position)
        {
            MovingTarget movingTarget;
            
            if (movingTargetPool.Count > 0)
            {
                movingTarget = movingTargetPool.Dequeue();
            }
            else
            {
                // Create new target if pool is empty
                movingTarget = CreateMovingTarget();
            }
            
            // Configure and activate target
            movingTarget.transform.position = position;
            movingTarget.gameObject.SetActive(true);
            movingTarget.Initialize(position);
            
            activeMovingTargets.Add(movingTarget);
            
            Debug.Log($"✅ Moving target retrieved from pool at position: {position}. Pool size: {movingTargetPool.Count}, Active moving targets: {activeMovingTargets.Count}");
            
            return movingTarget;
        }
        
        /// <summary>
        /// Return a moving target to the pool
        /// </summary>
        /// <param name="movingTarget">Moving target to return</param>
        public void ReturnMovingTarget(MovingTarget movingTarget)
        {
            if (movingTarget == null) return;
            
            activeMovingTargets.Remove(movingTarget);
            movingTarget.gameObject.SetActive(false);
            movingTargetPool.Enqueue(movingTarget);
            
            Debug.Log($"🎯 Moving target returned to pool. Pool size: {movingTargetPool.Count}, Active moving targets: {activeMovingTargets.Count}");
        }
        
        /// <summary>
        /// Add a moving target to the pool (for cleanup)
        /// </summary>
        /// <param name="movingTarget">Moving target to add to pool</param>
        public void AddToPool(MovingTarget movingTarget)
        {
            if (movingTarget == null) return;
            
            if (!movingTargetPool.Contains(movingTarget))
            {
                movingTargetPool.Enqueue(movingTarget);
            }
        }
        
        /// <summary>
        /// Remove moving target from active count
        /// </summary>
        /// <param name="movingTarget">Moving target to remove</param>
        public void RemoveFromActiveCount(MovingTarget movingTarget)
        {
            if (movingTarget == null) return;
            
            activeMovingTargets.Remove(movingTarget);
            Debug.Log($"🎯 Moving target removed from active count. Active moving targets: {activeMovingTargets.Count}");
        }
        
        /// <summary>
        /// Get count of active moving targets
        /// </summary>
        /// <returns>Number of active moving targets</returns>
        public int GetActiveMovingTargetCount()
        {
            return activeMovingTargets.Count;
        }
        
        /// <summary>
        /// Return all moving targets to pool
        /// </summary>
        public void ReturnAllMovingTargets()
        {
            for (int i = activeMovingTargets.Count - 1; i >= 0; i--)
            {
                if (activeMovingTargets[i] != null)
                {
                    ReturnMovingTarget(activeMovingTargets[i]);
                }
            }
            
            Debug.Log("🎯 All moving targets returned to pool");
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
