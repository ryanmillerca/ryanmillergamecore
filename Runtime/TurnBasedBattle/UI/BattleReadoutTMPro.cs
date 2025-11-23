namespace RyanMillerGameCore.TurnBasedCombat.UI {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;
	using TurnBasedCombat;

	/// <summary>
	/// Subscribe to BattleManager events and show a rolling single-line/multi-line readout in a TextMeshProUGUI label.
	/// Preserves color tags produced by the existing DebugBattleListener style messages.
	/// </summary>
	[DisallowMultipleComponent]
	public class BattleReadoutTMPro : MonoBehaviour {
		[Header("References")]
		[SerializeField] private BattleManager m_BattleManager;
		[SerializeField] private TextMeshProUGUI m_Label;

		[Header("Behavior")]
		[Tooltip("Maximum number of lines kept in the readout.")]
		[SerializeField] private int maxLines = 6;

		[Tooltip("How long each message stays visible (seconds).")]
		[SerializeField] private float messageDuration = 6f;

		[Tooltip("If true, also log events to Debug.Log for development.")]
		[SerializeField] private bool debugLogAlso = false;

		// internal message store with expiration time
		private readonly List<(string text, float expireAt)> messages = new List<(string, float)>();
		private Coroutine expiryCoroutine;

		private void Reset() {
			// try auto-assign label if found in scene (convenience)
			if (m_Label == null)
				m_Label = GetComponent<TextMeshProUGUI>();
		}

		private void OnEnable() {
			if (m_BattleManager == null)
				m_BattleManager = FindFirstObjectByType<BattleManager>();

			if (m_BattleManager != null) {
				m_BattleManager.BattleEvent += OnBattleEvent;
				m_BattleManager.TurnEvent += OnTurnEvent;
				m_BattleManager.MoveResolved += OnMoveResolved;
				m_BattleManager.BattleEnded += OnBattleEnded;
				m_BattleManager.PlayerInputRequired += OnPlayerInputRequired;
				m_BattleManager.PlayerInputReceived += OnPlayerInputReceived;
			}
		}

		private void OnDisable() {
			if (m_BattleManager == null) return;

			m_BattleManager.BattleEvent -= OnBattleEvent;
			m_BattleManager.TurnEvent -= OnTurnEvent;
			m_BattleManager.MoveResolved -= OnMoveResolved;
			m_BattleManager.BattleEnded -= OnBattleEnded;
			m_BattleManager.PlayerInputRequired -= OnPlayerInputRequired;
			m_BattleManager.PlayerInputReceived -= OnPlayerInputReceived;

			if (expiryCoroutine != null) StopCoroutine(expiryCoroutine);
			messages.Clear();
			RefreshLabel();
		}


		#region Event handlers (mirror DebugBattleListener messages)

		private void OnPlayerInputRequired(PlayerInputData inputData) {
			string colorTag = $"<color={inputData.Actor?.ColorAsHex}>";
			string colorClose = inputData.Actor != null ? "</color>" : "";
			string moves = inputData.AvailableMoves != null ? string.Join(", ", inputData.AvailableMoves.Select(m => m.ActionName)) : "—";
			string targets = inputData.ValidTargets != null ? string.Join(", ", inputData.ValidTargets.Select(t => t.CombatantName)) : "—";

			AddMessage($"{colorTag}PLAYER INPUT REQUIRED: {inputData.Actor?.CombatantName}{colorClose}");
			AddMessage($"{colorTag}Moves: {moves}{colorClose}");
			AddMessage($"{colorTag}Targets: {targets}{colorClose}");
		}

		private void OnPlayerInputReceived(PlayerInputResponse response) {
			if (response.IsValid) {
				string colorTag = "<color=#00FF00>";
				string colorClose = "</color>";
				AddMessage($"{colorTag}PLAYER SELECTED: {response.SelectedAction.ActionName} -> {response.SelectedTarget.CombatantName}{colorClose}");
			}
			else {
				string colorTag = "<color=#FF0000>";
				string colorClose = "</color>";
				AddMessage($"{colorTag}INVALID PLAYER INPUT: {response.ValidationMessage}{colorClose}");
			}
		}

		private void OnBattleEvent(BattleEventData eventData) {
			string colorTag = eventData.Combatant != null ? $"<color={eventData.Combatant.ColorAsHex}>" : "";
			string colorClose = eventData.Combatant != null ? "</color>" : "";

			string message = eventData.Message ?? "[no message]";
			switch (eventData.EventType) {
				case BattleEventType.BattleStarted:
					AddMessage($"BATTLE STARTED: {message}");
					break;
				case BattleEventType.BattleEnded:
					AddMessage($"BATTLE ENDED: {message}");
					break;
				case BattleEventType.TurnSkipped:
				case BattleEventType.NoValidTargets:
				case BattleEventType.NoMovesAvailable:
					AddMessage($"{colorTag}{message}{colorClose}");
					break;
				case BattleEventType.CommandError:
				case BattleEventType.ResolutionError:
				case BattleEventType.EventHandlerError:
					AddMessage($"<color=#FF3333>{message}</color>");
					break;
				case BattleEventType.BattleEndConditionMet:
					AddMessage($"{colorTag}{message}{colorClose}");
					break;
				case BattleEventType.TurnOrderUpdated:
					AddMessage(message);
					break;
				case BattleEventType.TargetChanged:
					AddMessage($"{colorTag}{message}{colorClose}");
					break;
				default:
					AddMessage(message);
					break;
			}
		}

		private void OnTurnEvent(TurnEventData eventData) {
			if (eventData.Combatant == null) return;

			string colorTag = $"<color={eventData.Combatant.ColorAsHex}>";
			string colorClose = "</color>";

			switch (eventData.EventType) {
				case TurnEventType.TurnStarted:
					string brainInfo = eventData.Combatant.AIBrain ? $" ({eventData.Combatant.AIBrain.GetType().Name})" : "";
					AddMessage($"{colorTag}It's {eventData.Combatant.CombatantName}'s turn!{brainInfo}{colorClose}");
					break;
				case TurnEventType.ActionSelected:
					string aiNote = eventData.Combatant.Team == Team.Enemy ? " [AI]" : "";
					AddMessage($"{colorTag}{eventData.Combatant.CombatantName} selects {eventData.Action.ActionName} targeting {eventData.Target.CombatantName}{aiNote}{colorClose}");
					break;
				case TurnEventType.MultiTurnStarted:
					AddMessage($"{colorTag}{eventData.Combatant.CombatantName} starts charging {eventData.Action.ActionName} for {eventData.Action.TurnCost} turns!{colorClose}");
					break;
				case TurnEventType.TurnEnded:
					AddMessage($"{colorTag}{eventData.Combatant.CombatantName}'s turn ended{colorClose}");
					break;
			}
		}

		private void OnMoveResolved(BattleResult result) {
			if (result == null) {
				AddMessage("<color=#FF0000> BattleResult null!</color>");
				return;
			}

			string colorTag = $"<color={result.Actor?.ColorAsHex}>";
			string colorClose = result.Actor != null ? "</color>" : "";

			if (result.Missed) {
				AddMessage($"{colorTag} {result.Message ?? "Missed."}{colorClose}");
			}
			else if (result.DamageDealt > 0) {
				string critTag = result.CriticalHit ? "CRIT " : "";
				string critInfo = result.CriticalHit ? $"[Crit Chance: {result.CriticalChance:P1}]" : "";
				string targetName = result.Target != null ? result.Target.CombatantName : "DEAD TARGET";
				string safeMessage = result.Message ?? $"{result.Actor.CombatantName} used attack on {targetName} for {result.DamageDealt} damage.";
				AddMessage($"{colorTag}{critTag}{safeMessage} {critInfo}{colorClose}");
			}
			else if (result.HealingDone > 0) {
				string targetName = result.Target != null ? result.Target.CombatantName : "SELF";
				string safeMessage = result.Message ?? $"{result.Actor.CombatantName} used heal on {targetName} for {result.HealingDone} healing.";
				AddMessage($"{colorTag}{safeMessage}{colorClose}");
			}
			else {
				string safeMessage = result.Message ?? $"{result.Actor.CombatantName} used {result.BattleAction?.ActionName ?? "UNKNOWN ACTION"}.";
				AddMessage($"{colorTag}{safeMessage}{colorClose}");
			}
		}

		private void OnBattleEnded(BattleOutcome outcome) {
			switch (outcome) {
				case BattleOutcome.Victory:
					AddMessage("<b>VICTORY!</b> You (Player) win!");
					break;
				case BattleOutcome.Defeat:
					AddMessage("<b>DEFEAT!</b> Enemies win!");
					break;
				case BattleOutcome.Undefined:
					AddMessage("Battle ended with undefined outcome");
					break;
			}
		}

		private void OnCombatantEvent(CombatantEventData eventData) {
			if (eventData.Combatant == null) return;
			string colorTag = $"<color={eventData.Combatant.ColorAsHex}>";
			string colorClose = "</color>";
			AddMessage($"{colorTag}{eventData.Message}{colorClose}");
		}

		#endregion


		#region Message management

		/// <summary> Add a new message to the readout (handles trimming and expiry) </summary>
		private void AddMessage(string text) {
			if (string.IsNullOrWhiteSpace(text)) return;

			if (debugLogAlso) Debug.Log($"[BattleReadout] {text}");

			messages.Add((text, Time.time + messageDuration));

			// ensure we don't exceed maxLines by removing oldest now
			while (messages.Count > maxLines)
				messages.RemoveAt(0);

			RefreshLabel();

			if (expiryCoroutine == null)
				expiryCoroutine = StartCoroutine(MessageExpiryRoutine());
		}

		private IEnumerator MessageExpiryRoutine() {
			while (messages.Count > 0) {
				float now = Time.time;
				bool removedAny = false;

				// remove any expired
				for (int i = messages.Count - 1; i >= 0; i--) {
					if (messages[i].expireAt <= now) {
						messages.RemoveAt(i);
						removedAny = true;
					}
				}

				if (removedAny)
					RefreshLabel();

				// stop if no messages remain
				if (messages.Count == 0) break;

				yield return new WaitForSeconds(Mathf.Min(0.5f, messageDuration * 0.2f));
			}

			expiryCoroutine = null;
		}

		private void RefreshLabel() {
			if (m_Label == null) return;

			// show newest messages last (so they appear at bottom)
			var lines = messages.Select(m => m.text).ToArray();
			m_Label.text = string.Join("\n", lines);
		}

		#endregion


	}
}
