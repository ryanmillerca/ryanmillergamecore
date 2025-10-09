namespace RyanMillerGameCore.Camera {
	using UnityEngine;
	using Character;
	using Interactions;
	using System.Collections;

	public class CameraController : Singleton<CameraController> {
		[Header("References")]
		[SerializeField] private AudioListener audioListener;
		[SerializeField] private Transform defaultTarget;

		[Header("Position")]
		[SerializeField] private Vector3 cameraOffset;
		[SerializeField] private float smoothingSpeedPosition = 3;
		[SerializeField] private float smoothingLeadSpace = 1;
		[SerializeField] private float leadSpace = 1;

		[Header("Rotation")]
		[SerializeField] private bool canRotateCamera = true;
		[SerializeField] private float smoothingSpeedRotation = 3;
		[SerializeField] private float xRotationStrength = -50f;
		[SerializeField] private float yRotationStrength = 100;
		[SerializeField] private float minXRotation = 20;
		[SerializeField] private float maxXRotation = 90;
		[SerializeField] private float minYRotation = -180;
		[SerializeField] private float maxYRotation = 180;
		[SerializeField] private float mouseRotateSensitivity = 0.5f;

		[Header("Zoom")]
		[SerializeField] private bool canZoomCamera = true;
		[SerializeField] private float smoothingSpeedZoom = 2;
		[SerializeField] private float zoomStrength = 2;
		[SerializeField] private float minZoomDistance = -30;
		[SerializeField] private float maxZoomDistance = -10;

		private Camera _cameraComponent;
		private Transform _cameraTransform;
		private Rigidbody _targetRigidbody;
		private Transform _target;
		private float _targetXRotation;
		private float _targetYRotation;
		private Vector3 _currentLeadSpace;
		private Vector2 _lookInputValue;
		private Vector2 _smoothedLookInput;
		private float _currentZoomDistance;
		private float _targetZoomDistance;
		private bool _readyToTrack;
		private bool _moveAudioListener;

		[ContextMenu("Snap to Player")]
		private void SnapToPlayer() {
			var playerChar = FindFirstObjectByType<PlayerCharacter>();
			if (playerChar) {
				transform.position = playerChar.transform.position + cameraOffset;
			}
		}

		private void OnEnable() {
			_readyToTrack = false;
		}

		private void Start() {
			_cameraComponent = GetComponentInChildren<Camera>();
			_cameraTransform = _cameraComponent.transform;

			if (!defaultTarget && !_target) {
				if (CharacterManager.Instance) {
					Target = CharacterManager.Instance.Player.transform;
					defaultTarget = Target;
					_readyToTrack = true;
				}
			}
			else {
				Target = defaultTarget;
				_readyToTrack = true;
			}

			if (canRotateCamera) {
				_targetXRotation = NormalizeAngle(transform.localEulerAngles.x);
				_targetYRotation = NormalizeAngle(transform.localEulerAngles.y);
			}
			_currentZoomDistance = _cameraTransform.localPosition.z;
			_targetZoomDistance = _currentZoomDistance;
			_moveAudioListener = audioListener != null && audioListener.transform != _cameraTransform;
		}

		public float TargetYRotation {
			get { return _targetYRotation; }
			set { _targetYRotation = value; }
		}

		public Transform Target {
			get {
				return _target;
			}
			set {
				_target = value;
				var rb = _target.GetComponent<Rigidbody>();
				if (rb) {
					_targetRigidbody = rb;
				}
			}
		}

		public void SetTemporaryCameraTarget(Transform newTarget, float yRotationOffset) {
			_target = newTarget;
			_targetYRotation = newTarget.forward.y + yRotationOffset;
		}

		public void RestoreCameraTarget() {
			_target = defaultTarget;
		}

		public void SetLookInput(Vector3 newLookInput) {
			_lookInputValue = newLookInput;
		}

		public void SetZoomInput(float newZoomInput) {
			if (canZoomCamera) {
				_targetZoomDistance += newZoomInput * zoomStrength * Time.deltaTime;
				_targetZoomDistance = Mathf.Clamp(_targetZoomDistance, minZoomDistance, maxZoomDistance);
			}
		}

		private void UpdateCameraLocalPosition() {
			if (canZoomCamera) {
				Vector3 cameraLocalPos = _cameraTransform.localPosition;
				_currentZoomDistance = Mathf.Lerp(cameraLocalPos.z, _targetZoomDistance,
					Time.deltaTime * smoothingSpeedZoom);
				_cameraTransform.localPosition = new Vector3(0, 0, _currentZoomDistance);
			}
		}

		private void UpdateCameraWorldPosition() {
			if (!_readyToTrack) {
				return;
			}

			if (leadSpace > 0) {
				CalculateLeadSpace();
			}

			if (!_target) {
				_readyToTrack = false;
				return;
			}

			transform.position = Vector3.Lerp(transform.position, _target.position + _currentLeadSpace + cameraOffset,
				Time.deltaTime * smoothingSpeedPosition);

			if (_moveAudioListener) {
				audioListener.transform.position = _target.position;
			}
		}

		private void UpdateCameraRotation() {
			if (canRotateCamera == false) {
				return;
			}

			// update the input value to be a smoothed version for an inertia effect
			_smoothedLookInput = _lookInputValue * mouseRotateSensitivity;

			// modify the rotation target based on input
			_targetYRotation += _smoothedLookInput.x * yRotationStrength * Time.deltaTime;
			_targetXRotation += _smoothedLookInput.y * xRotationStrength * Time.deltaTime;

			// limit the rotations, if that's what we want
			if (minYRotation != 0 || maxYRotation != 0) {
				_targetYRotation = Mathf.Clamp(_targetYRotation, minYRotation, maxYRotation);
			}
			if (minXRotation != 0 || maxXRotation != 0) {
				_targetXRotation = Mathf.Clamp(_targetXRotation, minXRotation, maxXRotation);
			}

			// Target rotation as a Quaternion
			Quaternion targetRotation = Quaternion.Euler(_targetXRotation, _targetYRotation, 0);

			// Smoothly interpolate from current to target rotation
			transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothingSpeedRotation * Time.deltaTime);
		}

		private void Update() {
			UpdateCameraLocalPosition();
			UpdateCameraWorldPosition();
			UpdateCameraRotation();
		}

		private void CalculateLeadSpace() {
			if (_targetRigidbody) {
				Vector3 targetLeadSpace = Vector3.ClampMagnitude(_targetRigidbody.linearVelocity, 1) * leadSpace;
				_currentLeadSpace = Vector3.Lerp(_currentLeadSpace, targetLeadSpace, Time.deltaTime * smoothingLeadSpace);
			}
		}

		private float NormalizeAngle(float angle) {
			angle %= 360;
			if (angle > 180) angle -= 360;
			else if (angle < -180) angle += 360;
			return angle;
		}
	}
}
