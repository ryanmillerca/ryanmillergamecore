namespace RyanMillerGameCore.Tiles
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [SelectionBase] 
    public class TileMap : MonoBehaviour
    {
        public List<int> tiles;
        public int rows = 25;
        public int cols = 25;

        private void Awake()
        {
            #if UNITY_EDITOR
            // in editor, these tiles are all prefabs
            // the tiles are optimized by modifying the parents of all tile meshes
            // so that the tiles can all exist in the same hierarchy level, encouraging static/dynamic batching or instancing 
            // it's also more optimized this way for fewer transforms to be calculated
            // in any case... we can't modify the tile mesh child objects while they are prefabs. 
            // so this loops through and unpacks all prefabs on start. it is editor only, since there are no prefabs at runtime.
            for (int i = 0; i < transform.childCount; i++)
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(transform.GetChild(i).gameObject))
                {
                    PrefabUtility.UnpackPrefabInstance(transform.GetChild(i).gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }
            #endif
        }

        public void Init()
        {
            tiles ??= new List<int>();
            if (tiles.Count == 0)
            {
                #if UNITY_EDITOR
                Undo.RecordObject(gameObject, "Initialize KTileMap " + gameObject.name);
                #endif
                tiles.Clear();
                for (int i = 0; i < rows * cols; i++)
                {
                    tiles.Add(0);
                }
            
            }
        }
    
        public bool CoordIsValid(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }
            if (x >= cols || y >= cols)
            {
                return false;
            }
            return true;
        }

        public int GetTileValueAt(int x, int y)
        {   
            int returnIndex = x + (cols * y);
            if (returnIndex >= 0 && returnIndex < tiles.Count)
            {
                return tiles[returnIndex];
            }
            else
            {
                return -1;
            }
        }

        public int[] PositionToGridCoord(Vector3 worldPosition)
        {
            int[] coord = new int[2];
            worldPosition -= transform.position;
            worldPosition.x /= transform.localScale.x;
            worldPosition.y /= transform.localScale.y;
            worldPosition.z /= transform.localScale.z;
            coord[0] = Mathf.RoundToInt(worldPosition.x);
            coord[1] = Mathf.RoundToInt(worldPosition.z);
            return coord;
        }
    
        public int GetTileIndexAt(int x, int y)
        {
            return x + (cols * y);
        }
    
        public void SetTile(int x, int y, int value)
        {
            if (x >= cols || y >= rows || x < 0 || y < 0){
                return;
            }
            int returnIndex = x + (cols * y);
            if (returnIndex >= 0 && returnIndex < tiles.Count)
            {
                tiles[x + (cols * y)] = value;
            }
        }
    
        public void SetTile(int index, int value)
        {
            tiles[index] = value;
        }

        public TileNeighbors GetNeighborsFor(int x, int y)
        {
            TileNeighbors neighbors = new TileNeighbors(
                GetTileValueAt(x, y + 1),
                GetTileValueAt(x + 1, y + 1),
                GetTileValueAt(x + 1, y),
                GetTileValueAt(x + 1, y - 1),
                GetTileValueAt(x, y - 1),
                GetTileValueAt(x - 1, y - 1),
                GetTileValueAt(x - 1, y),
                GetTileValueAt(x - 1, y + 1)
            );
            return neighbors;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 size = new Vector3(rows * transform.localScale.x, 0, cols * transform.localScale.z);
            Gizmos.DrawWireCube(transform.position + ((size - new Vector3(1,0,1)) * 0.5f), size);
        }
    }

    [Serializable]
    public class TileNeighbors
    {
        public TileNeighbors(int n, int ne, int e, int se, int s, int sw, int w, int nw)
        {
            this.n = n;
            this.ne = ne;
            this.e = e;
            this.se = se;
            this.s = s;
            this.sw = sw;
            this.w = w;
            this.nw = nw;
        }

        public int n;
        public int ne;
        public int e;
        public int se;
        public int s;
        public int sw;
        public int w;
        public int nw;
    }
}