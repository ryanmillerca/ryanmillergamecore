namespace RyanMillerGameCore.TurnBasedCombat.UI {
	using System.Collections;
	using TMPro;
	using UnityEngine;

	/// <summary>
	/// Small TextMeshPro readout for a single Combatant.
	/// Attach to a UI element (TextMeshProUGUI) or to the combatant GameObject.
	/// Auto-assigns Combatant via GetComponent&lt;Combatant&gt;() if available.
	/// </summary>
	[DisallowMultipleComponent]
	public class CombatantReadoutTMPro : MonoBehaviour {
		[Header("References")]
		[Tooltip("The Combatant to observe. If left empty, a Combatant on this GameObject will be used.")]
		[SerializeField] private Combatant m_Combatant;

		[Tooltip("The TextMeshProUGUI label to update.")]
		[SerializeField] private TextMeshProUGUI m_Label;

		[Header("Display Options")]
		[Tooltip("Show Attack / Defense / Speed under the name")]
		[SerializeField] private bool showStats = true;

		[Tooltip("Show a short event message line when Combatant raises events")]
		[SerializeField] private bool showEventMessages = true;

		[Tooltip("How long the event message remains visible (seconds).")]
		[SerializeField] private float messageDuration = 3f;

		// internal event message
		private string lastEventMessage = "";
		private float lastEventExpiresAt = 0f;
		private Coroutine eventClearCoroutine;

		private void Reset() {
			// try to auto assign label (convenience) if left empty
			if (m_Label == null)
				m_Label = GetComponentInChildren<TextMeshProUGUI>();
		}

		private void Awake() {
			// prefer an assigned Combatant, otherwise try to get one on this GameObject
			if (m_Combatant == null)
				m_Combatant = GetComponent<Combatant>();

			if (m_Label == null)
				Debug.LogWarning($"{nameof(CombatantReadoutTMPro)} on '{gameObject.name}' has no TextMeshProUGUI assigned.");

			// initial update (if we have a combatant)
			if (m_Combatant != null)
				RefreshLabel();
		}

		private void OnEnable() {
			if (m_Combatant != null) {
				m_Combatant.CombatantEvent += OnCombatantEvent;
				RefreshLabel();
			}
		}

		private void OnDisable() {
			if (m_Combatant != null) {
				m_Combatant.CombatantEvent -= OnCombatantEvent;
			}

			if (eventClearCoroutine != null) {
				StopCoroutine(eventClearCoroutine);
				eventClearCoroutine = null;
			}
		}

		/// <summary>
		/// External callers can assign a Combatant at runtime.
		/// </summary>
		public void SetCombatant(Combatant c) {
			if (m_Combatant != null) {
				m_Combatant.CombatantEvent -= OnCombatantEvent;
			}

			m_Combatant = c;

			if (m_Combatant != null) {
				m_Combatant.CombatantEvent += OnCombatantEvent;
			}

			RefreshLabel();
		}

		/// <summary>
		/// Called whenever the observed Combatant raises a CombatantEvent.
		/// You indicated eventData (and similar) are non-nullable, so this method assumes valid data.
		/// </summary>
		private void OnCombatantEvent(CombatantEventData eventData) {
			// build a short event text to display beneath the main info (if enabled)
			if (showEventMessages) {
				lastEventMessage = TruncateForLabel(eventData.Message, 120);
				lastEventExpiresAt = Time.time + messageDuration;

				// kick off coroutine to clear after duration if not already running
				if (eventClearCoroutine == null)
					eventClearCoroutine = StartCoroutine(ClearEventMessageAfterDelay());
			}

			// refresh main readout right away to reflect HP / status changes
			RefreshLabel();
		}

		private IEnumerator ClearEventMessageAfterDelay() {
			while (Time.time < lastEventExpiresAt) {
				yield return null;
			}
			lastEventMessage = "";
			eventClearCoroutine = null;
			RefreshLabel();
		}

		/// <summary>
		/// Compose the label text from combatant data and current transient event message.
		/// Uses TMP rich text color tags for the name using Combatant.ColorAsHex.
		/// </summary>
		private void RefreshLabel() {
			if (m_Label == null || m_Combatant == null) return;

			// name (colored)
			string nameColor = m_Combatant.ColorAsHex;
			string nameLine = $"<color={nameColor}><b>{m_Combatant.CombatantName}</b></color>";

			// HP line
			int cur = m_Combatant.CurrentHp;
			int max = m_Combatant.MaxHp;
			int pct = (max > 0) ? Mathf.RoundToInt((float)cur / max * 100f) : 0;
			string hpLine = $"HP: {cur}/{max} ({pct}%)";

			// status / alive
			string aliveMarker = m_Combatant.isAlive ? "" : " <color=#FF3333><b>DEAD</b></color>";

			// stats (optional)
			string statsLine = "";
			if (showStats) {
				statsLine = $"ATK {m_Combatant.Attack} • DEF {m_Combatant.Defense} • SPD {m_Combatant.Speed}";
			}

			// event message (optional)
			string eventLine = "";/*
			if (showEventMessages && !string.IsNullOrEmpty(lastEventMessage)) {
				// slightly dim the event line
				eventLine = $"\n<small><i>{lastEventMessage}</i></small>";
			}*/

			// Compose final text. Keep compact for small labels.
			// Example:
			// [Name (colored)]
			// HP: 75/100 (75%) [DEAD?]
			// ATK 20 • DEF 10 • SPD 8
			string composed =
				$"{nameLine}\n{hpLine}{aliveMarker}" +
				(string.IsNullOrEmpty(statsLine) ? "" : $"\n{statsLine}") +
				eventLine;

			m_Label.text = composed;
		}

		// small helper to avoid excessive length in a small UI label
		private string TruncateForLabel(string input, int maxLen) {
			if (string.IsNullOrEmpty(input)) return "";
			if (input.Length <= maxLen) return input;
			return input.Substring(0, maxLen - 1) + "…";
		}

#if UNITY_EDITOR
		// Editor-only helper so changes to the combatant in inspector update the label in edit mode
		private void OnValidate() {
			if (!Application.isPlaying && m_Label != null && m_Combatant != null) {
				RefreshLabel();
			}
		}
#endif
	}
}
