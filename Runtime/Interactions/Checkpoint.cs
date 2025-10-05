namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using SaveSystem;
    using Character;
    using SceneControl;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif
    using UnityEngine.SceneManagement;

    public class Checkpoint : ColliderSensor
    {
        [SerializeField] private bool saveImmediately = true;
        [SerializeField] private CheckpointRotation checkpointRotation = CheckpointRotation.UseCheckpointRotation;
        [SerializeField] private bool isSpawnPoint = false;
        [SerializeField] private bool showLogs = false;
        [SerializeField, Tooltip("Optional: Tag with an ID")] private LocationID locationID;
        
        public bool HasLocationID {
            get { return locationID != null; }
        }

        public bool IsSpawnPoint {
            get {
                return isSpawnPoint;
            }
        }

        public void SaveCheckpointDataToLocationID() {
    #if UNITY_EDITOR
            Undo.RecordObject(this,"Save Checkpoint Data to LocationID");
    #endif
            if (!HasLocationID) {
                Debug.LogWarning("No LocationID assigned to this Checkpoint.");
                return;
            }
            if (!DataManager.Instance) {
                Debug.LogWarning("DataManager not found, cannot save checkpoint data.");
                return;
            }
            locationID.CheckpointData = new CheckpointData(transform.position, transform.rotation);
        }

        protected override void ItemEnteredTrigger(Collider item) {
            if (!DataManager.Instance) {
                Log("DataManager not found, cannot set checkpoint.");
                return;
            }
            float facing = 0;
            switch (checkpointRotation) {
                case CheckpointRotation.UseCheckpointRotation:
                    facing = transform.eulerAngles.y;
                    break;
                case CheckpointRotation.UsePlayerRotation: {
                    CharacterMovement characterMovement = item.GetComponentInParent<CharacterMovement>();
                    facing = characterMovement.CharacterViewRotation;
                    break;
                }
            }
            Log("Setting checkpoint at " + transform.position + " facing " + facing);
            DataManager.Instance.SetCheckpoint(transform.position, facing, saveImmediately);
        }

#if UNITY_EDITOR
        [ContextMenu("Clear this Scene's Checkpoint Save Data")]
        public void ClearCheckpoints() {
            string sceneName = "";
            if (!Application.isPlaying) {
                sceneName = EditorSceneManager.GetActiveScene().name;
            }
            else {
                sceneName = SceneManager.GetActiveScene().name;
            }
            DataManager.Instance.ClearCheckpointsInScene(sceneName);
        }
#endif

        private void Log(string message) {
            if (showLogs) {
                Debug.Log(this.gameObject.name + ": " + message, this);
            }
        }
    }

    public enum CheckpointRotation
    {
        UseCheckpointRotation = 0,
        UsePlayerRotation = 1
    }

}