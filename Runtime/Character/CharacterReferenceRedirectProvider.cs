namespace RyanMillerGameCore.Character {
	using UnityEngine;
	using Interactions;
	using RyanMillerGameCore.Animation;

	/// <summary>
	/// Provides reference redirection to character components
	/// </summary>
	public class CharacterReferenceRedirectProvider : MonoBehaviour, ICharacterReferenceProvider {
		[SerializeField] private CharacterReferenceProvider m_ReferenceProvider;

		private void Awake() {
			if (m_ReferenceProvider == null) {
				m_ReferenceProvider = GetComponent<CharacterReferenceProvider>();
			}
			if (m_ReferenceProvider == null) {
				m_ReferenceProvider = GetComponentInParent<CharacterReferenceProvider>();
			}
		}

		// Delegate all interface calls to the actual provider
		public CharacterMovement Movement {
			get => m_ReferenceProvider.Movement;
			set => m_ReferenceProvider.Movement = value;
		}

		public Collider MainCollider {
			get => m_ReferenceProvider.MainCollider;
			set => m_ReferenceProvider.MainCollider = value;
		}

		public CharacterInput CharacterInput {
			get => m_ReferenceProvider.CharacterInput;
			set => m_ReferenceProvider.CharacterInput = value;
		}

		public CharacterAnimation CharacterAnimation {
			get => m_ReferenceProvider.CharacterAnimation;
			set => m_ReferenceProvider.CharacterAnimation = value;
		}

		public ICharacter Character {
			get => m_ReferenceProvider.Character;
			set => m_ReferenceProvider.Character = value;
		}

		public CharacterBrain CharacterBrain {
			get => m_ReferenceProvider.CharacterBrain;
			set => m_ReferenceProvider.CharacterBrain = value;
		}

		public PlayerCharacter PlayerCharacter {
			get => m_ReferenceProvider.PlayerCharacter;
			set => m_ReferenceProvider.PlayerCharacter = value;
		}

		public ColliderSensor AttackColliderSensor {
			get => m_ReferenceProvider.AttackColliderSensor;
			set => m_ReferenceProvider.AttackColliderSensor = value;
		}

		public ColliderSensor AggroColliderSensor {
			get => m_ReferenceProvider.AggroColliderSensor;
			set => m_ReferenceProvider.AggroColliderSensor = value;
		}

		public InteractiveObjectColliderSensor InteractColliderSensor {
			get => m_ReferenceProvider.InteractColliderSensor;
			set => m_ReferenceProvider.InteractColliderSensor = value;
		}

		public Rigidbody Rb {
			get => m_ReferenceProvider.Rb;
			set => m_ReferenceProvider.Rb = value;
		}

		public DamageDealer DamageDealer {
			get => m_ReferenceProvider.DamageDealer;
			set => m_ReferenceProvider.DamageDealer = value;
		}

		public Animator Animator {
			get => m_ReferenceProvider.Animator;
			set => m_ReferenceProvider.Animator = value;
		}

		public Renderer[] Renderers {
			get => m_ReferenceProvider.Renderers;
			set => m_ReferenceProvider.Renderers = value;
		}

		public CharacterPathfind CharacterPathfind {
			get => m_ReferenceProvider.CharacterPathfind;
			set => m_ReferenceProvider.CharacterPathfind = value;
		}

		public ColliderSensor GetColliderSensor(ColliderSensorType sensorType) {
			return m_ReferenceProvider.GetColliderSensor(sensorType);
		}

		public CharacterAnimParamMappings CharacterAnimParamMappings {
			get => m_ReferenceProvider.CharacterAnimParamMappings;
			set => m_ReferenceProvider.CharacterAnimParamMappings = value;
		}
	}
}
