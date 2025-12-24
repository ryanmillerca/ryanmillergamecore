namespace RyanMillerGameCore.Character {
	using UnityEngine;

	public class AggressiveMeleeBrain : CharacterBrain {
		private BrainSwitcher _brainSwitcher;

		protected override void Awake() {
			base.Awake();
		}

		protected override void OnEnable() {
			if (_brainSwitcher == null) {
				_brainSwitcher = GetComponent<BrainSwitcher>();
			}

			base.OnEnable();

			// subscribe to sensors
			if (m_ICharacterReferenceProvider == null) {
				Debug.LogWarning("CharacterReferences is null", gameObject);
				return;
			}
			if (m_ICharacterReferenceProvider.AggroColliderSensor == null) {
				Debug.LogWarning("characterReferences.aggroColliderSensor is null", gameObject);
				return;
			}
			m_ICharacterReferenceProvider.AggroColliderSensor.ObjectEnteredSensor += OnObjectEnteredAggroColliderSensor;
			m_ICharacterReferenceProvider.AggroColliderSensor.ObjectExitedSensor += OnObjectExitedAggroColliderSensor;
			m_ICharacterReferenceProvider.AttackColliderSensor.ObjectEnteredSensor += OnObjectEnteredAttackColliderSensor;
			m_ICharacterReferenceProvider.AttackColliderSensor.ObjectExitedSensor += OnObjectExitedAttackColliderSensor;

			// Check if the aggro sensor has any objects in it at the start
			Collider c = m_ICharacterReferenceProvider.AggroColliderSensor.GetFirstItemInSensor();
			if (c) {
				OnObjectEnteredAggroColliderSensor(c);
			}

			// Check if the attack sensor has any objects in it at the start
			c = m_ICharacterReferenceProvider.AttackColliderSensor.GetFirstItemInSensor();
			if (c) {
				OnObjectEnteredAttackColliderSensor(c);
			}
		}

		protected override void OnDisable() {
			base.OnDisable();

			// unsubscribe from sensors
			m_ICharacterReferenceProvider.AggroColliderSensor.ObjectEnteredSensor -= OnObjectEnteredAggroColliderSensor;
			m_ICharacterReferenceProvider.AggroColliderSensor.ObjectExitedSensor -= OnObjectExitedAggroColliderSensor;
			m_ICharacterReferenceProvider.AttackColliderSensor.ObjectEnteredSensor -= OnObjectEnteredAttackColliderSensor;
			m_ICharacterReferenceProvider.AttackColliderSensor.ObjectExitedSensor -= OnObjectExitedAttackColliderSensor;

			SetAggroTarget(null, false);
		}

		private void OnObjectEnteredAggroColliderSensor(Collider obj) {
			SetAggroTarget(obj.transform, true);
			Aggro();
		}

		private void OnObjectExitedAggroColliderSensor(Collider obj) {
			_target = null;
			SetAggroTarget(null, false);
		}

		private void OnObjectEnteredAttackColliderSensor(Collider obj) {
			Attack();
		}

		private void OnObjectExitedAttackColliderSensor(Collider obj) { }

		protected override void AggroTargetDied() {
			base.AggroTargetDied();
			_brainSwitcher.SwitchToBrain(0);
		}
	}
}
