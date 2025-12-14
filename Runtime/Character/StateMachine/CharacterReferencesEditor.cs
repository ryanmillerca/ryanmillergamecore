namespace RyanMillerGameCore.Character {
#if UNITY_EDITOR
	using UnityEditor;
	using UnityEngine;
	using RyanMillerGameCore.Animation;
	using Interactions;

	/// <summary>
	/// Custom editor for CharacterReferences component.
	/// </summary>
	[CustomEditor(typeof(CharacterReferenceProvider))]
	public class CharacterReferencesEditor : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			var characterRefs = (CharacterReferenceProvider)target;

			if (GUILayout.Button("Get References")) {
				Undo.RecordObject(characterRefs, "Get References");
				Transform charTransform = characterRefs.Character.Transform;

				// ROOT components
				characterRefs.Character = characterRefs.GetComponent<ICharacter>();
				characterRefs.Movement = characterRefs.GetComponent<CharacterMovement>();
				characterRefs.CharacterInput = characterRefs.GetComponent<CharacterInput>();
				characterRefs.CharacterAnimation = characterRefs.GetComponent<CharacterAnimation>();
				characterRefs.CharacterBrain = characterRefs.GetComponent<CharacterBrain>();
				characterRefs.PlayerCharacter = characterRefs.GetComponent<PlayerCharacter>();
				characterRefs.Rb = characterRefs.GetComponent<Rigidbody>();
				characterRefs.CharacterPathfind = characterRefs.GetComponent<CharacterPathfind>();

				// CHILD components
				characterRefs.Animator = characterRefs.GetComponentInChildren<Animator>();
				characterRefs.DamageDealer = characterRefs.GetComponentInChildren<DamageDealer>();
				characterRefs.Renderers = characterRefs.gameObject.GetComponentsInChildren<Renderer>();
				characterRefs.InteractColliderSensor = characterRefs.GetComponentInChildren<InteractiveObjectColliderSensor>();

				// ROOT colliders (Main Collider)
				var colliderSensors = characterRefs.GetComponentsInChildren<Collider>();
				foreach (var collider in colliderSensors) {
					if (!collider.isTrigger) {
						characterRefs.MainCollider = characterRefs.GetComponent<Collider>();
					}
				}

				// CHILD colliders (Sensors)
				var childColliders = characterRefs.GetComponentsInChildren<Collider>();
				foreach (var collider in childColliders) {
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
