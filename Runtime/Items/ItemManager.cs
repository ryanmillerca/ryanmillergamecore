namespace RyanMillerGameCore.Items
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface IPoolable
    {
        void OnAcquire();
        void OnRelease();
    }

    public class ItemManager : Singleton<ItemManager>
    {
        [System.Serializable]
        public class ItemPool
        {
            public GameObject prefab;
            [HideInInspector] public ID id;
            [HideInInspector] public Queue<GameObject> pool = new Queue<GameObject>();
        }

        [Header("Item Prefabs")] [Tooltip("Assign prefabs that contain a component with an ID field")]
        public List<GameObject> itemPrefabs;

        [Header("Pooling Settings")] public int defaultPoolAmount = 5;
        public bool enableDiagnostics = false;

        private Dictionary<ID, ItemPool> poolLookup = new Dictionary<ID, ItemPool>();

        protected override void Awake()
        {
            base.Awake();

            // Basic mapping only — no instantiation
            foreach (GameObject prefab in itemPrefabs)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("Null prefab in itemPrefabs list.");
                    continue;
                }

                Item item = prefab.GetComponent<Item>();
                if (item == null || item.identifier == null)
                {
                    Debug.LogError($"Prefab '{prefab.name}' is missing an Item component or ID.");
                    continue;
                }
                ID id = item.identifier;
                
                if (id == null)
                {
                    Debug.LogError($"Prefab '{prefab.name}' is missing an ID component.");
                    continue;
                }

                if (!poolLookup.ContainsKey(id))
                {
                    poolLookup[id] = new ItemPool { prefab = prefab, id = id };
                }
            }
        }

        private void Start()
        {
            // Deferred instantiation
            foreach (var entry in poolLookup.Values)
            {
                for (int i = 0; i < defaultPoolAmount; i++)
                {
                    GameObject obj = Instantiate(entry.prefab);
                    obj.SetActive(false);
                    entry.pool.Enqueue(obj);
                }

                if (enableDiagnostics)
                {
                    Debug.Log($"[ItemManager] Initialized pool for '{entry.prefab.name}' with {defaultPoolAmount} items.");
                }
            }
        }

        public GameObject GetItem(ID identifier)
        {
            if (!poolLookup.TryGetValue(identifier, out var itemPool))
            {
                Debug.LogWarning($"[ItemManager] No pool found for ID: {identifier.name}");
                return null;
            }

            GameObject item;
            if (itemPool.pool.Count > 0)
            {
                item = itemPool.pool.Dequeue();
            }
            else
            {
                item = Instantiate(itemPool.prefab);
                if (enableDiagnostics)
                {
                    Debug.LogWarning($"[ItemManager] Pool for '{identifier.name}' was empty. Instantiated new item.");
                }
            }

            item.SetActive(true);
            item.GetComponent<IPoolable>()?.OnAcquire();
            return item;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null)
                return;

            Item item = obj.GetComponent<Item>();
            if (item == null || item.identifier == null || !poolLookup.TryGetValue(item.identifier, out var itemPool))
            {
                Debug.LogError("[ItemManager] Returned object does not belong to any known pool.");
                Destroy(obj); // Optional: discard unknown objects
                return;
            }

            obj.GetComponent<IPoolable>()?.OnRelease();
            obj.SetActive(false);
            itemPool.pool.Enqueue(obj);
        }

        [ContextMenu("Validate Prefabs")]
        private void ValidatePrefabs()
        {
            foreach (var prefab in itemPrefabs)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("Null prefab assigned.");
                    continue;
                }

                if (prefab.GetComponent<ID>() == null)
                {
                    Debug.LogWarning($"Prefab '{prefab.name}' is missing an ID component.");
                }
            }

            Debug.Log("Prefab validation complete.");
        }

        [ContextMenu("Log Pool Stats")]
        private void LogPoolStats()
        {
            foreach (var entry in poolLookup)
            {
                Debug.Log($"Pool '{entry.Value.prefab.name}': {entry.Value.pool.Count} item(s) available.");
            }
        }
    }
}