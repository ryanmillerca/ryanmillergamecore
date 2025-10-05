namespace RyanMillerGameCore.SceneControl
{
	using UnityEngine;
	using SaveSystem;

	[CreateAssetMenu(fileName = "New Location ID", menuName = "RyanMillerGameCore/ID/Location ID")]
	public class LocationID : ID {

		public string SceneName {
			get {
				return sceneReference.SceneName;
			}
			set {
				sceneReference.SceneName = value;
			}
		}

		public CheckpointData CheckpointData {
			get {
				return checkpointData;
			}
			set {
				checkpointData = value;
			}
		}

		[SerializeField] private SceneReference sceneReference;
		[SerializeField] private CheckpointData checkpointData;
	}
}
