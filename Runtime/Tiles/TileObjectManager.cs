namespace RyanMillerGameCore.Tiles
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [RequireComponent(typeof(TileMap))]
    public class TileObjectManager : MonoBehaviour
    {
        [SerializeField] private TileBase tilePrefab;
        [SerializeField] private List<TileBase> activeTiles;
        [SerializeField] private TileMap tileMap;

        public TileMap TileMap
        {
            get
            {
                if (tileMap == null)
                {
                    tileMap = GetComponent<TileMap>();
                }

                return tileMap;
            }
        }

#if UNITY_EDITOR

        [ContextMenu("Tile Shadows On")]
        public void SetAllShadowsOn()
        {
            Object[] allMeshRenderers = GetComponentsInChildren<MeshRenderer>();
            Undo.RecordObjects(allMeshRenderers, "Set All Shadows On");
            foreach (var o in allMeshRenderers)
            {
                var mr = (MeshRenderer)o;
                mr.shadowCastingMode = ShadowCastingMode.On;
            }
        }

        [ContextMenu("Tile Shadows Off")]
        public void SetAllShadowsOff()
        {
            Object[] allMeshRenderers = GetComponentsInChildren<MeshRenderer>();
            Undo.RecordObjects(allMeshRenderers, "Set All Shadows Off");
            foreach (var o in allMeshRenderers)
            {
                var mr = (MeshRenderer)o;
                mr.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        public void RegeneratePrefabs()
        {
            DeleteAllTiles();
            CreateNewTileObjects();
        }

        private void DeleteAllTiles()
        {
            for (int i = transform.childCount-1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

        public void ChangeTile(int x, int y, int newValue)
        {
            if (tileMap.CoordIsValid(x, y) == false)
            {
                return;
            }

            tileMap.SetTile(x, y, newValue);
            TileBase ktb = GetTileWithCoord(x, y);
            ktb.ChangeValue(newValue);
        }

        public void SetAllTiles(int newValue)
        {
            for (int r = 0; r < tileMap.rows; r++)
            {
                for (int c = 0; c < tileMap.cols; c++)
                {
                    ChangeTile(c, r, newValue);
                }
            }

            RefreshAllTiles();
        }


        public void RefreshAllTiles()
        {
            foreach (TileBase k in activeTiles)
            {
                k.PostInitialize();
            }
        }

#endif

        
        private void CreateNewTileObjects()
        {
            if (tileMap == null)
            {
                tileMap = GetComponent<TileMap>();
            }

            tileMap.Init();

            int totalTiles = tileMap.cols * tileMap.rows;

            // if there are too many tiles, destory 'em all
            if (transform.childCount > totalTiles)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(transform.GetChild(i).gameObject);
                    }
                    else
                    {
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    }
                }
            }

            activeTiles ??= new List<TileBase>();
            activeTiles.Clear();

            // how many need to be spawned?
            TileBase[] tiles = GetComponentsInChildren<TileBase>();
            int numToSpawn = totalTiles - tiles.Length;
            Debug.Log("Spawning " + numToSpawn + " new tiles.");

            activeTiles.AddRange(tiles);
            for (int toSpawn = 0; toSpawn < numToSpawn; toSpawn++)
            {
                TileBase ktb = SpawnTile();
                activeTiles.Add(ktb);
            }

            for (int r = 0; r < tileMap.rows; r++)
            {
                for (int c = 0; c < tileMap.cols; c++)
                {
                    activeTiles[c + (r * tileMap.cols)].Initialize(c, r, tileMap.GetTileValueAt(c, r));
                }
            }

            foreach (TileBase ktb in activeTiles)
            {
                ktb.PostInitialize();
            }
        }

        private TileBase SpawnTile()
        {
            GameObject ktbGameObject;
            #if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    ktbGameObject = PrefabUtility.InstantiatePrefab(tilePrefab.gameObject, transform) as GameObject;
                }
                else
                {
                    ktbGameObject = Instantiate(tilePrefab.gameObject, transform, true) as GameObject;
                }
            #else
            ktbGameObject = Instantiate(tilePrefab.gameObject, transform, true) as GameObject;
            #endif
            return ktbGameObject.GetComponent<TileBase>();
        }
        
        private TileBase GetTileWithCoord(int x, int y)
        {
            foreach (TileBase t in activeTiles)
            {
                if (t.X == x && t.Y == y)
                {
                    return t;
                }
            }
            Debug.LogError("Failed to Get Tile at " + x + ", " + y);
            return null;
        }
    }
}