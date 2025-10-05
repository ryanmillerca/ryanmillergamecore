namespace RyanMillerGameCore.SaveSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Camera;

    [Serializable]
    public class SaveData
    {
        public List<string> keys = new List<string>();
        public SceneCheckpointData sceneCheckpoints = new SceneCheckpointData();

        public bool AddKey(string keyword, bool allowDuplicateEntries = false)
        {
            if (keys.Contains(keyword) == false || allowDuplicateEntries)
            {
                keys.Add(keyword);
                return true;
            }
            return false;
        }
        
        public bool RemoveKey(string key)
        {
            if (keys.Contains(key))
            {
                keys.Remove(key);
                return true;
            }
            return false;
        }

        public bool HasKey(string key)
        {
            return keys.Contains(key);
        }
    }
    
    [Serializable]
    public class SceneCheckpointData
    {
        [Serializable]
        public struct SceneCheckpointPair
        {
            public string sceneName;
            public CheckpointData checkpointData;
        }

        [SerializeField] private List<SceneCheckpointPair> sceneCheckpoints = new List<SceneCheckpointPair>();

        public void SetCheckpoint(string sceneName, Vector3 position, float facing, float cameraRotation = 0)
        {
            float camRotation = 0;
            Vector3 camPosition = Vector3.zero;
            if (CameraController.Instance)
            {
                camRotation = CameraController.Instance.TargetYRotation;
                camPosition = CameraController.Instance.transform.position;
            }
            for (int i = 0; i < sceneCheckpoints.Count; i++)
            {
                if (sceneCheckpoints[i].sceneName == sceneName)
                {
                    sceneCheckpoints[i] = new SceneCheckpointPair { sceneName = sceneName, checkpointData = new CheckpointData(position, facing, camRotation, camPosition) };
                    return;
                }
            }
            sceneCheckpoints.Add(new SceneCheckpointPair { sceneName = sceneName, checkpointData = new CheckpointData(position, facing, camRotation, camPosition) });
        }

        public CheckpointData TryGetCheckpoint(string sceneName)
        {
            foreach (var entry in sceneCheckpoints)
            {
                if (entry.sceneName == sceneName)
                {
                    return entry.checkpointData;
                }
            }
            return null;
        }
        
        public bool TryGetCheckpoint(string sceneName, out Vector3 position)
        {
            foreach (var entry in sceneCheckpoints)
            {
                if (entry.sceneName == sceneName)
                {
                    position = entry.checkpointData.position;
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }
        
        /// <summary>
        /// Remove all saved checkpoints for the given scene.
        /// Returns true if any entries were removed.
        /// </summary>
        public bool ClearScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) {
                return false;
            }
            bool removedAny = false;
            for (int i = sceneCheckpoints.Count - 1; i >= 0; i--)
            {
                Debug.Log("Found " + sceneName + " Checkpoint: " + sceneCheckpoints[i].checkpointData.position);
                if (sceneCheckpoints[i].sceneName == sceneName)
                {
                    Debug.Log("Deleted " + sceneName + " Checkpoint: " + sceneCheckpoints[i].checkpointData.position);
                    sceneCheckpoints.RemoveAt(i);
                    removedAny = true;
                }
            }
            return removedAny;
        }
    }
}