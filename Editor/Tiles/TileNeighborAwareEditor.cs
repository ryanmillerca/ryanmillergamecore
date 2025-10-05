namespace RyanMillerGameCore.Tiles
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(TileNeighborAware))]
    public class TileNeighborAwareEditor : Editor
    {
        private bool _foldout;

        TileNeighborAware Tile => (TileNeighborAware)target;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Setup"))
            {
                Tile.Reset();
                EditorUtility.SetDirty(Tile);
            }
            _foldout = EditorGUILayout.BeginToggleGroup("Show Settings", _foldout);
            if (_foldout)
            {
                base.OnInspectorGUI();
            }

            EditorGUILayout.EndToggleGroup();
        }
    }
}