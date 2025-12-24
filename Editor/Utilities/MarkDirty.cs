#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MarkDirty : MonoBehaviour {
	[MenuItem("GameObject/Mark Dirty", priority = 1000)]
	public static void MarkThisDirty() {
		var selected = Selection.activeGameObject;
		if (selected != null) {
			// Force serialization refresh
			EditorUtility.SetDirty(selected);

			// Also mark any components as dirty
			foreach (var component in selected.GetComponents<Component>()) {
				if (component != null)
					EditorUtility.SetDirty(component);
			}

			// Force a reimport of the asset if it's an asset
			var path = AssetDatabase.GetAssetPath(selected);
			if (!string.IsNullOrEmpty(path)) {
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}

			// Refresh the inspector
			SceneView.RepaintAll();
		}
	}
}
#endif
