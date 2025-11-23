namespace RyanMillerGameCore.Character.SMB {
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Custom editor for CharacterReferences component.
	/// </summary>
	[CustomEditor(typeof(CharacterReferences))]
	public class CharacterReferencesEditor : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			var characterRefs = (CharacterReferences)target;

			if (characterRefs.animator == null && GUILayout.Button("Auto Assign Animator")) {
				characterRefs.animator = characterRefs.GetComponent<Animator>();
				EditorUtility.SetDirty(characterRefs);
			}
			if (GUILayout.Button("Get References")) {
				AssignComponentIfNull(ref characterRefs.movement, characterRefs);
				AssignComponentIfNull(ref characterRefs.mainCollider, characterRefs);
				AssignComponentIfNull(ref characterRefs.characterInput, characterRefs);
				AssignComponentIfNull(ref characterRefs.characterAnimation, characterRefs);
				AssignComponentIfNull(ref characterRefs.character, characterRefs);
				AssignComponentIfNull(ref characterRefs.characterBrain, characterRefs);
				AssignComponentIfNull(ref characterRefs.playerCharacter, characterRefs);
				AssignComponentIfNull(ref characterRefs.attackColliderSensor, characterRefs);
				AssignComponentIfNull(ref characterRefs.interactColliderSensor, characterRefs);
				AssignComponentIfNull(ref characterRefs.aggroColliderSensor, characterRefs);
				AssignComponentIfNull(ref characterRefs.rb, characterRefs);
				AssignComponentIfNull(ref characterRefs.animator, characterRefs);
				AssignComponentIfNull(ref characterRefs.damageDealer, characterRefs);
				AssignComponentIfNull(ref characterRefs.characterPathfind, characterRefs);
				characterRefs.renderers = characterRefs.gameObject.GetComponentsInChildren<Renderer>();
				EditorUtility.SetDirty(characterRefs);
			}
		}

		private void AssignComponentIfNull<T>(ref T field, CharacterReferences context) where T : Component {
			if (!field) {
				field = context.transform.root.GetComponent<T>();
				if (!field) {
					var charRoot = context.transform.GetComponentInParent<Character>();
					field = charRoot.GetComponentInChildren<T>();
				}
			}
		}
	}
#endif
}
