namespace RyanMillerGameCore.SceneControl {
	using UnityEngine;
	using System;

#if UNITY_EDITOR
	using UnityEditor;
#endif

	[Serializable]
	public class SceneReference : ISerializationCallbackReceiver {
#if UNITY_EDITOR
		[SerializeField] private SceneAsset sceneAsset;
#endif
		[SerializeField, HideInInspector] private string sceneName;

		public string SceneName {
			get {
				return sceneName;
			}
			set {
				sceneName = value;
			}
		}

		public void OnBeforeSerialize() {
#if UNITY_EDITOR
			sceneName = sceneAsset != null ? sceneAsset.name : string.Empty;
#endif
		}

		public void OnAfterDeserialize() { }
	}
}
