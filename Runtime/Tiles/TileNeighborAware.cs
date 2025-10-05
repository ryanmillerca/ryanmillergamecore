namespace RyanMillerGameCore.Tiles
{
    using UnityEngine;
    using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class TileNeighborAware : TileBase
    {
        [Header("Manually Set")]
        [SerializeField] private Material tileMaterial;
        [SerializeField] private Mesh[] outerCornerMesh;
        [SerializeField] private Mesh[] innerCornerMesh;
        [SerializeField] private Mesh[] midMesh;
        [SerializeField] private Mesh[] solidWallMesh;
        [SerializeField] private Mesh outerCornerCollider;
        [SerializeField] private Mesh innerCornerCollider;
        [SerializeField] private Mesh midCollider;
        [SerializeField] private Mesh fillCollider;

        [Header("Automatically Set")]
        [SerializeField] private Transform neTransform;
        [SerializeField] private Transform seTransform;
        [SerializeField] private Transform swTransform;
        [SerializeField] private Transform nwTransform;
        [SerializeField] private MeshFilter neMeshFilter;
        [SerializeField] private MeshFilter seMeshFilter;
        [SerializeField] private MeshFilter swMeshFilter;
        [SerializeField] private MeshFilter nwMeshFilter;
        [SerializeField] private MeshCollider neCollider;
        [SerializeField] private MeshCollider seCollider;
        [SerializeField] private MeshCollider swCollider;
        [SerializeField] private MeshCollider nwCollider;
        [SerializeField] private NeighborMatch neighborMatch;

        private void SetCorner(MeshFilter corner, Vector3 newRotation, Mesh[] newMesh, MeshCollider cornerCollider, Mesh collisionMesh)
        {
            if (newMesh == null || newMesh.Length == 0)
            {
                corner.mesh = null;
                cornerCollider.enabled = false;
                return;
            }

            corner.transform.localEulerAngles = newRotation;
            Mesh meshChoice = newMesh[Mathf.Clamp(randomSeed, 0, newMesh.Length - 1)];
            corner.mesh = meshChoice;

            cornerCollider.sharedMesh = collisionMesh;
        }

        public override void Reset()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Setup " + gameObject.name);
#endif
            neTransform ??= GameObject.Find("NE").transform;
            seTransform ??= GameObject.Find("SE").transform;
            swTransform ??= GameObject.Find("SW").transform;
            nwTransform ??= GameObject.Find("NW").transform;
            neMeshFilter = SetupCornerMeshFilter(neTransform.gameObject);
            seMeshFilter = SetupCornerMeshFilter(seTransform.gameObject);
            swMeshFilter = SetupCornerMeshFilter(swTransform.gameObject);
            nwMeshFilter = SetupCornerMeshFilter(nwTransform.gameObject);
        }

        private MeshFilter SetupCornerMeshFilter(GameObject cornerGameObject)
        {
            MeshFilter cornerMeshFilter = cornerGameObject.GetComponent<MeshFilter>();
            if (cornerMeshFilter == null)
            {
                cornerMeshFilter = cornerGameObject.AddComponent<MeshFilter>();
            }
            if (outerCornerMesh.Length > 0 && outerCornerMesh[0] != null)
            {
                cornerMeshFilter.sharedMesh = outerCornerMesh[0];
            }
            MeshRenderer cornerMeshRenderer = cornerGameObject.GetComponent<MeshRenderer>();
            if (cornerMeshRenderer == null)
            {
                cornerMeshRenderer = cornerGameObject.AddComponent<MeshRenderer>();
            }

            cornerMeshRenderer.sharedMaterial = tileMaterial;
            return cornerMeshFilter;
        }
        protected override void ReparentChildren()
        {
            neTransform.gameObject.SetActive(true);
            seTransform.gameObject.SetActive(true);
            swTransform.gameObject.SetActive(true);
            nwTransform.gameObject.SetActive(true);

            neTransform.SetParent(transform);
            seTransform.SetParent(transform);
            swTransform.SetParent(transform);
            nwTransform.SetParent(transform);
        }

        public override void PostInitialize()
        {
            // 
            neighbors = TileMap.GetNeighborsFor(x, y);
            neighborMatch = new NeighborMatch()
            {
                N = neighbors.n.Equals(tileValue),
                NE = neighbors.ne.Equals(tileValue),
                E = neighbors.e.Equals(tileValue),
                SE = neighbors.se.Equals(tileValue),
                S = neighbors.s.Equals(tileValue),
                SW = neighbors.sw.Equals(tileValue),
                W = neighbors.w.Equals(tileValue),
                NW = neighbors.nw.Equals(tileValue)
            };
            
            // set objects active/inactive
            swTransform.gameObject.SetActive(tileValue > 0);
            seTransform.gameObject.SetActive(tileValue > 0);
            neTransform.gameObject.SetActive(tileValue > 0);
            nwTransform.gameObject.SetActive(tileValue > 0);
            
            // NE Corner
            // if fill
            if (neighborMatch.N && neighborMatch.NE && neighborMatch.E)
            {
                SetCorner(neMeshFilter, Vector3.zero, solidWallMesh, neCollider, fillCollider);
            }
            // if outer corner
            else if (!neighborMatch.N && !neighborMatch.E)
            {
                SetCorner(neMeshFilter, Vector3.zero, outerCornerMesh, neCollider, outerCornerCollider);
            }
            // if inner corner
            else if (neighborMatch.E && neighborMatch.N)
            {
                SetCorner(neMeshFilter, new Vector3(0, 180, 0), innerCornerMesh, neCollider, innerCornerCollider);
            }
            // if mid facing north
            else if (neighborMatch.N)
            {
                SetCorner(neMeshFilter, new Vector3(0, 270, 0), midMesh, neCollider, midCollider);
            }
            // if mid facing east
            else
            {
                SetCorner(neMeshFilter, new Vector3(0, 180, 0), midMesh, neCollider, midCollider);
            }

            // SE Corner
            // if fill
            if (neighborMatch.S && neighborMatch.SE && neighborMatch.E)
            {
                SetCorner(seMeshFilter, Vector3.zero, solidWallMesh, seCollider, fillCollider);
            }
            // if outer corner
            else if (!neighborMatch.S && !neighborMatch.E)
            {
                SetCorner(seMeshFilter, new Vector3(0, 90, 0), outerCornerMesh, seCollider, outerCornerCollider);
            }
            // if inner corner
            else if (neighborMatch.S && neighborMatch.E)
            {
                SetCorner(seMeshFilter, new Vector3(0, 270, 0), innerCornerMesh, seCollider, innerCornerCollider);
            }
            // if mid facing east
            else if (neighborMatch.E)
            {
                SetCorner(seMeshFilter, new Vector3(0, 0, 0), midMesh, seCollider, midCollider);
            }
            // if mid facing south
            else
            {
                SetCorner(seMeshFilter, new Vector3(0, 270, 0), midMesh, seCollider, midCollider);
            }

            // SW Corner
            // if fill
            if (neighborMatch.S && neighborMatch.SW && neighborMatch.W)
            {
                SetCorner(swMeshFilter, Vector3.zero, solidWallMesh, swCollider, fillCollider);
            }
            // if outer corner
            else if (!neighborMatch.S && !neighborMatch.W)
            {
                SetCorner(swMeshFilter, new Vector3(0, 180, 0), outerCornerMesh, swCollider, outerCornerCollider);
            }
            // if inner corner
            else if (neighborMatch.S && neighborMatch.W)
            {
                SetCorner(swMeshFilter, Vector3.zero, innerCornerMesh, swCollider, innerCornerCollider);
            }
            // if mid facing south
            else if (neighborMatch.W)
            {
                SetCorner(swMeshFilter, Vector3.zero, midMesh, swCollider, midCollider);
            }
            // if mid facing west
            else
            {
                SetCorner(swMeshFilter, new Vector3(0, 90, 0), midMesh, swCollider, midCollider);
            }

            // NW Corner
            // if fill
            if (neighborMatch.N && neighborMatch.NW && neighborMatch.W)
            {
                SetCorner(nwMeshFilter, Vector3.zero, solidWallMesh, nwCollider, fillCollider);
            }
            // if outer corner
            else if (!neighborMatch.N && !neighborMatch.W)
            {
                SetCorner(nwMeshFilter, new Vector3(0, 270, 0), outerCornerMesh, nwCollider, outerCornerCollider);
            }
            // if inner corner
            else if (neighborMatch.N && neighborMatch.W)
            {
                SetCorner(nwMeshFilter, new Vector3(0, 90, 0), innerCornerMesh, nwCollider, innerCornerCollider);
            }
            // if mid facing north
            else if (neighborMatch.W)
            {
                SetCorner(nwMeshFilter, new Vector3(0, 180, 0), midMesh, nwCollider, midCollider);
            }
            // if mid facing west
            else
            {
                SetCorner(nwMeshFilter, new Vector3(0, 90, 0), midMesh, nwCollider, midCollider);
            }
        }
    }
    
    [Serializable]
    public class NeighborMatch
    {
        public bool N;
        public bool NE;
        public bool E;
        public bool SE;
        public bool S;
        public bool SW;
        public bool W;
        public bool NW;
    }
}