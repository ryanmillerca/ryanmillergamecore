using UnityEngine;
using UnityEditor;

namespace RyanMillerGameCore.Factory {
	[CustomEditor(typeof(ComponentRecipe))]
	public class ComponentRecipeEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector(); // Draw all serialized fields

			ComponentRecipe recipe = (ComponentRecipe)target;

			GUILayout.Space(10);

			if (GUILayout.Button("Generate GameObject in Scene")) {
				GameObject go = recipe.CreateInstanceInScene();
				Undo.RegisterCreatedObjectUndo(go, "Generate GameObject from Recipe");
				Selection.activeGameObject = go; // select the new object
			}
		}
	}
}
