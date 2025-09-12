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
#if UNITY_EDITOR
                targetPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Target Prefab.prefab");
                if (targetPrefab != null)
                {
                    Debug.Log("✅ Target prefab loaded from Assets/Prefabs/");
                }
                else
                {
                    Debug.LogError("❌ Target prefab not found at Assets/Prefabs/Target Prefab.prefab");
                }
#endif
            }
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
            
            Debug.Log($"Target retrieved from pool at position: {position}. Pool size: {targetPool.Count}, Active targets: {activeTargets.Count}");
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
