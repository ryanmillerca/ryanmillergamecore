namespace RyanMillerGameCore.Editor.Recipes {
	using UnityEngine;
	using Dialog;
	using Interactions;
	using UnityEditor;
	using UI;

	public class DialogRecipe : AbstractRecipe {

		private const string ToolName = "Talk to Dialog";

		[MenuItem(MenuPrefix + ToolName, false, 0)]
		public static void CreateRecipe() {
			GameObject go = CreateGameObject(ToolName);

			go.AddComponent<SphereCollider>();
			go.layer = LayerMask.NameToLayer("Interactive");
			PlayDialog playDialog = go.AddComponent<PlayDialog>();
			playDialog.Setup(TriggerDialogOn.Interact, false, false);
			Interactive interactive = go.AddComponent<Interactive>();
			DisplayButtonPrompt displayButtonPrompt = go.AddComponent<DisplayButtonPrompt>();
			displayButtonPrompt.Setup(PromptAction.Interact);

			FinishCreation(go, ToolName);
		}
	}
}
