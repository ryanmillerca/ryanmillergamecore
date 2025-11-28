namespace RyanMillerGameCore.UI {
	using UnityEngine;
	using System;
	using UnityEngine.InputSystem;

	public class ButtomPromptFromInputActionReference : MonoBehaviour {

		[SerializeField] private InputActionPromptPair[] m_Pairs = new InputActionPromptPair[] { };
		private PerPlatformPrompt m_perPlatformPrompt;

		private void OnEnable() {
			m_perPlatformPrompt = FindFirstObjectByType<PerPlatformPrompt>();
		}

		public Sprite GetSpriteForInputActionReference(InputActionReference inputActionReference) {
			Sprite sprite = null;
			foreach (var inputActionPromptPair in m_Pairs) {
				if (inputActionPromptPair.inputActionReference.Equals(inputActionReference)) {
					sprite = m_perPlatformPrompt.GetSpriteFor(inputActionPromptPair.promptAction);
				}
			}
			return sprite;
		}
	}

	[Serializable]
	public class InputActionPromptPair {
		public InputActionReference inputActionReference;
		public PromptAction promptAction;
	}
}
