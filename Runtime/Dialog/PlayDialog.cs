namespace RyanMillerGameCore.Dialog {
	using System;
	using System.Collections;
	using Interactions;
	using UnityEngine;
	using UnityEngine.Events;

	public class PlayDialog : MonoBehaviour {

		public UnityEvent DialogCompleteEvent {
			get { return dialogCompleteEvent; }
			set { dialogCompleteEvent = value; }
		}
		[SerializeField] private DialogContent content;
		[SerializeField] private TriggerDialogOn triggerDialogOn = TriggerDialogOn.Interact;
		[SerializeField] private bool triggerOnlyOnce = true;
		[SerializeField] private bool disableOnComplete = true;
		[SerializeField] private UnityEvent dialogCompleteEvent;
		[SerializeField] private float cooldownTrigger = 1;
		[SerializeField] private DialogStyle dialogStyle = DialogStyle.BottomBox;

		public event Action DialogComplete;

		private DialogPlayer m_dialogPlayer;
		private Interactive m_interactive;
		private bool m_triggered;
		private bool m_subscribed;
		private float m_cooldownTime;

		private DialogPlayer DialogPlayer {
			get {
				if (m_dialogPlayer != null) {
					return m_dialogPlayer;
				}
				if (DialogManager.Instance) {
					m_dialogPlayer = DialogManager.Instance.GetDialogPlayer(dialogStyle);
				}
				return m_dialogPlayer;
			}
		}

		private IEnumerator Start() {
			if (triggerDialogOn == TriggerDialogOn.Start) {
				yield return new WaitForEndOfFrame();
				TriggerDialog();
			}
		}

		public void TriggerDialog() {
			if (m_triggered) {
				return;
			}
			if (Time.unscaledTime < m_cooldownTime) {
				return;
			}
			m_cooldownTime = Time.unscaledTime + cooldownTrigger;
			if (content == null) {
				Debug.LogError("DialogTrigger can't be played because dialog content is missing.", gameObject);
				return;
			}
			if (DialogPlayer == null) {
				Debug.LogError("DialogTrigger can't be played because DialogPlayer is missing.", gameObject);
				return;
			}

			bool playDialogSuccess = DialogPlayer.PlayDialog(content);
			if (playDialogSuccess == false) {
				return;
			}
			if (m_interactive) {
				m_interactive.SetSelected(false);
			}
			m_triggered = true;
			m_subscribed = true;
			DialogPlayer.DialogComplete += OnDialogComplete;
		}

		private void OnEnable() {
			if (triggerDialogOn == TriggerDialogOn.Interact) {
				m_interactive = GetComponent<Interactive>();
				if (m_interactive == null) {
					m_interactive = gameObject.AddComponent<Interactive>();
				}
				if (m_interactive.OnInteract == null) {
					m_interactive.OnInteract = new UnityEvent();
				}
				m_interactive.OnInteract.AddListener(TriggerDialog);
			}
		}

		private void OnDisable() {
			if (m_subscribed && DialogPlayer) {
				DialogPlayer.DialogComplete -= OnDialogComplete;
				m_subscribed = false;
			}
			if (m_interactive != null) {
				m_interactive.OnInteract.RemoveListener(TriggerDialog);
			}
		}

		private void OnDialogComplete() {
			DialogComplete?.Invoke();
			dialogCompleteEvent?.Invoke();
			if (m_subscribed) {
				DialogPlayer.DialogComplete -= OnDialogComplete;
				m_subscribed = false;
			}
			if (disableOnComplete) {
				enabled = false;
				if (m_interactive) {
					m_interactive.enabled = false;
				}
			}
			if (m_interactive) {
				if (m_interactive) {
					m_interactive.InteractionWasCompleted(this);
				}
			}
			if (triggerOnlyOnce == false) {
				m_triggered = false;
			}
		}
	}

	public enum TriggerDialogOn {
		None = 0,
		Start = 1,
		Interact = 2
	}
}
