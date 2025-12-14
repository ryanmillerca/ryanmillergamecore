namespace RyanMillerGameCore.Editor.Recipes {
	using UnityEngine;
	using UnityEditor;

	abstract public class AbstractRecipe : Editor {

		protected static Vector3 GetSpawnPosition() {
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView != null) {
				Vector3 cameraPosition = sceneView.camera.transform.position;
				Vector3 cameraForward = sceneView.rotation * Vector3.forward;
				RaycastHit hit;
				if (Physics.Raycast(cameraPosition, cameraForward, out hit, 100f)) {
					return hit.point + Vector3.up * 0.1f;
				}
				return cameraPosition + cameraForward * 2f;
			}
			return Vector3.zero;
		}

		protected static void FinishCreation(GameObject go, string name) {
			Undo.RegisterCreatedObjectUndo(go, name);
			Selection.activeGameObject = go;
			SceneView.lastActiveSceneView?.FrameSelected();
		}

		protected const string MenuPrefix  = "GameObject/RMGC Recipe/";
	}
}
