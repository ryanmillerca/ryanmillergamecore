namespace RyanMillerGameCore.Interactions.Editor {
#if UNITY_EDITOR
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(Checkpoint))]
    public class CheckpointEditor : Editor {
        
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            Checkpoint checkpoint = (Checkpoint)target;
            if (checkpoint.HasLocationID) {
                if (GUILayout.Button("Save CheckpointData to Location ID")) {
                    checkpoint.SaveCheckpointDataToLocationID();
                }
            }
        }
    }
#endif
}