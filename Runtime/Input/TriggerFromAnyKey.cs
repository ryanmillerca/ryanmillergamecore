namespace RyanMillerGameCore.Input {
	using UnityEngine.InputSystem;
	using UnityEngine;
	using System;
	using UnityEngine.Events;
	using UnityEngine.InputSystem.Utilities;

	public class TriggerFromAnyKey : MonoBehaviour {

		[SerializeField] private UnityEvent AnyKeyPressed;
		[SerializeField] private bool m_TriggerOnlyOnce = false;
		private IDisposable m_EventListener;
		private bool m_DidTrigger = false;


		private void OnEnable() {
			m_EventListener = InputSystem.onAnyButtonPress.Call(OnButtonPressed);
		}

		private void OnDisable() {
			m_EventListener.Dispose();
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
