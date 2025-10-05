namespace RyanMillerGameCore.Tiles
{
    using UnityEngine;
    using System;
    using Random = UnityEngine.Random;

    public class TileBase : MonoBehaviour
    {
        [Header("Tile Base")]
        [SerializeField] private Mesh[] shuffleMeshes;
        [SerializeField] private GameObject colliderAndVisuals;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private bool randomizeRotations = true;
        [SerializeField] private bool unparentOnStart = true;
        [SerializeField] protected int randomSeed = -1;
        
        [HideInInspector] [SerializeField] private int randomTile = -1;
        [HideInInspector] [SerializeField] private TileMap tileMap;
        [HideInInspector] [SerializeField] protected int x;
        [HideInInspector] [SerializeField] protected int y;
        [HideInInspector] [SerializeField] protected int tileValue;

        [NonSerialized] protected TileNeighbors neighbors;

        public int X
        {
            get => x;
            set => x = value;
        }
        
        public int Y
        {
            get => y;
            set => y = value;
        }

        protected TileMap TileMap
        {
            get
            {
                if (tileMap == null)
                {
                    tileMap = GetComponentInParent<TileMap>();
                }

                return tileMap;
            }
        }

        public virtual void PostInitialize()
        {
            if (colliderAndVisuals)
            {
                colliderAndVisuals.SetActive(tileValue > 0);
            }
        }

        protected virtual void ReparentChildren()
        {
        }

        public void ChangeValue(int newValue)
        {
            tileValue = newValue;
            PostInitialize();
        }

        public void Initialize(int _x, int _y, int _tileValue)
        {
            if (randomSeed < 0)
            {
                randomSeed = 0;
            }

            if (randomTile < 0)
            {
                randomTile = 0;
            }

            this.x = _x;
            this.y = _y;
            this.tileValue = _tileValue;
            transform.SetLocalPositionAndRotation(new Vector3(x, 0, y), Quaternion.Euler(0, 0, 0));
            transform.localScale = Vector3.one;
            if (shuffleMeshes is { Length: > 0 })
            {
                if (meshFilter)
                {
                    meshFilter.mesh = shuffleMeshes[randomTile];
                }
                if (randomizeRotations)
                {
                    meshFilter.transform.localEulerAngles = new Vector3(0, randomSeed * 90, 0);
                }
            }
        }

        private void Start()
        {
#if UNITY_EDITOR
            gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
            if (randomSeed < 0)
            {
                randomSeed = Random.Range(0, 4);
            }

            if (randomTile < 0)
            {
                randomTile = Random.Range(0, shuffleMeshes.Length);
            }

            if (unparentOnStart)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
#if UNITY_EDITOR
                    transform.GetChild(i).hideFlags = HideFlags.HideInHierarchy;
#endif
                }
                transform.DetachChildren();
            }
        }
        
        public virtual void Reset()
        {
            PostInitialize();
        }
    }
}