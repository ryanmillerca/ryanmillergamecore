namespace RyanMillerGameCore.Editor.Recipes {
	using UnityEngine;
	using Dialog;
	using Interactions;
	using UnityEditor;

	public class DialogRecipe : AbstractRecipe {

		private const string ToolName = "Talk to Dialog";

		[MenuItem(MenuPrefix + ToolName, false, 0)]
		public static void CreateRecipe() {
			GameObject go = new GameObject("Talk To Dialog") {
				transform = {
					position = GetSpawnPosition()
				}
			};

			go.AddComponent<SphereCollider>();
			go.layer = LayerMask.NameToLayer("Interactive");
			PlayDialog playDialog = go.AddComponent<PlayDialog>();
			Interactive interactive = go.AddComponent<Interactive>();
			DisplayButtonPrompt displayButtonPrompt = go.AddComponent<DisplayButtonPrompt>();

			FinishCreation(go, ToolName);
		}
	}
}
