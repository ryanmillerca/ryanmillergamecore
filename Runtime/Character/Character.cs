using UnityEngine.Serialization;
namespace RyanMillerGameCore.Character {
	using UnityEngine;
	using System;
	using UnityEngine.Events;
	using SaveSystem;
	using Camera;
	using Interactions;
	using Utilities;

	/// <summary>
	/// Represents a general character in the game, managing health, damage, player status, and related events.
	/// </summary>
	[RequireComponent(typeof(AutoRegisterID))]
	public class Character : MonoBehaviour, ICharacter, ITakesDamage, IHasID {


		#region Events

		[Foldout("Unity Events"), SerializeField]
		private UnityEvent<float> m_ReceivedDamage;
		[Foldout("Unity Events"), SerializeField]
		private UnityEvent<float> m_ReceivedHeal;
		[Foldout("Unity Events"), SerializeField]
		private UnityEvent m_KnockedBack;
		[Foldout("Unity Events"), SerializeField]
		private UnityEvent m_Died;

		public event Action<float> OnReceiveDamage;
		public event Action<float> OnReceiveHeal;
		public event Action<Character> OnDied;
		public event Action<Vector3> OnKnockedBack;
		public event Action Died;
		public event Action Spawned;

		#endregion


		#region Public Properties

		public CharacterID identifier {
			get {
				if (m_Stats != null) {
					return m_Stats.m_Identifier;
				}
				return m_Identifier;
			}
		}

		public float maxHealth {
			get {
				if (m_Stats) {
					return m_Stats.m_MaxHealth;
				}
				return m_MaxHealth;
			}
			set {
				if (m_Stats) {
					m_Stats.m_MaxHealth = value;
				}
				m_MaxHealth = value;
			}
		}

		public float currentHealth {
			get {
				if (m_Stats) {
					return m_Stats.m_CurrentHealth;
				}
				return m_CurrentHealth;
			}
			set {
				if (m_Stats) {
					m_Stats.m_CurrentHealth = value;
				}
				m_CurrentHealth = value;
			}
		}

		public float damageCooldown {
			get {
				if (m_Stats != null) {
					return m_Stats.m_DamageCooldown;
				}
				return m_DamageCooldown;
			}
		}

		public Transform Transform {
			get {
				return transform;
			}
		}

		public CharacterReferenceProvider referenceProvider {
			get {
				if (m_characterReferenceProvider == null) {
					m_characterReferenceProvider = GetComponentInChildren<CharacterReferenceProvider>();
				}
				return m_characterReferenceProvider;
			}
		}

		public CharacterMovement characterMovement {
			get {
				if (!m_characterMovement) {
					m_characterMovement = GetComponent<CharacterMovement>();
				}
				return m_characterMovement;
			}
		}

		public CharacterBrain brain {
			get {
				if (!m_characterBrain) {
					m_characterBrain = GetComponent<CharacterBrain>();
				}
				return m_characterBrain;
			}
		}

		public float percentHealth {
			get { return currentHealth / maxHealth; }
		}

		#endregion


		#region Stats If No ScriptableObject

		[Foldout("Stats (if no ScriptableObject)"), SerializeField]
		private CharacterID m_Identifier;
		[Foldout("Stats (if no ScriptableObject)"), SerializeField]
		private float m_MaxHealth = 10;
		[Foldout("Stats (if no ScriptableObject)"), SerializeField]
		private float m_CurrentHealth = 10;
		[Foldout("Stats (if no ScriptableObject)"), SerializeField]
		private float m_DamageCooldown = 1f;

		#endregion


		#region Serialized Fields

		[SerializeField] private CharacterStats m_Stats;
		[SerializeField] private bool m_IsPlayer;
		[SerializeField] private bool m_SpawnAtCheckpoint = false;

		#endregion


		#region Private Fields

		private CharacterReferenceProvider m_characterReferenceProvider;
		private CharacterMovement m_characterMovement;
		private CharacterBrain m_characterBrain;
		private static bool _IsQuitting = false;
		private float m_lastDamageTime = -Mathf.Infinity;

		#endregion


		#region Public Methods

		public ID GetID() {
			return identifier;
		}
		public bool IsPlayer() {
			return m_IsPlayer;
		}

		public CharacterID ID() {
			return m_Identifier;
		}

		public bool Respawn() {
			if (currentHealth > 0) {
				return false;
			}
			GoToCheckpoint();
			Reset();
			Spawned?.Invoke();
			return true;
		}

		public void GoToCheckpoint(CheckpointData checkpointData = null) {
			if (!DataManager.Instance) {
				Debug.LogWarning("GoToCheckpoint failed: No DataManager in scene");
				return;
			}
			checkpointData ??= DataManager.Instance.TryGetCheckpoint();
			if (checkpointData == null) {
				return;
			}
			characterMovement.Teleport(checkpointData.position, checkpointData.rotation);
			if (!CameraController.Instance) {
				Debug.LogWarning("GoToCheckpoint issue: No Camera Controller in scene");
				return;
			}
			CameraController.Instance.TargetYRotation = checkpointData.cameraRotation;
			CameraController.Instance.transform.position = checkpointData.position;
		}


		public void Reset() {
			currentHealth = maxHealth;
		}

		public void Interact(IInteractive interactive) {
			interactive.Interact(this);
		}

		public bool CanReceiveDamage() {
			// cannot, in damage cooldown period / iframes
			if (Time.time < m_lastDamageTime + damageCooldown) {
				return false;
			}
			// cannot, if dead
			if (currentHealth <= 0) {
				return false;
			}
			// well we must be alive, then
			m_lastDamageTime = Time.time;
			return true;
		}

		public bool CanReceiveHealing() {
			return currentHealth < maxHealth;
		}

		public bool ReceiveDamage(float damageAmount, Component attacker = null) {
			if (damageAmount == 0 || currentHealth <= 0) {
				return false;
			}

			currentHealth -= damageAmount;
			OnReceiveDamage?.Invoke(damageAmount);
			m_ReceivedDamage?.Invoke(damageAmount);
			if (currentHealth <= 0) {
				brain.GoToDead();
				OnDied?.Invoke(this);
				Died?.Invoke();
				m_Died?.Invoke();
			}
			else {
				brain.GoToHurt();
			}

			return true;
		}

		public bool ReceiveHeal(float healAmount, bool overHeal = false) {
			if (healAmount == 0) {
				return false;
			}
			currentHealth += healAmount;
			if (overHeal == false) {
				currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
			}
			OnReceiveHeal?.Invoke(healAmount);
			m_ReceivedHeal?.Invoke(healAmount);
			return true;
		}

		public bool ReceiveKnockback(Vector3 direction) {
			if (direction.magnitude <= 0.01f) {
				return false;
			}
			OnKnockedBack?.Invoke(direction);
			m_KnockedBack?.Invoke();
			return true;
		}

		public void GetKnockback(Vector3 direction) {
			ReceiveKnockback(direction);
		}

		#endregion


		#region Private Methods

		private void Awake() {
			m_CurrentHealth = m_MaxHealth;
		}

		private void Start() {
			RegisterToCharacterManager();
			if (m_SpawnAtCheckpoint) {
				Respawn();
			}
			else {
				Spawned?.Invoke();
			}
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Init() {
			_IsQuitting = false;
		}

		private void OnApplicationQuit() {
			_IsQuitting = true;
		}

		private void OnDestroy() {
			if (!_IsQuitting && Application.isPlaying) {
				if (CharacterManager.Instance) {
					CharacterManager.Instance.RemoveCharacter(this);
				}
			}
		}

		private void RegisterToCharacterManager() {
			if (CharacterManager.Instance == null) {
				return;
			}
			CharacterManager.Instance.RegisterCharacter(this);
		}

		#endregion


	}
}
