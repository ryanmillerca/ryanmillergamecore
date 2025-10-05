namespace RyanMillerGameCore.Utilities
{
    using UnityEngine;

    public class PoolableObject : MonoBehaviour
    {
        public void OnObjectSpawn()
        {
            // This method is called when an object is spawned from the pool.
            // You can use this to reset any properties or states of the object.
        }

        public void ReturnToPool()
        {
            gameObject.SetActive(false);
            PoolManager.Instance.poolDictionary[gameObject.tag].Enqueue(gameObject);
        }
    }
}