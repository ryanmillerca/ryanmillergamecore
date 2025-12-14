namespace RyanMillerGameCore.Interactions {
	using UnityEngine;
	using UI;

	public class DisplayButtonPrompt : MonoBehaviour {


		#region Setup

		public void Setup(PromptAction promptAction) {
			this._promptAction = promptAction;
		}

		#endregion


		[SerializeField] private Vector3 promptOffset = new Vector3(0f, 0.5f, 0f);
		[SerializeField] private PromptAction _promptAction;

		private IInteractive m_interactive;
		private Collider m_targetCollider;
		private PromptData m_promptData;
		private Vector3 m_transformOffset;

		private void Awake() {
			m_targetCollider = GetComponent<Collider>();
			m_transformOffset = GetOffset();
			m_promptData = new PromptData {
				targetTransform = transform,
				transformOffset = m_transformOffset,
				promptAction = _promptAction
			};
		}

		private void OnEnable() {
			m_interactive = gameObject.GetComponent<IInteractive>() ?? gameObject.AddComponent<Interactive>();
			m_interactive.WasSelected += InteractiveOnWasSelected;
		}

		private void InteractiveOnWasSelected(bool obj) {
			if (obj) {
				ShowPrompt();
			}
			else {
				HidePrompt();
			}
		}

		private void OnDisable() {
			if (UIButtonPrompt.Instance) {
				UIButtonPrompt.Instance.TryHidePrompt(m_promptData);
			}
			if (m_interactive != null) {
				m_interactive.WasSelected -= InteractiveOnWasSelected;
			}
		}

		private void HidePrompt() {
			if (UIButtonPrompt.Instance) {
				UIButtonPrompt.Instance.TryHidePrompt(m_promptData);
			}
			else {
				Debug.LogError("You need to have a UIButtonPrompt object in the scene.");
			}
		}

		private void ShowPrompt() {
			if (UIButtonPrompt.Instance) {
				UIButtonPrompt.Instance.TryDisplayPrompt(m_promptData);
			}
			else {
				Debug.LogError("You need to have a UIButtonPrompt object in the scene.");
			}
		}

		private Vector3 GetOffset() {
			Vector3 offset = promptOffset;
			if (m_targetCollider) {
				offset.y += m_targetCollider.bounds.extents.y;
			}
			return offset;
		}
	}
}
