namespace RyanMillerGameCore.Camera {
	using System;
	using UnityEngine;
	using UnityEngine.InputSystem;

	public class CameraInput : MonoBehaviour {

		[SerializeField] private InputActionReference inputActionLook;
		[SerializeField] private InputActionReference inputActionZoom;
		[NonSerialized] private CameraController cameraController;
		[NonSerialized] private InputAction lookAction;
		[NonSerialized] private InputAction zoomAction;

		private Vector2 cachedLookInput = Vector2.zero;
		private float cachedZoomInput;

		private void OnEnable() {
			if (inputActionLook != null) {
				inputActionLook.action.Enable();
				inputActionLook.action.performed += OnLook;
				inputActionLook.action.canceled += OnLook;
				inputActionLook.action.Enable();
			}
			if (inputActionZoom != null) {
				inputActionZoom.action.performed += OnZoom;
				inputActionZoom.action.canceled += OnZoom;
				inputActionZoom.action.Enable();
			}
		}

		private void OnDisable() {
			if (inputActionLook != null) {
				inputActionLook.action.Disable();
				inputActionLook.action.performed -= OnLook;
			}
			if (inputActionZoom != null) {
				inputActionZoom.action.Disable();
				inputActionZoom.action.performed -= OnZoom;
			}
		}

		private void Start() {
			cameraController = GetComponent<CameraController>();
		}

		private void Update() {
			if (cameraController != null) {
				cameraController.SetLookInput(cachedLookInput);
				cameraController.SetZoomInput(cachedZoomInput);
			}
		}

		private void OnLook(InputAction.CallbackContext context) {
			cachedLookInput = context.ReadValue<Vector2>();
		}

		private void OnZoom(InputAction.CallbackContext context) {
			cachedZoomInput = context.ReadValue<float>();
		}
	}
}
