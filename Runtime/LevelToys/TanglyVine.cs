namespace RyanMillerGameCore.LevelToys {
	using UnityEngine;
	using UnityEngine.Events;
	using Interactions;

	public class TanglyVine : MonoBehaviour {
		[SerializeField] private ColliderSensor colliderSensor;
		[SerializeField, Range(0.1f, 10f)] private float maxBreakDistance = 2f;
		[SerializeField] private UnityEvent OnSpringBreak; 

		#region Public Properties

		private SpringJoint SpringJoint {
			get {
				m_SpringJoint = m_SpringJoint ?? GetComponentInChildren<SpringJoint>();
				return m_SpringJoint;
			}
		}

		#endregion


		#region MonoBehaviour

		public void OnEnable() {
			if (colliderSensor != null) {
				colliderSensor.ObjectEnteredSensor -= OnObjectEnteredColliderSensor;
				colliderSensor.ObjectEnteredSensor += OnObjectEnteredColliderSensor;
			}
		}

		public void OnDisable() {
			if (colliderSensor != null) {
				colliderSensor.ObjectEnteredSensor -= OnObjectEnteredColliderSensor;
			}
		}

		private void Update() {
			if (this.SpringJoint.connectedBody != null) {
				Vector3 connectedAnchor = this.SpringJoint.connectedBody.transform.position;
				float distance = Vector3.Distance(transform.position, connectedAnchor);
				Debug.Log($"TanglyVine: distance between anchors: {distance}");

				if (distance >= maxBreakDistance) {
					Debug.Log($"TanglyVine: {this.SpringJoint.connectedBody.name} broke the spring");
					this.SpringJoint.connectedBody = null;
					this.OnSpringBreak?.Invoke();
				}
			}
		}

		#endregion


		#region Fields

		private SpringJoint m_SpringJoint;

		#endregion


		#region Private Methods

		private void OnObjectEnteredColliderSensor(Collider other) {
			if (this.SpringJoint.connectedBody != null) {
				Debug.Log($"TanglyVine: {other.name} already connected");
				return;
			}

			Debug.Log($"TanglyVine: {other.name} entered sensor");

			this.SpringJoint.connectedBody = other.attachedRigidbody;
			this.SpringJoint.breakForce = Mathf.Infinity;
		}

		#endregion


		#region Editor Helper

		private void OnDrawGizmos() {
			Gizmos.color = Color.green;

			if (this.SpringJoint != null && this.SpringJoint.connectedBody != null) {
				Vector3 connectedAnchor = this.SpringJoint.connectedBody.transform.position;

				Gizmos.DrawSphere(transform.position, 0.05f);
				Gizmos.DrawSphere(connectedAnchor, 0.05f);
				Gizmos.DrawLine(transform.position, connectedAnchor);
			}
			else {
				Gizmos.DrawSphere(transform.position, 0.05f);
			}
		}

		#endregion


	}
}
