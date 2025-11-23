using UnityEditor;

namespace RyanMillerGameCore.Editor {
	[InitializeOnLoad]
	public static class LayerSetup {
		// List of layers you want to enforce (indices 0-31)
		private static readonly string[] desiredLayers = new string[] {
			"Default",
			"TransparentFX",
			"Ignore Raycast",
			"UICamera",
			"Water",
			"UI",
			"Ground",
			"Breakable",
			"Character",
			"Sensor",
			"Interactive",
			"PlayerBarrier",
			"PolishDetails"
		};

		// List of tags you want
		private static readonly string[] desiredTags = new string[] {
			"Obstacle",
			"Character",
			"Sensor"
		};

		static LayerSetup() {
			// Optional: run automatically when project opens
			EditorApplication.delayCall += CheckLayersOnLoad;
		}

		private static void CheckLayersOnLoad() {
			if (!AreLayersAndTagsSet()) {
				if (EditorUtility.DisplayDialog(
					"Missing Layers/Tags",
					"Your project is missing some required layers or tags for RMGC. Would you like to add them now?",
					"Yes", "Later")) {
					SetLayersAndTags();
				}
			}
		}

		[MenuItem("Edit/RMGC/Set Layers and Tags")]
		public static void SetLayersAndTagsMenu() {
			SetLayersAndTags();
			EditorUtility.DisplayDialog("RMGC", "Layers and tags setup complete!", "OK");
		}

		private static bool AreLayersAndTagsSet() {
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

			// Check layers
			SerializedProperty layers = tagManager.FindProperty("layers");
			foreach (string layer in desiredLayers) {
				if (string.IsNullOrEmpty(layer)) continue;
				bool found = false;
				for (int i = 0; i < layers.arraySize; i++) {
					if (layers.GetArrayElementAtIndex(i).stringValue == layer) {
						found = true;
						break;
					}
				}
				if (!found) return false;
			}

			// Check tags
			SerializedProperty tags = tagManager.FindProperty("tags");
			foreach (string tag in desiredTags) {
				bool found = false;
				for (int i = 0; i < tags.arraySize; i++) {
					if (tags.GetArrayElementAtIndex(i).stringValue == tag) {
						found = true;
						break;
					}
				}
				if (!found) return false;
			}

			return true;
		}

		private static void SetLayersAndTags() {
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			SerializedProperty tags = tagManager.FindProperty("tags");

			// Add layers
			for (int i = 0; i < desiredLayers.Length; i++) {
				if (i >= layers.arraySize) continue;
				if (!string.IsNullOrEmpty(desiredLayers[i]) && string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) {
					layers.GetArrayElementAtIndex(i).stringValue = desiredLayers[i];
				}
			}

			// Add tags
			foreach (string tag in desiredTags) {
				bool exists = false;
				for (int i = 0; i < tags.arraySize; i++) {
					if (tags.GetArrayElementAtIndex(i).stringValue == tag) {
						exists = true;
						break;
					}
				}
				if (!exists) {
					tags.InsertArrayElementAtIndex(tags.arraySize);
					tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
				}
			}

			tagManager.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
		}
	}
}
