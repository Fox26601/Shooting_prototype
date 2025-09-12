using System.Collections.Generic;
using UnityEngine;

namespace ShootingSystem
{
    public class BulletPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int poolSize = 50;
        [SerializeField] private Transform poolParent;
        
        private Queue<Bullet> bulletPool;
        private List<Bullet> activeBullets;
        
        public static BulletPool Instance { get; private set; }
        
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
            if (bulletPrefab == null)
            {
#if UNITY_EDITOR
                bulletPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullet Prefab.prefab");
                if (bulletPrefab != null)
                {
                    Debug.Log("✅ Bullet prefab loaded from Assets/Prefabs/");
                }
                else
                {
                    Debug.LogError("❌ Bullet prefab not found at Assets/Prefabs/Bullet Prefab.prefab");
                }
#endif
            }
        }
        
        private void InitializePool()
        {
            bulletPool = new Queue<Bullet>();
            activeBullets = new List<Bullet>();
            
            if (poolParent == null)
            {
                poolParent = transform;
            }
            
            for (int i = 0; i < poolSize; i++)
            {
                CreateBullet();
            }
        }
        
        private void CreateBullet()
        {
            if (bulletPrefab == null)
            {
                Debug.LogError("❌ Bullet prefab is null! Cannot create bullet.");
                return;
            }
            
            GameObject bulletObj = Instantiate(bulletPrefab, poolParent);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            
            if (bullet == null)
            {
                bullet = bulletObj.AddComponent<Bullet>();
            }
            
            bulletObj.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
        
        public Bullet GetBullet()
        {
            if (bulletPool.Count == 0)
            {
                CreateBullet();
            }
            
            Bullet bullet = bulletPool.Dequeue();
            activeBullets.Add(bullet);
            
            Debug.Log($"Bullet retrieved from pool. Pool size: {bulletPool.Count}, Active bullets: {activeBullets.Count}");
            return bullet;
        }
        
        public void ReturnBullet(Bullet bullet)
        {
            if (activeBullets.Contains(bullet))
            {
                activeBullets.Remove(bullet);
                bulletPool.Enqueue(bullet);
            }
        }
        
        public void ReturnAllBullets()
        {
            for (int i = activeBullets.Count - 1; i >= 0; i--)
            {
                activeBullets[i].ReturnToPool();
            }
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
