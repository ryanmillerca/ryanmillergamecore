namespace RyanMillerGameCore.Character.SMB {
	using UnityEngine;
	using Animation;
	using Interactions;

	/// <summary>
	/// Provides controlled access to character components with lazy initialization
	/// </summary>
	public class CharacterReferenceProvider : MonoBehaviour, ICharacterReferenceProvider {
		[Header("Component References")]
		[SerializeField] private CharacterMovement _movement;
		[SerializeField] private Collider _mainCollider;
		[SerializeField] private CharacterInput _characterInput;
		[SerializeField] private CharacterAnimation _characterAnimation;
		[SerializeField] private Character _character;
		[SerializeField] private CharacterBrain _characterBrain;
		[SerializeField] private PlayerCharacter _playerCharacter;
		[SerializeField] private ColliderSensor _attackColliderSensor;
		[SerializeField] private ColliderSensor _aggroColliderSensor;
		[SerializeField] private InteractiveObjectColliderSensor _interactColliderSensor;
		[SerializeField] private Rigidbody m_Rb;
		[SerializeField] private DamageDealer _damageDealer;
		[SerializeField] private Animator _animator;
		[SerializeField] private Renderer[] _renderers;
		[SerializeField] private CharacterPathfind _characterPathfind;
		[SerializeField] private CharacterAnimParamMappings _characterAnimParamMappings;

		[Header("Stats")]
		public float attackDashForce = 500;

		private bool _isInitialized = false;

		private void Awake() {
			Initialize();
		}

		private void Initialize() {
			if (_isInitialized) return;

			// Ensure animator is set up
			if (_animator == null) {
				_animator = GetComponent<Animator>();
			}

			if (_animator != null) {
				_animator.SetComponentReference(this);
			}

			_isInitialized = true;
		}

		// Simple redirect method - minimal boilerplate
		public T Get<T>(System.Func<T> getter) where T : Component {
			Initialize();
			return getter();
		}

		// All properties now have getters and setters
		public CharacterMovement Movement {
			get => _movement;
			set => _movement = value;
		}

		public Collider MainCollider {
			get => _mainCollider;
			set => _mainCollider = value;
		}

		public CharacterInput CharacterInput {
			get => _characterInput;
			set => _characterInput = value;
		}

		public CharacterAnimation CharacterAnimation {
			get => _characterAnimation;
			set => _characterAnimation = value;
		}

		public Character Character {
			get => _character;
			set => _character = value;
		}

		public CharacterBrain CharacterBrain {
			get => _characterBrain;
			set => _characterBrain = value;
		}

		public PlayerCharacter PlayerCharacter {
			get => _playerCharacter;
			set => _playerCharacter = value;
		}

		public ColliderSensor AttackColliderSensor {
			get => _attackColliderSensor;
			set => _attackColliderSensor = value;
		}

		public ColliderSensor AggroColliderSensor {
			get => _aggroColliderSensor;
			set => _aggroColliderSensor = value;
		}

		public InteractiveObjectColliderSensor InteractColliderSensor {
			get => _interactColliderSensor;
			set => _interactColliderSensor = value;
		}

		public Rigidbody Rb {
			get => m_Rb;
			set => m_Rb = value;
		}

		public DamageDealer DamageDealer {
			get => _damageDealer;
			set => _damageDealer = value;
		}

		public Animator Animator {
			get => _animator;
			set => _animator = value;
		}

		public Renderer[] Renderers {
			get => _renderers;
			set => _renderers = value;
		}

		public CharacterPathfind CharacterPathfind {
			get => _characterPathfind;
			set => _characterPathfind = value;
		}

		// Type-safe sensor access
		public ColliderSensor GetColliderSensor(ColliderSensorType sensorType) {
			return sensorType switch {
				ColliderSensorType.Attack => _attackColliderSensor,
				ColliderSensorType.Aggro => _aggroColliderSensor,
				ColliderSensorType.Interact => _interactColliderSensor,
				_ => null
			};
		}

		public CharacterAnimParamMappings CharacterAnimParamMappings {
			get => _characterAnimParamMappings;
			set => _characterAnimParamMappings = value;
		}

		private void OnDestroy() {
			if (_animator != null) {
				_animator.ClearComponentReference();
			}
		}
	}

	public enum ColliderSensorType {
		Attack,
		Aggro,
		Interact
	}
}
