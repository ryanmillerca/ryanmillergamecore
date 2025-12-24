namespace RyanMillerGameCore.LevelToys {
	using System;
	using Random = UnityEngine.Random;
	using Character;
	using UnityEngine;
	using UnityEngine.Events;
	using Items;
	using Utilities;

	public class Breakable : MonoBehaviour, ITakesDamage {
		[SerializeField] private UnityEvent m_OnBroken;

        #pragma warning disable CS0067
		public event Action Died;
		public event Action Spawned;

		[SerializeField] private bool m_IsBroken = false;
		[SerializeField] private Rigidbody m_Rigidbody;
		[SerializeField] private Collider m_Collider;
		[SerializeField] private float m_MaxDurability = 1;
		[SerializeField] private Rigidbody[] m_PassOnForceToThese;
		[SerializeField] private bool m_UnparentForceTakers = true;
		[SerializeField] private float m_RandomAngularForce = 100;
		[SerializeField] private WeightedIDTable m_DropTable;

		private EnableDisabler m_enableDisabler;
		private float m_currentDurability;
		private Vector3 m_storedVelocity;

		private void Awake() {
			m_enableDisabler = GetComponent<EnableDisabler>();
			if (m_enableDisabler) {
				m_enableDisabler.Completed += EnableDisablerCompleted;
			}
			m_currentDurability = m_MaxDurability;
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Collider = GetComponent<Collider>();
		}

		private void OnDestroy() {
			if (m_enableDisabler) {
				m_enableDisabler.Completed -= EnableDisablerCompleted;
			}
		}

		private void EnableDisablerCompleted() {
			if (m_UnparentForceTakers) {
				foreach (Rigidbody rb in m_PassOnForceToThese) {
					rb.transform.SetParent(null);
				}
			}
			foreach (Rigidbody rb in m_PassOnForceToThese) {
				rb.AddForce(m_storedVelocity, ForceMode.Impulse);
			}
		}

		public bool CanReceiveDamage() {
			return !m_IsBroken;
		}

		public bool ReceiveDamage(float damageAmount, Component attacker = null) {
			if (m_IsBroken) {
				return false;
			}
			m_currentDurability -= damageAmount;
			if (m_currentDurability <= 0) {
				Break();
			}

			return true;
		}

		private void Break() {
			m_IsBroken = true;

			if (m_Collider) {
				m_Collider.enabled = false;
			}
			if (m_Rigidbody) {
				m_Rigidbody.isKinematic = true;
				m_Rigidbody.useGravity = false;
			}

			m_OnBroken?.Invoke();

			DropItem();

		}

		private void DropItem() {
			ID dropID = null;
			if (m_DropTable != null) {
				dropID = m_DropTable.GetRandomID();
			}
			if (dropID != null && ItemManager.Instance) {
				GameObject item = ItemManager.Instance.GetItem(dropID);
				if (item) {
					item.transform.position = transform.position;
					item.transform.rotation = Quaternion.identity;
				}
			}
		}

		public bool ReceiveKnockback(Vector3 direction) {
			m_storedVelocity = direction;
			if (m_Rigidbody) {
				m_Rigidbody.AddForce(direction, ForceMode.Impulse);
				return true;
			}
			if (m_RandomAngularForce > 0) {
				m_Rigidbody.AddTorque(Random.Range(-m_RandomAngularForce, m_RandomAngularForce),
					Random.Range(-m_RandomAngularForce, m_RandomAngularForce),
					Random.Range(-m_RandomAngularForce, m_RandomAngularForce), ForceMode.Impulse);
			}
			return false;
		}
	}
}
