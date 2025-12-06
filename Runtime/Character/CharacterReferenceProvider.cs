namespace RyanMillerGameCore.Character {
	using UnityEngine;
	using Animation;
	using Interactions;
	using SMB;

	/// <summary>
	/// Provides controlled access to character components with lazy initialization
	/// </summary>
	public class CharacterReferenceProvider : MonoBehaviour, ICharacterReferenceProvider {
		[Header("Component References")]
		[SerializeField] private CharacterMovement m_Movement;
		[SerializeField] private Collider m_MainCollider;
		[SerializeField] private CharacterInput m_CharacterInput;
		[SerializeField] private CharacterAnimation m_CharacterAnimation;
		[SerializeField] private Component m_Character;
		[SerializeField] private CharacterBrain m_CharacterBrain;
		[SerializeField] private PlayerCharacter m_PlayerCharacter;
		[SerializeField] private ColliderSensor m_AttackColliderSensor;
		[SerializeField] private ColliderSensor m_AggroColliderSensor;
		[SerializeField] private InteractiveObjectColliderSensor m_InteractColliderSensor;
		[SerializeField] private Rigidbody m_Rb;
		[SerializeField] private DamageDealer m_DamageDealer;
		[SerializeField] private Animator m_Animator;
		[SerializeField] private Renderer[] m_Renderers;
		[SerializeField] private CharacterPathfind m_CharacterPathfind;
		[SerializeField] private CharacterAnimParamMappings m_CharacterAnimParamMappings;

		[Header("Stats")]
		public float m_AttackDashForce = 500;

		private bool m_isInitialized = false;

		private void Awake() {
			Initialize();
		}

		private void Initialize() {
			if (m_isInitialized) return;

			// Ensure animator is set up
			if (m_Animator == null) {
				m_Animator = GetComponent<Animator>();
			}

			if (m_Animator != null) {
				m_Animator.SetComponentReference(this);
			}

			m_isInitialized = true;
		}

		// Simple redirect method - minimal boilerplate
		public T Get<T>(System.Func<T> getter) where T : Component {
			Initialize();
			return getter();
		}

		// All properties now have getters and setters
		public CharacterMovement Movement {
			get => m_Movement;
			set => m_Movement = value;
		}

		public Collider MainCollider {
			get => m_MainCollider;
			set => m_MainCollider = value;
		}

		public CharacterInput CharacterInput {
			get => m_CharacterInput;
			set => m_CharacterInput = value;
		}

		public CharacterAnimation CharacterAnimation {
			get => m_CharacterAnimation;
			set => m_CharacterAnimation = value;
		}

		public ICharacter Character {
			get => (ICharacter)m_Character;
			set => m_Character = (Component)value;
		}

		public CharacterBrain CharacterBrain {
			get => m_CharacterBrain;
			set => m_CharacterBrain = value;
		}

		public PlayerCharacter PlayerCharacter {
			get => m_PlayerCharacter;
			set => m_PlayerCharacter = value;
		}

		public ColliderSensor AttackColliderSensor {
			get => m_AttackColliderSensor;
			set => m_AttackColliderSensor = value;
		}

		public ColliderSensor AggroColliderSensor {
			get => m_AggroColliderSensor;
			set => m_AggroColliderSensor = value;
		}

		public InteractiveObjectColliderSensor InteractColliderSensor {
			get => m_InteractColliderSensor;
			set => m_InteractColliderSensor = value;
		}

		public Rigidbody Rb {
			get => m_Rb;
			set => m_Rb = value;
		}

		public DamageDealer DamageDealer {
			get => m_DamageDealer;
			set => m_DamageDealer = value;
		}

		public Animator Animator {
			get => m_Animator;
			set => m_Animator = value;
		}

		public Renderer[] Renderers {
			get => m_Renderers;
			set => m_Renderers = value;
		}

		public CharacterPathfind CharacterPathfind {
			get => m_CharacterPathfind;
			set => m_CharacterPathfind = value;
		}

		// Type-safe sensor access
		public ColliderSensor GetColliderSensor(ColliderSensorType sensorType) {
			return sensorType switch {
				ColliderSensorType.Attack => m_AttackColliderSensor,
				ColliderSensorType.Aggro => m_AggroColliderSensor,
				ColliderSensorType.Interact => m_InteractColliderSensor,
				_ => null
			};
		}

		public CharacterAnimParamMappings CharacterAnimParamMappings {
			get => m_CharacterAnimParamMappings;
			set => m_CharacterAnimParamMappings = value;
		}

		private void OnDestroy() {
			if (m_Animator != null) {
				m_Animator.ClearComponentReference();
			}
		}
	}

	public enum ColliderSensorType {
		Attack,
		Aggro,
		Interact
	}
}
