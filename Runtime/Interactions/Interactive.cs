namespace RyanMillerGameCore.Interactions {
	using System;
	using UnityEngine;
	using UnityEngine.Events;
	using Character;
	using RyanMillerGameCore.UI;

	public class Interactive : MonoBehaviour, IInteractive {

		public event Action<bool> WasSelected;

		public UnityEvent OnInteract;
		public UnityEvent OnSelected;
		public UnityEvent OnDeselected;

		private void Awake() {
			gameObject.layer = LayerMask.NameToLayer("Interactive");
		}

		public virtual void Interact(ICharacter character) {
			OnInteract?.Invoke();
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

		public void InteractionWasCompleted(MonoBehaviour sender) {
			InteractionComplete?.Invoke();
		}

		public event Action InteractionComplete;
	}

	public interface IInteractive {
		bool enabled { get; set; }
		GameObject gameObject { get; }
		Transform transform { get; }
		event Action<bool> WasSelected;
		void Interact(ICharacter character);
		void SetSelected(bool active);
		event Action InteractionComplete;
	}
}
