using UnityEngine;
using UnityEditor;
using System.Linq;

namespace RyanMillerGameCore.Factory {
	public static class ComponentRecipeExtractor {
		[MenuItem("Tools/Factory/Extract Recipe From Selected")]
		public static void ExtractRecipeFromSelected() {
			if (Selection.activeGameObject == null) {
				Debug.LogWarning("No GameObject selected.");
				return;
			}

			GameObject go = Selection.activeGameObject;

			// Create a new asset
			ComponentRecipe recipe = ScriptableObject.CreateInstance<ComponentRecipe>();
			recipe.defaultName = go.name;

			var components = go.GetComponents<Component>()
				.Where(c => !(c is Transform)) // skip Transform
				.ToArray();

			recipe.components = new ComponentEntry[components.Length];

			for (int i = 0; i < components.Length; i++) {
				var c = components[i];
				var entry = new ComponentEntry();
				entry.typeName = c.GetType().AssemblyQualifiedName;

				// Extract serializable properties
				UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(c);
				var iterator = so.GetIterator();
				var overrides = new System.Collections.Generic.List<string>();

				if (iterator.NextVisible(true)) {
					do {
						// Skip 'm_Script' for MonoBehaviours
						if (iterator.name == "m_Script") continue;

						string valueStr = GetPropertyValueString(iterator);
						if (!string.IsNullOrEmpty(valueStr))
							overrides.Add($"{iterator.name}={valueStr}");
					} while (iterator.NextVisible(false));
				}

				entry.propertyOverrides = overrides.ToArray();
				recipe.components[i] = entry;
			}

			// Optional: extract children recursively
			if (go.transform.childCount > 0) {
				recipe.children = new ComponentRecipe[go.transform.childCount];
				for (int i = 0; i < go.transform.childCount; i++) {
					var child = go.transform.GetChild(i).gameObject;
					recipe.children[i] = ExtractRecipeFromGO(child);
				}
			}

			// Save asset
			string path = EditorUtility.SaveFilePanelInProject("Save ComponentRecipe", go.name + "Recipe", "asset", "Select location for the new recipe asset.");
			if (!string.IsNullOrEmpty(path))
				AssetDatabase.CreateAsset(recipe, path);

			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = recipe;

			Debug.Log($"Recipe extracted: {go.name}");
		}

		private static ComponentRecipe ExtractRecipeFromGO(GameObject go) {
			ComponentRecipe recipe = ScriptableObject.CreateInstance<ComponentRecipe>();
			recipe.defaultName = go.name;

			var components = go.GetComponents<Component>()
				.Where(c => !(c is Transform))
				.ToArray();

			recipe.components = new ComponentEntry[components.Length];

			for (int i = 0; i < components.Length; i++) {
				var c = components[i];
				var entry = new ComponentEntry();
				entry.typeName = c.GetType().AssemblyQualifiedName;

				// Extract serializable properties
				UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(c);
				var iterator = so.GetIterator();
				var overrides = new System.Collections.Generic.List<string>();

				if (iterator.NextVisible(true)) {
					do {
						if (iterator.name == "m_Script") continue;

						string valueStr = GetPropertyValueString(iterator);
						if (!string.IsNullOrEmpty(valueStr))
							overrides.Add($"{iterator.name}={valueStr}");
					} while (iterator.NextVisible(false));
				}

				entry.propertyOverrides = overrides.ToArray();
				recipe.components[i] = entry;
			}

			// Children
			if (go.transform.childCount > 0) {
				recipe.children = new ComponentRecipe[go.transform.childCount];
				for (int i = 0; i < go.transform.childCount; i++) {
					recipe.children[i] = ExtractRecipeFromGO(go.transform.GetChild(i).gameObject);
				}
			}

			return recipe;
		}

		private static string GetPropertyValueString(UnityEditor.SerializedProperty prop) {
			switch (prop.propertyType) {
				case UnityEditor.SerializedPropertyType.Integer: return prop.intValue.ToString();
				case UnityEditor.SerializedPropertyType.Float: return prop.floatValue.ToString();
				case UnityEditor.SerializedPropertyType.Boolean: return prop.boolValue.ToString();
				case UnityEditor.SerializedPropertyType.String: return prop.stringValue;
				case UnityEditor.SerializedPropertyType.Color: return $"#{ColorUtility.ToHtmlStringRGBA(prop.colorValue)}";
				case UnityEditor.SerializedPropertyType.Vector3: return $"{prop.vector3Value.x},{prop.vector3Value.y},{prop.vector3Value.z}";
				case UnityEditor.SerializedPropertyType.Enum: return prop.enumNames[prop.enumValueIndex];
				case UnityEditor.SerializedPropertyType.ObjectReference:
					return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "";
				default: return null;
			}
		}
	}
}