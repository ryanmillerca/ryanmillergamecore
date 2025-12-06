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

			if (characterRefs._animator == null && GUILayout.Button("Auto Assign Animator")) {
				characterRefs._animator = characterRefs.GetComponent<Animator>();
				EditorUtility.SetDirty(characterRefs);
			}
			if (GUILayout.Button("Get References")) {
				AssignComponentIfNull(ref characterRefs._movement, characterRefs);
				AssignComponentIfNull(ref characterRefs._mainCollider, characterRefs);
				AssignComponentIfNull(ref characterRefs._characterInput, characterRefs);
				AssignComponentIfNull(ref characterRefs._characterAnimation, characterRefs);
				AssignComponentIfNull(ref characterRefs._character, characterRefs);
				AssignComponentIfNull(ref characterRefs._characterBrain, characterRefs);
				AssignComponentIfNull(ref characterRefs._playerCharacter, characterRefs);
				AssignComponentIfNull(ref characterRefs._attackColliderSensor, characterRefs);
				AssignComponentIfNull(ref characterRefs._interactColliderSensor, characterRefs);
				AssignComponentIfNull(ref characterRefs._aggroColliderSensor, characterRefs);
				AssignComponentIfNull(ref characterRefs._rb, characterRefs);
				AssignComponentIfNull(ref characterRefs._animator, characterRefs);
				AssignComponentIfNull(ref characterRefs._damageDealer, characterRefs);
				AssignComponentIfNull(ref characterRefs._characterPathfind, characterRefs);
				characterRefs._renderers = characterRefs.gameObject.GetComponentsInChildren<Renderer>();
				EditorUtility.SetDirty(characterRefs);
			}
		}

		private void AssignComponentIfNull<T>(ref T field, CharacterReferences context) where T : Component {
			if (!field) {
				field = context.transform.GetComponentInParent<T>();
				if (!field) {
					var charRoot = context.transform.GetComponentInParent<Character>();
					field = charRoot.GetComponentInChildren<T>();
				}
			}
		}
	}
#endif
}
