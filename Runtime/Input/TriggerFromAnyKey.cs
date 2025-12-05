namespace RyanMillerGameCore.Input {
	using UnityEngine.InputSystem;
	using UnityEngine;
	using System;
	using UnityEngine.Events;
	using UnityEngine.InputSystem.Utilities;

	public class TriggerFromAnyKey : MonoBehaviour {

		[SerializeField] private InputActionReference[] m_InputActionReferences;
		[SerializeField] private UnityEvent AnyKeyPressed;
		[SerializeField] private bool m_TriggerOnlyOnce = false;
		private IDisposable m_EventListener;
		private bool m_DidTrigger = false;


		private void OnEnable() {
			m_EventListener = InputSystem.onAnyButtonPress.Call(OnButtonPressed);
			if (m_InputActionReferences.Length > 0) {
				foreach (var inputActionReference in m_InputActionReferences) {
					inputActionReference.action.Enable();
					inputActionReference.action.performed += OnButtonPressed;
				}
			}
		}

		private void OnDisable() {
			m_EventListener.Dispose();
			foreach (var inputActionReference in m_InputActionReferences) {
				inputActionReference.action.Disable();
				inputActionReference.action.performed -= OnButtonPressed;
			}
		}

		public void OnButtonPressed(InputAction.CallbackContext context) {
			if (m_TriggerOnlyOnce) {
				if (m_DidTrigger) {
					return;
				}
			}
			AnyKeyPressed?.Invoke();
			m_DidTrigger = true;
		}

		private void OnButtonPressed(InputControl button) {
			if (m_TriggerOnlyOnce) {
				if (m_DidTrigger) {
					return;
				}
			}
			AnyKeyPressed?.Invoke();
			m_DidTrigger = true;
		}
	}
}
