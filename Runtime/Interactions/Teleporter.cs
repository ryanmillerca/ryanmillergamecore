namespace RyanMillerGameCore.Interactions {
	using Character;
	using UnityEngine;

	/// <summary>
	/// Teleports objects to a transform or specified position/rotation
	/// has special handling for players to use their CharacterMovement.Teleport
	/// </summary>
	public class Teleporter : MonoBehaviour {

		public Transform m_TeleportToTransform;

		[Header("Optional")]
		[Tooltip("Can leave blank if teleportToTransform is set")]
		public Vector3 m_TeleportToPosition;
		[Tooltip("Can leave blank if teleportToTransform is set")]
		public Quaternion m_TeleportToRotation;
		[Tooltip("Leave blank if using TriggerTeleport(Collider)")]
		public Transform m_TeleportThis;

		private Vector3 teleportPosition {
			get {
				if (m_TeleportToTransform) {
					return m_TeleportToTransform.position;
				}
				return m_TeleportToPosition;
			}
		}

		private Quaternion teleportRotation {
			get {
				if (m_TeleportToTransform) {
					return m_TeleportToTransform.rotation;
				}
				return m_TeleportToRotation;
			}
		}

		public void TriggerTeleport() {
			TeleportThisToThere(m_TeleportThis);
		}

		public void TriggerTeleport(Collider c) {
			TeleportThisToThere(c.transform);
		}

		private void TeleportThisToThere(Transform t) {
			Character character = t.GetComponent<Character>();
			if (character) {
				character.characterMovement.Teleport(teleportPosition, teleportRotation);
			}
		}
	}
}
