namespace RyanMillerGameCore.SaveSystem {
	using UnityEngine;

	[System.Serializable]
	public class CheckpointData {
		public CheckpointData(Vector3 position, float facing, float cameraRotation, Vector3 cameraPosition) {
			this.position = position;
			this.facing = facing;
			this.cameraPosition = cameraPosition;
			this.cameraRotation = cameraRotation;
		}
		
		public CheckpointData(Vector3 position, Quaternion rotation) {
			this.position = position;
			this.facing = rotation.eulerAngles.y;
		}

		public Quaternion rotation {
			get { return Quaternion.Euler(0, facing, 0); }
		}

		public Vector3 position;
		public float facing;
		public Vector3 cameraPosition;
		public float cameraRotation;
		public string checkpointID;
	}
}
