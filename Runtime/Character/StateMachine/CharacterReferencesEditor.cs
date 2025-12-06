namespace RyanMillerGameCore.Character.SMB {
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine;
	using Animation;
	using Interactions;

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
				characterRefs.Character = characterRefs.GetComponentInParent<ICharacter>();
				Transform root = characterRefs.Character.Transform;
				characterRefs.Movement = root.GetComponentInChildren<CharacterMovement>();
				characterRefs.MainCollider = root.GetComponent<Collider>();
				characterRefs.CharacterInput = root.GetComponent<CharacterInput>();
				characterRefs.CharacterAnimation = root.GetComponent<CharacterAnimation>();
				characterRefs.CharacterBrain = root.GetComponent<CharacterBrain>();
				characterRefs.PlayerCharacter = root.GetComponent<PlayerCharacter>();
				characterRefs.Rb = root.GetComponent<Rigidbody>();
				characterRefs.Animator = root.GetComponentInChildren<Animator>();
				characterRefs.DamageDealer = root.GetComponentInChildren<DamageDealer>();
				characterRefs.CharacterPathfind = root.GetComponent<CharacterPathfind>();
				characterRefs.Renderers = characterRefs.gameObject.GetComponentsInChildren<Renderer>();
				characterRefs.InteractColliderSensor = root.GetComponentInChildren<InteractiveObjectColliderSensor>();
				var colliderSensors = root.GetComponentsInChildren<Collider>();
				foreach (var collider in colliderSensors) {
					if (collider.name.ToLowerInvariant().Contains("aggro")) {
						characterRefs.AggroColliderSensor = collider.GetComponent<ColliderSensor>();
					}
					else if (collider.name.ToLowerInvariant().Contains("attack")) {
						characterRefs.AttackColliderSensor = collider.GetComponent<ColliderSensor>();
					}
				}
				EditorUtility.SetDirty(characterRefs);
			}
		}
	}
#endif
}
