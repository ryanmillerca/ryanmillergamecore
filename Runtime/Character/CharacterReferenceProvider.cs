namespace RyanMillerGameCore.Character {
	using UnityEngine;
	using RyanMillerGameCore.Animation;
	using Interactions;
	using SMB;
	using System.Collections.Generic;

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
		private Dictionary<ColliderSensorType, ColliderSensor> m_sensorCache;

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

			// Initialize sensor cache
			m_sensorCache = new Dictionary<ColliderSensorType, ColliderSensor>();

			m_isInitialized = true;
		}

		/// <summary>
		/// Get component with lazy initialization and error handling
		/// </summary>
		public T Get<T>(System.Func<T> getter) {
			Initialize();
			return getter();
		}

		/// <summary>
		/// Gets the character component with proper type checking
		/// </summary>
		public ICharacter Character {
			get {
				if (m_Character == null) return null;

				// Try to cast to ICharacter
				if (m_Character is ICharacter character) {
					return character;
				}

				// If component doesn't implement ICharacter, try to find one in children
				var characterComponent = m_Character.GetComponent<ICharacter>();
				return characterComponent;
			}
			set {
				// Validate that the value is a Component
				if (value == null) {
					m_Character = null;
					return;
				}

				// Ensure it's actually a Component (MonoBehaviour, ScriptableObject, etc.)
				if (value is Component component) {
					m_Character = component;
				}
				else {
					Debug.LogError($"Cannot assign {value.GetType()} to Character property. Value must be a Component.");
				}
			}
		}

		// All properties with proper null checks and initialization
		public CharacterMovement Movement {
			get => Get(() => m_Movement ?? GetComponent<CharacterMovement>());
			set => m_Movement = value;
		}

		public Collider MainCollider {
			get => Get(() => m_MainCollider ?? GetComponent<Collider>());
			set => m_MainCollider = value;
		}

		public CharacterInput CharacterInput {
			get => Get(() => m_CharacterInput ?? GetComponent<CharacterInput>());
			set => m_CharacterInput = value;
		}

		public CharacterAnimation CharacterAnimation {
			get => Get(() => m_CharacterAnimation ?? GetComponent<CharacterAnimation>());
			set => m_CharacterAnimation = value;
		}

		public CharacterBrain CharacterBrain {
			get => Get(() => m_CharacterBrain ?? GetComponent<CharacterBrain>());
			set => m_CharacterBrain = value;
		}

		public PlayerCharacter PlayerCharacter {
			get => Get(() => m_PlayerCharacter ?? GetComponent<PlayerCharacter>());
			set => m_PlayerCharacter = value;
		}

		public ColliderSensor AttackColliderSensor {
			get => Get(() => m_AttackColliderSensor ?? GetComponent<ColliderSensor>());
			set => m_AttackColliderSensor = value;
		}

		public ColliderSensor AggroColliderSensor {
			get => Get(() => m_AggroColliderSensor ?? GetComponent<ColliderSensor>());
			set => m_AggroColliderSensor = value;
		}

		public InteractiveObjectColliderSensor InteractColliderSensor {
			get => Get(() => m_InteractColliderSensor ?? GetComponent<InteractiveObjectColliderSensor>());
			set => m_InteractColliderSensor = value;
		}

		public Rigidbody Rb {
			get => Get(() => m_Rb ?? GetComponent<Rigidbody>());
			set => m_Rb = value;
		}

		public DamageDealer DamageDealer {
			get => Get(() => m_DamageDealer ?? GetComponent<DamageDealer>());
			set => m_DamageDealer = value;
		}

		public Animator Animator {
			get => Get(() => m_Animator ?? GetComponent<Animator>());
			set => m_Animator = value;
		}

		public Renderer[] Renderers {
			get => m_Renderers ?? new Renderer[0];
			set => m_Renderers = value;
		}

		public CharacterPathfind CharacterPathfind {
			get => Get(() => m_CharacterPathfind ?? GetComponent<CharacterPathfind>());
			set => m_CharacterPathfind = value;
		}

		public CharacterAnimParamMappings CharacterAnimParamMappings {
			get => m_CharacterAnimParamMappings;
			set => m_CharacterAnimParamMappings = value;
		}

		/// <summary>
		/// Type-safe sensor access with caching for performance
		/// </summary>
		public ColliderSensor GetColliderSensor(ColliderSensorType sensorType) {
			Initialize();

			if (m_sensorCache.TryGetValue(sensorType, out ColliderSensor cachedSensor)) {
				return cachedSensor;
			}

			ColliderSensor sensor = sensorType switch {
				ColliderSensorType.Attack => m_AttackColliderSensor ?? GetComponent<ColliderSensor>(),
				ColliderSensorType.Aggro => m_AggroColliderSensor ?? GetComponent<ColliderSensor>(),
				ColliderSensorType.Interact => m_InteractColliderSensor ?? GetComponent<InteractiveObjectColliderSensor>(),
				_ => null
			};

			m_sensorCache[sensorType] = sensor;
			return sensor;
		}

		/// <summary>
		/// Get all components that implement a specific interface
		/// </summary>
		public T[] GetAllComponents<T>() where T : Component {
			Initialize();
			return Get(() => GetComponentsInChildren<T>(true));
		}

		/// <summary>
		/// Get a component with fallback to child search
		/// </summary>
		public T GetComponentWithFallback<T>(T defaultComponent = null) where T : Component {
			Initialize();
			return Get(() => defaultComponent ?? GetComponent<T>() ?? GetComponentInChildren<T>());
		}

		private void OnDestroy() {
			if (m_Animator != null) {
				m_Animator.ClearComponentReference();
			}

			// Clear cache to prevent memory leaks
			m_sensorCache?.Clear();
		}

		/// <summary>
		/// Validate that all required components are present
		/// </summary>
		public bool ValidateComponents() {
			Initialize();

			var errors = new List<string>();

			if (m_Animator == null) {
				errors.Add("Animator is missing");
			}

			if (m_Rb == null) {
				errors.Add("Rigidbody is missing");
			}

			if (m_MainCollider == null) {
				errors.Add("Main Collider is missing");
			}

			if (errors.Count > 0) {
				Debug.LogError($"CharacterReferenceProvider validation failed:\n{string.Join("\n", errors)}");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reset all component references to null
		/// </summary>
		public void ClearReferences() {
			m_Movement = null;
			m_MainCollider = null;
			m_CharacterInput = null;
			m_CharacterAnimation = null;
			m_CharacterBrain = null;
			m_PlayerCharacter = null;
			m_AttackColliderSensor = null;
			m_AggroColliderSensor = null;
			m_InteractColliderSensor = null;
			m_Rb = null;
			m_DamageDealer = null;
			m_Animator = null;
			m_Renderers = null;
			m_CharacterPathfind = null;
			m_CharacterAnimParamMappings = null;
		}

		/// <summary>
		/// Force re-initialization
		/// </summary>
		public void Reinitialize() {
			m_isInitialized = false;
			Initialize();
		}
	}

	public enum ColliderSensorType {
		Attack,
		Aggro,
		Interact
	}
}
