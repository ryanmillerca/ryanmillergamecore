namespace RyanMillerGameCore.UI {
	using UnityEngine;
	using UnityEngine.UI;
#if UNITY_EDITOR
	using UnityEditor;
#endif

	public class ShowPlatformButton : MonoBehaviour {
		[SerializeField] private PromptAction m_PromptAction;
		public PromptAction PromptAction => m_PromptAction;
		private PerPlatformPrompt m_perPlatformPrompt;
		private Image m_imageComponent;
		private SpriteRenderer m_spriteRendererComponent;

		private void Awake() {
			m_imageComponent = GetComponent<Image>();
			m_spriteRendererComponent = GetComponent<SpriteRenderer>();
		}

		private void OnEnable() {
			m_perPlatformPrompt = FindFirstObjectByType<PerPlatformPrompt>();
			m_perPlatformPrompt.OnPromptGraphicsChanged += OnPromptGraphicsChanged;
			OnPromptGraphicsChanged(null);
		}

		private void OnDisable() {
			if (m_perPlatformPrompt) {
				m_perPlatformPrompt.OnPromptGraphicsChanged -= OnPromptGraphicsChanged;
			}
		}

		private void OnPromptGraphicsChanged(PromptGraphics obj) {
			UpdateSprite();
		}

		private void UpdateSprite() {
			if (m_perPlatformPrompt) {
				Sprite sprite = m_perPlatformPrompt.GetSpriteFor(m_PromptAction);
				if (m_imageComponent) {
					m_imageComponent.sprite = sprite;
				}
				if (m_spriteRendererComponent) {
					m_spriteRendererComponent.sprite = sprite;
				}
			}
		}
	}
}
