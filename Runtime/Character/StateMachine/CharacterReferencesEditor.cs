namespace RyanMillerGameCore.Character.SMB {
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Custom editor for CharacterReferences component.
	/// </summary>
	[CustomEditor(typeof(CharacterReferenceProvider))]
	public class CharacterReferencesEditor : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			var characterRefs = (CharacterReferenceProvider)target;

			if (!characterRefs.Animator && GUILayout.Button("Auto Assign Animator")) {
				characterRefs.Animator = characterRefs.GetComponent<Animator>();
				EditorUtility.SetDirty(characterRefs);
			}
			if (GUILayout.Button("Get References")) {
				characterRefs.Movement = AssignComponentIfNull(characterRefs.Movement, characterRefs);
				characterRefs.MainCollider = AssignComponentIfNull(characterRefs.MainCollider, characterRefs);
				characterRefs.CharacterInput = AssignComponentIfNull(characterRefs.CharacterInput, characterRefs);
				characterRefs.CharacterAnimation = AssignComponentIfNull(characterRefs.CharacterAnimation, characterRefs);
				characterRefs.Character = AssignComponentIfNull(characterRefs.Character, characterRefs);
				characterRefs.CharacterBrain = AssignComponentIfNull(characterRefs.CharacterBrain, characterRefs);
				characterRefs.PlayerCharacter = AssignComponentIfNull(characterRefs.PlayerCharacter, characterRefs);
				characterRefs.AttackColliderSensor = AssignComponentIfNull(characterRefs.AttackColliderSensor, characterRefs);
				characterRefs.InteractColliderSensor = AssignComponentIfNull(characterRefs.InteractColliderSensor, characterRefs);
				characterRefs.AggroColliderSensor = AssignComponentIfNull(characterRefs.AggroColliderSensor, characterRefs);
				characterRefs.Rb = AssignComponentIfNull(characterRefs.Rb, characterRefs);
				characterRefs.Animator = AssignComponentIfNull(characterRefs.Animator, characterRefs);
				characterRefs.DamageDealer = AssignComponentIfNull(characterRefs.DamageDealer, characterRefs);
				characterRefs.CharacterPathfind = AssignComponentIfNull(characterRefs.CharacterPathfind, characterRefs);
				characterRefs.Renderers = characterRefs.gameObject.GetComponentsInChildren<Renderer>();
				EditorUtility.SetDirty(characterRefs);
			}
		}

		private T AssignComponentIfNull<T>(T field, CharacterReferenceProvider context) where T : Component {
			if (!field) {
				field = context.transform.GetComponentInParent<T>();
				if (!field) {
					var charRoot = (Component)context.transform.GetComponentInParent<ICharacter>();
					field = charRoot?.GetComponentInChildren<T>();
				}
			}

			return field;
		}
	}
#endif
}
