namespace RyanMillerGameCore.Dialog {
	using UnityEngine;
	using System;
	using Interactions;
	using UnityEngine.Events;

	public class SequencedDialog : MonoBehaviour, IInteractive {
		[Tooltip("Leave empty to use child objects")]
		[SerializeField] private PlayDialog[] dialogs;
		[SerializeField] private int currentDialogIndex = 0;
		[SerializeField] private bool repeatLastDialog = true;
		public UnityEvent OnInteract;
		public UnityEvent OnSelected;
		public UnityEvent OnDeselected;
		public event Action<bool> WasSelected;
		public event Action InteractionComplete;

		private void Start() {
			gameObject.layer = LayerMask.NameToLayer("Interactive");
			if (dialogs.Length == 0) {
				dialogs = GetComponentsInChildren<PlayDialog>();
			}
			SetCurrentInteraction(currentDialogIndex);
		}

		private void SetCurrentInteraction(int index) {
			if (!repeatLastDialog) {
				if (index >= dialogs.Length) {
					this.enabled = false;
					return;
				}
			}
			index = Mathf.Clamp(index, 0, dialogs.Length - 1);
			dialogs[currentDialogIndex].DialogCompleteEvent.RemoveListener(OnDialogComplete);
			currentDialogIndex = index;
			for (int i = 0; i < dialogs.Length; i++) {
				if (i == currentDialogIndex) {
					dialogs[i].gameObject.SetActive(true);
				}
				else {
					dialogs[i].gameObject.SetActive(false);
				}
			}
			dialogs[currentDialogIndex].DialogCompleteEvent.AddListener(OnDialogComplete);
		}

		private void OnDialogComplete() {
			SetCurrentInteraction(currentDialogIndex + 1);
			InteractionComplete?.Invoke();
		}

		private void OnDisable() {
			dialogs[currentDialogIndex].DialogComplete -= OnDialogComplete;
		}


		public virtual void Interact(Character.Character character) {
			OnInteract?.Invoke();
			dialogs[currentDialogIndex].TriggerDialog();
		}

		public void SetSelected(bool active) {
			if (active) {
				OnSelected?.Invoke();
			}
			else {
				OnDeselected?.Invoke();
			}
			WasSelected?.Invoke(active);
		}
	}
}
