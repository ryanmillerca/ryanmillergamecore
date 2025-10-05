namespace RyanMillerGameCore.Tiles
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(TileMap))]
    public class KTileMapInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Initialize List"))
            {
                ((TileMap)target).Init();
            }
        }
    }
}