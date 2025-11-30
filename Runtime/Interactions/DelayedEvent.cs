namespace RyanMillerGameCore.Interactions {

	using System.Collections;
	using UnityEngine.Events;
	using UnityEngine;

	public class DelayedEvent : MonoBehaviour {

		[SerializeField] private UnityEvent m_BeforeDelayEvent;
		[SerializeField] private UnityEvent m_AfterDelayEvent;
		[SerializeField] private float m_DelayDuration = 0.5f;
		[SerializeField] private bool m_PreventInterruption = false;
		[SerializeField] private bool m_UseUnscaledTime = false;
		private bool isRunning = false;

		public void TriggerEvent() {
			if (isRunning && m_PreventInterruption) {
				return;
			}
			StartCoroutine(DelayedEventCoroutine());
		}

		IEnumerator DelayedEventCoroutine() {

			m_BeforeDelayEvent.Invoke();
			if (m_UseUnscaledTime) {
				yield return new WaitForSecondsRealtime(m_DelayDuration);
			}
			else {
				yield return new WaitForSeconds(m_DelayDuration);
			}
			m_AfterDelayEvent.Invoke();
		}
	}
}
