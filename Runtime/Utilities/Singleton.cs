namespace RyanMillerGameCore
{
    using UnityEngine;

    abstract public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = false;
        [SerializeField] private bool preserveParentObject = false;

        private static T _Instance;
        private static bool isShuttingDown = false;

        public static T Instance
        {
            get
            {
                if (isShuttingDown) {
                    return null;
                }

                if (!_Instance)
                {
                    _Instance = FindFirstObjectByType<T>();
                    if (!_Instance) {
                        return null;
                    }
                }

                return _Instance;
            }
        }

        protected virtual void Awake()
        {
            if (_Instance && _Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _Instance = this as T;

            if (dontDestroyOnLoad)
            {
                if (preserveParentObject && transform.parent != null)
                {
                    Transform parent = transform.parent;
                    if (parent != null && parent != transform)
                    {
                        parent.SetParent(null);
                        DontDestroyOnLoad(parent.gameObject);
                    }
                    else
                    {
                        DontDestroyOnLoad(gameObject);
                    }
                }
                else
                {
                    if (transform == transform.root) {
                        DontDestroyOnLoad(gameObject);
                    }
                    else {
                        Debug.Log("Warning: " + this.name + " is marked as DontDestroyOnLoad. You should fix this.", gameObject);
                    }
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                _Instance = null;
            }

            if (!Application.isPlaying)
            {
                isShuttingDown = true;
            }
        }
    }
}