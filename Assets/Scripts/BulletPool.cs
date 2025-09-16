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
                // Try to load prefab from Resources folder (works in build)
                bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet Prefab");
                
                if (bulletPrefab == null)
                {
                    // Fallback: create bullet prefab at runtime
                    Debug.LogWarning("⚠️ Bullet prefab not found in Resources. Creating runtime prefab...");
                    bulletPrefab = CreateBulletPrefabRuntime();
                }
                else
                {
                    Debug.Log("✅ Bullet prefab loaded from Resources");
                }
            }
        }
        
        private GameObject CreateBulletPrefabRuntime()
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Bullet Prefab";
            bullet.transform.localScale = Vector3.one * 0.1f;
            
            // Add Rigidbody
            Rigidbody rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0f;
            
            // Add Bullet script
            bullet.AddComponent<Bullet>();
            
            // Add collider
            Collider collider = bullet.GetComponent<Collider>();
            collider.isTrigger = true;
            
            // Set material
            Renderer renderer = bullet.GetComponent<Renderer>();
            renderer.material.color = Color.yellow;
            
            return bullet;
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
